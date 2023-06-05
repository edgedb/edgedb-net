#if NET461
namespace EdgeDB
{
    /// <summary>
    ///     A class containing default implementations of <see cref="INamingStrategy"/>.
    /// </summary>
    public static class NamingStrategies
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
        ///     Gets the 'snake_case' naming strategy.
        /// </summary>
        /// <remarks>
        ///     This is the default naming strategy for the <see cref="TypeBuilder"/>.
        /// </remarks>
        public static INamingStrategy SnakeCaseNamingStrategy
            => new SnakeCaseNamingStrategy();
    }
}
#endif
