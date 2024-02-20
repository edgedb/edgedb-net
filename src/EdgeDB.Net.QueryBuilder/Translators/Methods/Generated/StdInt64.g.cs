#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using EdgeDB.Translators.Methods;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdInt64MethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Len))]
        public void LenTranslator(QueryWriter writer, TranslatedParameter strParam)
        {
            writer.Function("std::len", strParam);
        }

        [MethodName(nameof(EdgeQL.Sum))]
        public void SumTranslator(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::sum", sParam);
        }

        [MethodName(nameof(EdgeQL.Count))]
        public void CountTranslator(QueryWriter writer, TranslatedParameter sParam)
        {
            writer.Function("std::count", sParam);
        }

        [MethodName(nameof(EdgeQL.Round))]
        public void RoundTranslator(QueryWriter writer, TranslatedParameter valParam)
        {
            writer.Function("std::round", valParam);
        }

        [MethodName(nameof(EdgeQL.Find))]
        public void FindTranslator(QueryWriter writer, TranslatedParameter haystackParam, TranslatedParameter needleParam)
        {
            writer.Function("std::find", haystackParam, needleParam);
        }

        [MethodName(nameof(EdgeQL.BitAnd))]
        public void BitAndTranslator(QueryWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bit_and", lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BitOr))]
        public void BitOrTranslator(QueryWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bit_or", lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BitXor))]
        public void BitXorTranslator(QueryWriter writer, TranslatedParameter lParam, TranslatedParameter rParam)
        {
            writer.Function("std::bit_xor", lParam, rParam);
        }

        [MethodName(nameof(EdgeQL.BitNot))]
        public void BitNotTranslator(QueryWriter writer, TranslatedParameter rParam)
        {
            writer.Function("std::bit_not", rParam);
        }

        [MethodName(nameof(EdgeQL.BitRshift))]
        public void BitRshiftTranslator(QueryWriter writer, TranslatedParameter valParam, TranslatedParameter nParam)
        {
            writer.Function("std::bit_rshift", valParam, nParam);
        }

        [MethodName(nameof(EdgeQL.BitLshift))]
        public void BitLshiftTranslator(QueryWriter writer, TranslatedParameter valParam, TranslatedParameter nParam)
        {
            writer.Function("std::bit_lshift", valParam, nParam);
        }

        [MethodName(nameof(EdgeQL.BytesGetBit))]
        public void BytesGetBitTranslator(QueryWriter writer, TranslatedParameter bytesParam, TranslatedParameter numParam)
        {
            writer.Function("std::bytes_get_bit", bytesParam, numParam);
        }

        [MethodName(nameof(EdgeQL.RangeUnpack))]
        public void RangeUnpackTranslator(QueryWriter writer, TranslatedParameter valParam)
        {
            writer.Function("std::range_unpack", valParam);
        }

        [MethodName(nameof(EdgeQL.SequenceReset))]
        public void SequenceResetTranslator(QueryWriter writer, TranslatedParameter seqParam, TranslatedParameter valueParam)
        {
            writer.Function("std::sequence_reset", seqParam, valueParam);
        }

        [MethodName(nameof(EdgeQL.ToInt64))]
        public void ToInt64Translator(QueryWriter writer, TranslatedParameter sParam, TranslatedParameter? fmtParam)
        {
            writer.Function("std::to_int64", sParam, OptionalArg(fmtParam));
        }

        [MethodName(nameof(EdgeQL.SequenceNext))]
        public void SequenceNextTranslator(QueryWriter writer, TranslatedParameter seqParam)
        {
            writer.Function("std::sequence_next", seqParam);
        }

        [MethodName(nameof(EdgeQL.Ceil))]
        public void CeilTranslator(QueryWriter writer, TranslatedParameter xParam)
        {
            writer.Function("math::ceil", xParam);
        }

        [MethodName(nameof(EdgeQL.Floor))]
        public void FloorTranslator(QueryWriter writer, TranslatedParameter xParam)
        {
            writer.Function("math::floor", xParam);
        }

    }
}
