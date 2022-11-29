using EdgeDB.Binary;
using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Benchmarks.Utils
{
    internal class MockQueryClient : EdgeDBBinaryClient
    {
        public static readonly byte[] AuthenticationFirst;
        public static readonly byte[] AuthenticationSecond;
        public static readonly byte[] AuthenticationThird;
        public static readonly byte[] AuthenticationOK;
        public static readonly byte[] ServerKeyData;
        public static readonly byte[] StateDataDescription;
        public static readonly byte[] ParameterStatusOne;
        public static readonly byte[] ParameterStatusTwo;
        public static readonly byte[] ReadyForCommand;
        public static readonly byte[] CommandDataDescription;
        public static readonly byte[] Data;
        public static readonly byte[] CommandComplete;

        internal override IBinaryDuplexer Duplexer
            => _duplexer;

        private readonly StreamDuplexer _duplexer;

        static MockQueryClient()
        {
            AuthenticationFirst = HexConverter.FromHex("520000001D0000000A000000010000000D534352414D2D5348412D323536");
            AuthenticationSecond = HexConverter.FromHex("52000000600000000B00000054723D724958336652764272684D34354D4F424D522F586D737264676E5A57457937334E785873646E445147747058797A4B562C733D69754A5A6E73682B504B31753546686B5A75665553773D3D2C693D34303936");
            AuthenticationThird = HexConverter.FromHex("520000003A0000000C0000002E763D744D4442614B732B664C4F4B79434838777159626150564362656A786D702F674B4B5A617A7437425A6F513D");
            AuthenticationOK = HexConverter.FromHex("520000000800000000");
            ServerKeyData = HexConverter.FromHex("4B000000240000000000000000000000000000000000000000000000000000000000000000");
            StateDataDescription = HexConverter.FromHex("7300000227E6D9537B10BBF8885269EA9A798124530000020F020000000000000000000000000000010104CF9DCE3617F0354F0925678E57A1843200020000000006C652F3F1DDE700613F077C3D260BFB7400010001FFFFFFFF07D7AA058C4E6511EDA4CD436078FBAE0C00020000000B416C77617973416C6C6F770000000A4E65766572416C6C6F770200000000000000000000000000000109020000000000000000000000000000010E08B5508C64E22B26AB824F0705CAB3E6240006000000006F0000000E616C6C6F775F626172655F64646C0003000000006F00000016616C6C6F775F646D6C5F696E5F66756E6374696F6E730004000000006F00000017616C6C6F775F757365725F7370656369666965645F69640004000000006F000000156170706C795F6163636573735F706F6C69636965730004000000006F0000001771756572795F657865637574696F6E5F74696D656F75740005000000006F0000002073657373696F6E5F69646C655F7472616E73616374696F6E5F74696D656F75740005020000000000000000000000000000010008185C1272B4D95557661F6B15078FE4FE0001000000006F0000001864656661756C743A3A63757272656E745F757365725F6964000708E6D9537B10BBF8885269EA9A798124530004000000006F000000066D6F64756C650000000000006F00000007616C69617365730002000000006F00000006636F6E6669670006000000006F00000007676C6F62616C730008");
            ParameterStatusOne = HexConverter.FromHex("53000000290000001A7375676765737465645F706F6F6C5F636F6E63757272656E637900000003313030");
            ParameterStatusTwo = HexConverter.FromHex("53000000C60000000D73797374656D5F636F6E666967000000AD00000071271B1C53DF6F2188ACBF79FD6DD4EE600200000000000000000000000000000100020000000000000000000000000000010E01271B1C53DF6F2188ACBF79FD6DD4EE6000020000000141000000026964000000000000410000001473657373696F6E5F69646C655F74696D656F75740001000000340000000200000B8600000010172097A439F411E9B1899321EB2F4B97000040590000001000000000039387000000000000000000");
            ReadyForCommand = HexConverter.FromHex("5A00000007000049");
            CommandDataDescription = HexConverter.FromHex("54000000480000000000000000000041000000000000000000000000000000000000000000000000000000000000000000000101000000110200000000000000000000000000000101");
            Data = HexConverter.FromHex("440000001700010000000D48656C6C6F2C20576F726C6421");
            CommandComplete = HexConverter.FromHex("430000002C000000000000000000000000000653454C4543540000000000000000000000000000000000000000");
        }

        public MockQueryClient(EdgeDBConnection connection, EdgeDBConfig clientConfig, IDisposable clientPoolHolder, ulong? clientId = null)
            : base(connection, clientConfig, clientPoolHolder, clientId)
        {

            _duplexer = new StreamDuplexer(this);
        }

        private class MockQueryClientStream : Stream
        {
            public override bool CanRead => true;

            public override bool CanSeek => true;

            public override bool CanWrite => true;

            public override long Length => 0;

            public override long Position { get => 0; set { return; } }

            private byte[]? _nextBuffer;
            private bool _trigger = false;
            private int _pos;

            public override void Flush() { }
            public override int Read(byte[] buffer, int offset, int count)
            {
                try
                {
                    buffer = _nextBuffer![_pos..(_pos + count)];
                    _pos += count;
                    return count;
                }
                catch(Exception x)
                {
                    return 0;
                }
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
            {
                try
                {
                    var data = _nextBuffer![_pos..(_pos + buffer.Length)];
                    data.CopyTo(buffer);
                    _pos += buffer.Length;
                    return ValueTask.FromResult(buffer.Length);
                }
                catch(Exception x)
                {
                    return ValueTask.FromResult(0);
                }
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
            public override void SetLength(long value) => throw new NotImplementedException();
            public override void Write(byte[] buffer, int offset, int count)
            {
                var type = (ClientMessageTypes)buffer[0];

                _nextBuffer = type switch
                {
                    ClientMessageTypes.ClientHandshake => AuthenticationFirst,
                    ClientMessageTypes.AuthenticationSASLInitialResponse => AuthenticationSecond,
                    ClientMessageTypes.AuthenticationSASLResponse => AuthenticationThird
                                                .Concat(AuthenticationOK)
                                                .Concat(ServerKeyData)
                                                .Concat(StateDataDescription)
                                                .Concat(ParameterStatusOne)
                                                .Concat(ParameterStatusTwo)
                                                .Concat(ReadyForCommand)
                                                .ToArray(),
                    ClientMessageTypes.Parse => CommandDataDescription
                                                .Concat(ReadyForCommand).ToArray(),
                    ClientMessageTypes.Execute => Data
                                                .Concat(CommandComplete)
                                                .Concat(ReadyForCommand).ToArray(),
                    ClientMessageTypes.Terminate => Array.Empty<byte>(),
                    _ => throw new Exception($"unknown message type {type}"),
                };
                _pos = 0;
                _trigger = true;
            }
        }

        public override bool IsConnected => _con;
        private bool _con;
        protected override ValueTask CloseStreamAsync(CancellationToken token = default(CancellationToken)) => ValueTask.CompletedTask;
        protected override ValueTask<Stream> GetStreamAsync(CancellationToken token = default(CancellationToken))
        {
            _con = true;
            return ValueTask.FromResult<Stream>(new MockQueryClientStream());
        }
    }
}
