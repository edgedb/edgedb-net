using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EdgeDB.Utils
{
    internal sealed class Scram : IDisposable
    {
        public const int NonceLength = 18;

        private byte[]? _clientNonce;
        private string? _rawFirstMessage;
        private static readonly IScalarCodec<string> _stringCodec;
        private static string SanitizeString(string str) => str.Normalize(NormalizationForm.FormKC);

        static Scram()
        {
            _stringCodec = CodecBuilder.GetScalarCodec<string>()!;
        }

        public Scram(byte[]? clientNonce = null)
        {
            _clientNonce = clientNonce;
        }

        private static byte[] GenerateNonce()
        {
            var bytes = new byte[NonceLength];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }

            return bytes;
        }

        public string BuildInitialMessage(string username)
        {
            _clientNonce ??= GenerateNonce();
            _rawFirstMessage = $"n={SanitizeString(username)},r={Convert.ToBase64String(_clientNonce)}";
            return $"n,,{_rawFirstMessage}";
        }

        public (string final, byte[] expectedSig) BuildFinalMessage(string initialResponse, string password)
        {
            var parsedMessage = ParseServerMessage(initialResponse);

            if (parsedMessage.Count < 3)
            {
                throw new FormatException("Received malformed scram message");
            }

            var salt = Convert.FromBase64String(parsedMessage["s"]);

            if (!int.TryParse(parsedMessage["i"], out var iterations))
            {
                throw new FormatException("Received malformed scram message");
            }

            // build final
            var final = $"c=biws,r={parsedMessage["r"]}";
            var authMsg = Encoding.UTF8.GetBytes($"{_rawFirstMessage},{initialResponse},{final}");

            var saltedPassword = SaltPassword(SanitizeString(password), salt, iterations);
            var clientKey = GetClientKey(saltedPassword);
            var storedKey = Hash(clientKey);
            var clientSig = ComputeHMACHash(storedKey, authMsg);
            var clientProof = XOR(clientKey, clientSig);

            var serverKey = GetServerKey(saltedPassword);
            var serverProof = ComputeHMACHash(serverKey, authMsg);

            return ($"{final},p={Convert.ToBase64String(clientProof)}", serverProof);
        }

        public static byte[] ParseServerSig(string data)
        {
            var parsed = ParseServerMessage(data);
            return Convert.FromBase64String(parsed["v"]);
        }

        private static Dictionary<string, string> ParseServerMessage(string msg)
        {
            var matches = Regex.Matches(msg, @"(.{1})=(.+?)(?>,|$)");

            return matches.ToDictionary(x => x.Groups[1].Value, x => x.Groups[2].Value);
        }

        private static byte[] SaltPassword(string password, byte[] salt, int iterations)
        {
            var pdb = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);

            return pdb.GetBytes(32);
        }

        private static byte[] ComputeHMACHash(byte[] data, string key)
            => ComputeHMACHash(data, Encoding.UTF8.GetBytes(key));

        private static byte[] ComputeHMACHash(byte[] data, byte[] key)
        {
            using var hmac = new HMACSHA256(data);
            return hmac.ComputeHash(key);
        }

        private static byte[] GetClientKey(byte[] password)
            => ComputeHMACHash(password, "Client Key");

        private static byte[] GetServerKey(byte[] password)
            => ComputeHMACHash(password, "Server Key");
        
        private static byte[] Hash(byte[] input)
        {
            using var hs = SHA256.Create();
            return hs.ComputeHash(input);
        }

        private static byte[] XOR(byte[] b1, byte[] b2)
        {
            var length = b1.Length;

            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(b1[i] ^ b2[i]);
            }

            return result;
        }

        public void Dispose()
        {
            _clientNonce = null;
            _rawFirstMessage = null;
        }
    }
}
