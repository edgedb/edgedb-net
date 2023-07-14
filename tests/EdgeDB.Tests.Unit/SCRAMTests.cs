using EdgeDB.Binary;
using EdgeDB.Binary.Packets;
using EdgeDB.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Unit
{
    [TestClass]
    public class SCRAMTests
    {
        private static readonly EdgeDBBinaryClient _client = new EdgeDBTcpClient(new(), new(), null!);

        public const string SCRAM_METHOD = "SCRAM-SHA-256";
        public const string SCRAM_USERNAME = "user";
        public const string SCRAM_PASSWORD = "pencil";
        public const string SCRAM_CLIENT_NONCE = "rOprNGfwEbeRWgbNEkqO";
        public const string SCRAM_SERVER_NONCE = "rOprNGfwEbeRWgbNEkqO%hvYDpWUa2RaTCAfuxFIlj)hNlF$k0";
        public const string SCRAM_SALT = "W22ZaJ0SNY7soEsUEjb6gQ==";

        [TestMethod]
        public void TestSCRAM()
        {
            var scram = new Scram(Convert.FromBase64String(SCRAM_CLIENT_NONCE));

            var clientFirst = scram.BuildInitialMessagePacket(_client, SCRAM_USERNAME, SCRAM_METHOD);
            Assert.AreEqual($"{SCRAM_METHOD} n,,n={SCRAM_USERNAME},r={SCRAM_CLIENT_NONCE}", clientFirst.ToString());

            var serverFirst = CreateServerFirstMessage();
            var clientFinal = scram.BuildFinalMessagePacket(_client, in serverFirst, SCRAM_PASSWORD);

            Assert.AreEqual("6rriTRBi23WpRR/wtup+mMhUZUn/dB5nLTJRsjl95G4=", Convert.ToBase64String(clientFinal.ExpectedSig));
            Assert.AreEqual($"c=biws,r={SCRAM_SERVER_NONCE},p=dHzbZapWIk4jUhN+Ute9ytag9zjfMHgsqmmiz7AndVQ=", clientFinal.FinalMessage.ToString());
        }

        private static AuthenticationStatus CreateServerFirstMessage()
        {
            var inner = Encoding.UTF8.GetBytes($"r={SCRAM_SERVER_NONCE},s={SCRAM_SALT},i=4096");
            var data = new byte[] { 0x00, 0x00, 0x00, 0x0b }.Concat(BitConverter.GetBytes(inner.Length).Reverse()).Concat(inner).ToArray();
            var reader = new PacketReader(data);
            return new AuthenticationStatus(ref reader);
        }
    }
}
