#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EdgeDB.Translators
{
    internal partial class StdInt64MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Len))]
        public void LenTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter strParam)
        {
            writer.Function("std::len", debug: null, metadata: new FunctionMetadata("std::len", method), strParam);
        }

        [MethodName(nameof(EdgeQL.Sum))]
        public void SumTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam)
        {
            writer.Function("std::sum", debug: null, metadata: new FunctionMetadata("std::sum", method), sParam);
        }

        [MethodName(nameof(EdgeQL.Count))]
        public void CountTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam)
        {
            writer.Function("std::count", debug: null, metadata: new FunctionMetadata("std::count", method), sParam);
        }

        [MethodName(nameof(EdgeQL.Round))]
        public void RoundTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valParam)
        {
            writer.Function("std::round", debug: null, metadata: new FunctionMetadata("std::round", method), valParam);
        }

        [MethodName(nameof(EdgeQL.Find))]
        public void FindTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter haystackParam, TranslatedParameter needleParam)
        {
            writer.Function("std::find", debug: null, metadata: new FunctionMetadata("std::find", method), haystackParam, needleParam);
        }

        [MethodName(nameof(EdgeQL.BitAnd))]
        public void BitAndTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bit_and", debug: null, metadata: new FunctionMetadata("std::bit_and", method), lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BitOr))]
        public void BitOrTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bit_or", debug: null, metadata: new FunctionMetadata("std::bit_or", method), lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BitXor))]
        public void BitXorTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bit_xor", debug: null, metadata: new FunctionMetadata("std::bit_xor", method), lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BitNot))]
        public void BitNotTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter rParam)
        {
            writer.Function("std::bit_not", debug: null, metadata: new FunctionMetadata("std::bit_not", method), rParam);
        }

        [MethodName(nameof(EdgeQL.BitRshift))]
        public void BitRshiftTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valParam, TranslatedParameter nParam)
        {
            writer.Function("std::bit_rshift", debug: null, metadata: new FunctionMetadata("std::bit_rshift", method), valParam, nParam);
        }

        [MethodName(nameof(EdgeQL.BitLshift))]
        public void BitLshiftTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valParam, TranslatedParameter nParam)
        {
            writer.Function("std::bit_lshift", debug: null, metadata: new FunctionMetadata("std::bit_lshift", method), valParam, nParam);
        }

        [MethodName(nameof(EdgeQL.BytesGetBit))]
        public void BytesGetBitTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter bytesParam, TranslatedParameter numParam)
        {
            writer.Function("std::bytes_get_bit", debug: null, metadata: new FunctionMetadata("std::bytes_get_bit", method), bytesParam, numParam);
        }

        [MethodName(nameof(EdgeQL.RangeUnpack))]
        public void RangeUnpackTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter valParam)
        {
            writer.Function("std::range_unpack", debug: null, metadata: new FunctionMetadata("std::range_unpack", method), valParam);
        }

        [MethodName(nameof(EdgeQL.SequenceReset))]
        public void SequenceResetTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter seqParam, TranslatedParameter valueParam)
        {
            writer.Function("std::sequence_reset", debug: null, metadata: new FunctionMetadata("std::sequence_reset", method), seqParam, valueParam);
        }

        [MethodName(nameof(EdgeQL.ToInt64))]
        public void ToInt64Translator(QueryWriter writer, MethodInfo method, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_int64", debug: null, metadata: new FunctionMetadata("std::to_int64", method), sParam, OptionalArg(fmtParam));
        }

        [MethodName(nameof(EdgeQL.SequenceNext))]
        public void SequenceNextTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter seqParam)
        {
            writer.Function("std::sequence_next", debug: null, metadata: new FunctionMetadata("std::sequence_next", method), seqParam);
        }

        [MethodName(nameof(EdgeQL.Ceil))]
        public void CeilTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter xParam)
        {
            writer.Function("math::ceil", debug: null, metadata: new FunctionMetadata("math::ceil", method), xParam);
        }

        [MethodName(nameof(EdgeQL.Floor))]
        public void FloorTranslator(QueryWriter writer, MethodInfo method, TranslatedParameter xParam)
        {
            writer.Function("math::floor", debug: null, metadata: new FunctionMetadata("math::floor", method), xParam);
        }

    }
}
