#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class TupleMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Enumerate))]
        public string EnumerateTranslator(string? valsParam)
        {
            return $"std::enumerate({valsParam})";
        }

        [MethodName(nameof(EdgeQL.JsonObjectUnpack))]
        public string JsonObjectUnpackTranslator(string? objParam)
        {
            return $"std::json_object_unpack({objParam})";
        }

        [MethodName(nameof(EdgeQL.GetVersion))]
        public string GetVersionTranslator()
        {
            return $"sys::get_version()";
        }

    }
}
