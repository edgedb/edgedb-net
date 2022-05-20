using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.BinaryDebugger
{
    public class StreamProxy : Stream
    {
        public override bool CanRead => _target.CanRead;

        public override bool CanSeek => _target.CanSeek;

        public override bool CanWrite => _target.CanWrite;

        public override long Length => _target.Length;

        public override long Position { get => _target.Position; set => _target.Position = value; }

        private readonly Stream _target;
        private readonly Action<int, byte[], int, int> _readCallback;
        private readonly Action<byte[], int, int> _writeCallback;

        public StreamProxy(Stream target, Action<int, byte[], int, int> readCallback, Action<byte[], int, int> writeCallback)
        {
            _target = target;
            _readCallback = readCallback;
            _writeCallback = writeCallback;
        }

        public override void Flush()
        {
            _target.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var r = _target.Read(buffer, offset, count);
            _readCallback(r, buffer, offset, count);
            return r;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var r = await _target.ReadAsync(buffer, offset, count);
            _readCallback(r, buffer, offset, count);
            return r;
        }

        public override long Seek(long offset, SeekOrigin origin)
            => _target.Seek(offset, origin);

        public override void SetLength(long value)
            => _target.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            _target.Write(buffer, offset, count);
            _writeCallback(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _target.WriteAsync(buffer, offset, count, cancellationToken);
            _writeCallback(buffer, offset, count);
        }
    }
}
