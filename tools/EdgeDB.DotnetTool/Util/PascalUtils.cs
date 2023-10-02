using System.Text.RegularExpressions;

namespace EdgeDB.DotnetTool;

internal static class PascalUtils
{
    private static readonly Regex _invalidCharsRgx = new("[^_a-zA-Z0-9]", RegexOptions.Compiled);

    private static readonly Regex _whiteSpace = new(@"(?<=\s)", RegexOptions.Compiled);

    private static readonly Regex _startsWithLowerCaseChar = new("^[a-z]", RegexOptions.Compiled);

    private static readonly Regex _firstCharFollowedByUpperCasesOnly =
        new("(?<=[A-Z])[A-Z0-9]+$", RegexOptions.Compiled);

    private static readonly Regex _lowerCaseNextToNumber = new("(?<=[0-9])[a-z]", RegexOptions.Compiled);

    private static readonly Regex _upperCaseInside =
        new("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))", RegexOptions.Compiled);

    public static string ToPascalCase(string? original)
    {
        if (original == null)
            return "";

        // replace white spaces with undescore, then replace all invalid chars with empty string
        var pascalCase = _invalidCharsRgx.Replace(_whiteSpace.Replace(original, "_"), string.Empty)
            // split by underscores
            .Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries)
            // set first letter to uppercase
            .Select(w => _startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
            // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
            .Select(w => _firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
            // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
            .Select(w => _lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
            // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
            .Select(w => _upperCaseInside.Replace(w, m => m.Value.ToLower()));

        return string.Concat(pascalCase);
    }
}
