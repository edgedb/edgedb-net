namespace EdgeDB.Translators.Methods;

/// <summary>
///     Represents a translator for translating methods within the <see cref="string" /> class.
/// </summary>
internal class StringMethodTranslators : MethodTranslator<string>
{
    /// <summary>
    ///     Translates the method <see cref="string.Concat(object?, object?)" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string to concat against.</param>
    /// <param name="variableArgs">The variable arguments that should be concatenated together.</param>
    [MethodName(nameof(string.Concat))]
    public void Concat(QueryStringWriter writer, TranslatedParameter? instance,
        params TranslatedParameter[] variableArgs)
    {
        if (variableArgs.Length == 0)
            throw new ArgumentException("At least 1 parameter is required for concatenation");

        if (instance is not null)
        {
            writer
                .Append(instance)
                .Append(" ++ ");
        }

        for (var i = 0; i != variableArgs.Length;)
        {
            writer.Append(variableArgs[i++]);

            if (i != variableArgs.Length)
                writer.Append(" ++ ");
        }
    }

    /// <summary>
    ///     Translates the method <see cref="string.Contains(string)" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string to concat against.</param>
    /// <param name="target">The value to check whether or not its within the instance</param>
    [MethodName(nameof(string.Contains))]
    public void Contains(QueryStringWriter writer, TranslatedParameter instance, TranslatedParameter target)
        => writer.Function("contains", instance, target);

    /// <summary>
    ///     Translates the method <see cref="string.IndexOf(string)" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <param name="target">The target substring to find within the instance.</param>
    [MethodName(nameof(string.IndexOf))]
    public void Find(QueryStringWriter writer, TranslatedParameter instance, TranslatedParameter target)
        => writer.Function("find", instance, target);

    /// <summary>
    ///     Translates the method <see cref="string.ToLower()" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <returns>The EdgeQL equivalent of the method.</returns>
    [MethodName(nameof(string.ToLower))]
    [MethodName(nameof(string.ToLowerInvariant))]
    public void ToLower(QueryStringWriter writer, TranslatedParameter instance)
        => writer.Function("str_lower", instance);

    /// <summary>
    ///     Translates the method <see cref="string.ToUpper()" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <returns>The EdgeQL equivalent of the method.</returns>
    [MethodName(nameof(string.ToUpper))]
    [MethodName(nameof(string.ToUpperInvariant))]
    public void ToUpper(QueryStringWriter writer, TranslatedParameter instance)
        => writer.Function("str_upper", instance);

    /// <summary>
    ///     Translates the method <see cref="string.PadLeft(int)" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <param name="amount">The amount to pad left</param>
    /// <param name="fill">The fill character to pad with</param>
    /// <returns>The EdgeQL equivalent of the method.</returns>
    [MethodName(nameof(string.PadLeft))]
    public void PadLeft(QueryStringWriter writer, TranslatedParameter instance, TranslatedParameter amount,
        TranslatedParameter? fill)
        => writer.Function(
            "str_pad_start",
            instance,
            amount,
            OptionalArg(fill)
        );

    /// <summary>
    ///     Translates the method <see cref="string.PadRight(int)" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <param name="amount">The amount to pad left</param>
    /// <param name="fill">The fill character to pad with</param>
    /// <returns>The EdgeQL equivalent of the method.</returns>
    [MethodName(nameof(string.PadRight))]
    public void PadRight(QueryStringWriter writer, TranslatedParameter instance, TranslatedParameter amount,
        TranslatedParameter? fill)
        => writer.Function("str_pad_end", instance, amount, OptionalArg(fill));

    /// <summary>
    ///     Translates the method <see cref="string.Trim()" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <param name="trimChars">The characters to trim.</param>
    /// <returns>The EdgeQL equivalent of the method.</returns>
    [MethodName(nameof(string.Trim))]
    public void Trim(QueryStringWriter writer, TranslatedParameter instance,
        params TranslatedParameter[]? trimChars)
    {
        if (trimChars is not null && trimChars.Any())
        {
            writer
                .Function(
                    "str_trim",
                    instance,
                    writer.Val(writer => writer
                        .SingleQuoted(writer.Val(writer =>
                        {
                            foreach (var trimChar in trimChars)
                                writer.Append(trimChar);
                        }))
                    )
                );
            return;
        }

        writer.Function("str_trim", instance);
    }

    /// <summary>
    ///     Translates the method <see cref="string.TrimStart()" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <param name="trimChars">The characters to trim.</param>
    /// <returns>The EdgeQL equivalent of the method.</returns>
    [MethodName(nameof(string.TrimStart))]
    public void TrimStart(QueryStringWriter writer, TranslatedParameter instance,
        params TranslatedParameter[]? trimChars)
    {
        if (trimChars != null && trimChars.Any())
        {
            writer
                .Function(
                    "str_trim_start",
                    instance,
                    writer.Val(writer => writer
                        .SingleQuoted(writer.Val(writer =>
                        {
                            foreach (var trimChar in trimChars)
                                writer.Append(trimChar);
                        }))
                    )
                );
            return;
        }

        writer.Function("str_trim_start", instance);
    }

    /// <summary>
    ///     Translates the method <see cref="string.TrimEnd()" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <param name="trimChars">The characters to trim.</param>
    /// <returns>The EdgeQL equivalent of the method.</returns>
    [MethodName(nameof(string.TrimEnd))]
    public void TrimEnd(QueryStringWriter writer, TranslatedParameter instance,
        params TranslatedParameter[]? trimChars)
    {
        if (trimChars != null && trimChars.Any())
        {
            writer
                .Function(
                    "str_trim_end",
                    instance,
                    writer.Val(writer => writer
                        .SingleQuoted(writer.Val(writer =>
                        {
                            foreach (var trimChar in trimChars)
                                writer.Append(trimChar);
                        }))
                    )
                );
            return;
        }

        writer.Function("str_trim_end", instance);
    }

    /// <summary>
    ///     Translates the method <see cref="string.Replace(char, char)" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <param name="old">The old string to replace.</param>
    /// <param name="newStr">The new string to replace the old one.</param>
    /// <returns>The EdgeQL equivalent of the method.</returns>
    [MethodName(nameof(string.Replace))]
    public void Replace(QueryStringWriter writer, TranslatedParameter instance, TranslatedParameter old,
        TranslatedParameter newStr)
        => writer.Function("str_replace", instance, old, newStr);

    /// <summary>
    ///     Translates the method <see cref="string.Split(char[])" />.
    /// </summary>
    /// <param name="writer">The query string writer to append the translated method to.</param>
    /// <param name="instance">The instance of the string.</param>
    /// <param name="separator">The char to split by.</param>
    /// <returns>The EdgeQL equivalent of the method.</returns>
    [MethodName(nameof(string.Split))]
    public void Split(QueryStringWriter writer, TranslatedParameter instance, TranslatedParameter separator)
        => writer.Function("str_split", instance, separator);
}
