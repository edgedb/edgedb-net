using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class Tuple : MethodTranslator<ITuple>
    {
        [MethodName(EdgeQL.Enumerate)]
        public string Enumerate(string? valsParam)
        {
            return $"std::enumerate({valsParam})";
        }

        [MethodName(EdgeQL.JsonObjectUnpack)]
        public string JsonObjectUnpack(string? objParam)
        {
            return $"std::json_object_unpack({objParam})";
        }

        [MethodName(EdgeQL.GetVersion)]
        public string GetVersion()
        {
            return $"sys::get_version()";
        }

    }
}
