using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an abstract naming strategy used to convert property names within 
    ///     a dotnet type to a name within a schema file.
    /// </summary>
    public interface INamingStrategy
    {
        /// <summary>
        ///     Gets the default naming strategy. This strategy does not modify property
        ///     names.
        /// </summary>
        public static INamingStrategy DefaultNamingStrategy
            => new DefaultNamingStrategy();

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
        ///     This is the default naming strategy for the <see cref="Binary.TypeBuilder"/>.
        /// </remarks>
        public static INamingStrategy SnakeCaseNamingStrategy
            => new SnakeCaseNamingStrategy();

        /// <summary>
        ///     Converts the <paramref name="property"/>'s name to the desired naming scheme.
        /// </summary>
        /// <param name="property">The property info of which to convert its name.</param>
        /// <returns>The name defined in the schema.</returns>
        public string Convert(PropertyInfo property);

        /// <summary>
        ///     Converts the name to the desired naming scheme.
        /// </summary>
        /// <param name="name">The property name of which to convert its name.</param>
        /// <returns>The name defined in the schema.</returns>
        public string Convert(string name);
    }
}
