using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static EdgeDB.TestGenerator.ValueGenerator;

namespace EdgeDB.TestGenerator.ValueProviders
{
    public interface IValueProvider<T> : IValueProvider
    {
        new T GetRandom(GenerationRuleSet rules);
        string ToEdgeQLFormat(T value);

        string IValueProvider.ToEdgeQLFormat(object value)
        {
            return ToEdgeQLFormat((T)value);
        }

        object IValueProvider.GetRandom(GenerationRuleSet rules) => GetRandom(rules)!;
    }

    public interface IValueProvider
    {
        string EdgeDBName { get; }
        object GetRandom(GenerationRuleSet rules);

        string ToEdgeQLFormat(object value);

        static string GetTrueName(IValueProvider provider)
        {
            return provider is IWrappingValueProvider w ? w.FormatAsGeneric() : provider.EdgeDBName;
        }

        static string GetSmallHash(IValueProvider provider)
        {
            const string chars = "ABCDEFGHIJ";
            using(var hash = MD5.Create())
            {
                return Regex.Replace(
                    HexConverter.ToHex(
                        hash.ComputeHash(
                            Encoding.UTF8.GetBytes(GetTrueName(provider))
                        )
                    )[12..],
                    "(\\d)", d => new string(new char[] { chars[int.Parse(d.Groups[1].Value)] })
                );
            }
        }
    }

    interface IWrappingValueProvider : IValueProvider
    {
        IValueProvider[]? Children { get; set; }

        public string FormatAsGeneric()
        {
            return $"{EdgeDBName}<{string.Join(", ", Children!.Select(x => x is IWrappingValueProvider w ? w.FormatAsGeneric() : x.EdgeDBName))}>";
        }
    }
}
