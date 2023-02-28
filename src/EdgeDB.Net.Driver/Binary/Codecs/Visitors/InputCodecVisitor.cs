using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class InputCodecVisitor : CodecVisitor
    {
        private readonly EdgeDBBinaryClient _client;
        private readonly Stack<IDictionary<string, object?>> _inputFrames;

        public InputCodecVisitor(EdgeDBBinaryClient client, IDictionary<string, object?> input)
        {
            _client = client;
            _inputFrames = new Stack<IDictionary<string, object?>>();
            _inputFrames.Push(input);
        }

        protected override void VisitCodec(ref ICodec codec)
        {
            // codec should be objectcodec or sparseobjectcodec
            if(codec is not IObjectCodec objectCodec)
            {
                throw new NotSupportedException($"Cannot use {codec} as a input codec");
            }

            // since we're only configuring the codecs to match the type of our data
            // we can ignore extra inputs in the data as well as safely skip
            // any codec fields that aren't present in the input data.

            var input = _inputFrames.Pop();
            var innerVisitor = new TypeVisitor(_client);

            for(int i = 0; i != objectCodec.PropertyCodecs.Length; i++)
            {
                // TODO:
                // We cant mutate the objects inner codecs becuase this object
                // codec is cached, mutating it here can also cause race conditions
                // since all clients anywhere share the same cache.
                ref var innerCodec = ref objectCodec.PropertyCodecs[i];
                var name = objectCodec.PropertyNames[i];

                if (!input.TryGetValue(name, out var value))
                    continue;

                // check for other obj codecs, we do this incase of sub-sparse objects.
                // normally, the type visitor would handle this but we need to check for sparce
                // codecs.
                if(innerCodec is IObjectCodec && value is IDictionary<string, object?> v)
                {
                    _inputFrames.Push(v);
                    VisitCodec(ref innerCodec);
                }
                else
                {
                    innerVisitor.SetTargetType(value is null ? typeof(void) : value.GetType());
                    innerVisitor.Visit(ref innerCodec);
                    innerVisitor.Reset();
                }
            } 
        }
    }
}
