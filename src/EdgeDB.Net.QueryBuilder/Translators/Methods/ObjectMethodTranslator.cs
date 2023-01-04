using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Methods
{
    /// <summary>
    ///     Represents a translator for translating methods within the <see cref="object"/> class.
    /// </summary>
    internal class ObjectMethodTranslator : MethodTranslator<object>
    {
        /// <summary>
        ///     Translates the method <see cref="object.ToString"/>.
        /// </summary>
        /// <param name="instance">The instance of the object.</param>
        /// <param name="format">The optional format for the tostring func.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(object.ToString))]
        public string ToStr(string instance, string? format)
            => $"to_str({instance}{OptionalArg(format)})";
    }
}
