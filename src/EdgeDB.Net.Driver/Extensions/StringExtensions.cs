#if NET461
namespace EdgeDB
{
    internal static class StringExtensions
    {
        public static string[] Split(this string str, string delimiter)
        {
            return str.Split(delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
#endif
