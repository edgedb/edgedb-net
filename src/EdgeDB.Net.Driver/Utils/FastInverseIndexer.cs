using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal ref struct FastInverseIndexer
    {
        private readonly int _bits;
        private readonly byte[] _buffer;
        private int _tracked;

        public FastInverseIndexer(int count)
        {
            _tracked = count;
            _bits = count;

            // create a span with 'count' bits
            // since we're dividing by 8, we can shift the number by 3 and then add one if the frist 3 bits are not zero
            var c = count + 0b111;
            var t = (count >> 3) + ((count | c) >> 3);
            _buffer = new byte[t];
        }

        public void Track(int index)
        {
            _tracked--;

            if (_tracked < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Too many items tracked!");

            // get the bit relative to a byte in our span, the and is equivalent to modulus 8
            var b = 1 << (index & 0b111);

            // get the index in our span 
            var i = index >> 3;

            // OR the bit in
            _buffer[i] |= (byte)b;
        }

        public int[] GetIndexies()
        {
            int[] missed = new int[_tracked];
            int p = 0;

            // iterate over the span
            for (int i = 0; i != _buffer.Length; i++)
            {
                // the byte->bit index
                var k = i << 3;

                // inverse the byte at the current index
                var b = ~_buffer[i];

                // iterate over the bits until we've read all 8 or we've read all total bits
                for (int j = 0; j != 8 && _bits > k + j; j++)
                {
                    // if the bit is tracked
                    if ((b & 1) == 1)
                    {
                        // add to our missed array
                        missed[p] = j + k;
                        p++;
                    }

                    // shift the next bit forwards
                    b >>= 1;
                }
            }

            return missed;
        }
    }
}
