using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.BinaryDebugger
{
    internal class DebuggerClient : EdgeDBTcpClient
    {
        public StreamWriter Writer { get; }
        public FileStream FileStream { get; }
        private Stream? _proxy;

        private bool _readingPacket;
        private ServerMessageType _packetType;
        private bool _isReadingBody;
        private int _packetLength;
        private List<byte>? _packetBody; 

        public DebuggerClient(EdgeDBConnection connection, EdgeDBConfig config, ulong? clientId = null) : base(connection, config, clientId)
        {
            if (File.Exists("./debug.log"))
                File.Delete("./debug.log");

            FileStream = File.OpenWrite("./debug.log");
            Writer = new StreamWriter(FileStream, Encoding.UTF8);
        }

        protected override async ValueTask<Stream> GetStreamAsync()
        {
            var stream = await base.GetStreamAsync().ConfigureAwait(false);
            _proxy = new StreamProxy(stream, OnRead, OnWrite);
            return _proxy;
        }

        private void OnRead(int read, byte[] buffer, int offset, int count)
        {
            if (!_readingPacket && count == 1)
            {
                _readingPacket = true;
                _packetType = (ServerMessageType)buffer[0];
                return;
            }

            if (!_isReadingBody && count == 4)
            {
                _isReadingBody = true;
                _packetLength = BitConverter.ToInt32(buffer.Reverse().ToArray());
                _packetBody = new();
                return;

            }

            if (_isReadingBody)
            {
                _packetBody.AddRange(buffer);

                if (_packetBody.Count >= _packetLength - 4)
                {
                    Writer.WriteLine($"READING\n" +
                             $"=======\n" +
                             $"Packet: {_packetType}\n" +
                             $"Length: {_packetLength}\n" +
                             $"Data: {Utils.HexConverter.ToHex(_packetBody.ToArray())}\n" +
                             $"Raw: {_packetType:X}{Utils.HexConverter.ToHex(BitConverter.GetBytes(_packetLength).Reverse().ToArray())}{Utils.HexConverter.ToHex(_packetBody.ToArray())}\n");

                    _isReadingBody = false;
                    _readingPacket = false;
                }
            }
        }

        private void OnWrite(byte[] buffer, int offset, int count)
        {
            Writer.WriteLine($"WRITING\n" +
                             $"=======\n" +
                             $"Buffer: {EdgeDB.Utils.HexConverter.ToHex(buffer)}\n" +
                             $"Length: {count}\n" +
                             $"Offset: {offset}\n");
        }

        protected override ValueTask CloseStreamAsync()
        {
            _proxy?.Close();
            FileStream.Close();

            return ValueTask.CompletedTask;
        }
    }
}
