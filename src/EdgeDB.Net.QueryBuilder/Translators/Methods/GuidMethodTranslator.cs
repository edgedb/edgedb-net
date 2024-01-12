using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Methods
{
    /// <summary>
    ///     Represents a translator for translating methods within the <see cref="Guid"/> struct.
    /// </summary>
    internal class GuidMethodTranslator : MethodTranslator<Guid>
    {
        /// <summary>
        ///     Translates the method <see cref="Guid.NewGuid"/>.
        /// </summary>
        /// <param name="writer">The query string writer to append the translated method to.</param>
        /// <returns>The EdgeQL equivalent of the method.</returns>
        [MethodName(nameof(Guid.NewGuid))]
        public void Generate(QueryStringWriter writer)
            => writer.Function("uuid_generate_v4");
    }
}
