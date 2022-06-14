using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Serializer
{
    /// <summary>
    ///     Represents an abstract naming strategy used to convert property names within 
    ///     a dotnet type to a name within a schema file.
    /// </summary>
    public interface INamingStrategy
    {
        /// <summary>
        ///     Gets the attribute-based naming strategy.
        /// </summary>
        public static INamingStrategy AttributeNamingStrategy
            => new AttributeNamingStrategy();

        /// <summary>
        ///     Gets the 'camelCase' naming strategy.
        /// </summary>
        public static INamingStrategy CamelCaseNamingStrategy
            => new CamelCaseNamingStrategy();

        /// <summary>
        ///     Gets the 'PascalCase' naming strategy.
        /// </summary>
        public static INamingStrategy PascalNamingStrategy
            => new PascalNamingStrategy();

        /// <summary>
        ///     Gets the 'snake-case' naming strategy.
        /// </summary>
        /// <remarks>
        ///     This is the default naming strategy for the <see cref="TypeBuilder"/>.
        /// </remarks>
        public static INamingStrategy SnakeCaseNamingStrategy
            => new SnakeCaseNamingStrategy();

        /// <summary>
        ///     Gets the name defined in the objects schema given the types property info.
        /// </summary>
        /// <param name="property">The property info of which to convert its name.</param>
        /// <returns>The name defined in the schema.</returns>
        public string GetName(PropertyInfo property);
    }
}
