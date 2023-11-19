using System.Text;

namespace EdgeDB.Translators;

/// <summary>
///     Represents a function that runs a translator and appends its result to the provided string builder.
/// </summary>
internal delegate void TranslatorProxy(StringBuilder result);
