#if NET461
using System.Text.RegularExpressions;

namespace EdgeDB
{
    internal static class RegexExtensions
    {
        public static IEnumerable<Match> Iterate(this MatchCollection collection)
        {
            var enumerator = collection.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return (Match)enumerator.Current;
            } 
        }
    }
}
#endif
