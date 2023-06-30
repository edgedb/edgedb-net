#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Translators
{
    internal partial class StdAnyrealMethodTranslator : MethodTranslator<EdgeQL>
    {
        [MethodName(nameof(EdgeQL.Abs))]
        public string AbsTranslator(string? xParam)
        {
            return $"math::abs({xParam})";
        }

    }
}
