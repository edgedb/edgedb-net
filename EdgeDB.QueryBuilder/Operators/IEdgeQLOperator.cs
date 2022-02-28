using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    public interface IEdgeQLOperator
    {
        ExpressionType? Operator { get; }
        string EdgeQLOperator { get; }

        string Build(params object[] args)
        {
            if (Regex.IsMatch(EdgeQLOperator, @".+?\(.*?\)")) 
            {
                // extract arguments
                return Regex.Replace(EdgeQLOperator, @"\((.*?)\)", (rootMatch) =>
                {
                    var param = string.Join(", ", Regex.Matches(rootMatch.Groups[1].Value, @"<(.*?{(\d|\d\?)+?})>|{(\d|\d\?)+?}").Select(m =>
                    {
                        if(!m.Groups[1].Success)
                        {
                            // value is 3rd group

                            if (int.TryParse(m.Groups[3].Value, out var i))
                                return $"{args[i]}";

                            if (Regex.IsMatch(m.Groups[3].Value, @"\d\?"))
                            {
                                i = int.Parse(Regex.Match(m.Groups[3].Value, @"(\d+)").Groups[1].Value);

                                return args.Length > i ? $"{args[i]}" : null;
                            }

                            return null;
                        }
                        else
                        {
                            // should be group1 {group2}
                            var name = m.Groups[1].Value;

                            // extract the {} param

#pragma warning disable CS8603 // Possible null reference return.
                            return Regex.Replace(name, @"{(\d|\d\?|.+?:\d+?\+)+?}", x =>
                            {
                                if (int.TryParse(x.Groups[1].Value, out var i))
                                    return $"{args[i]}";

                                if (Regex.IsMatch(x.Groups[1].Value, @"\d\?"))
                                {
                                    i = int.Parse(Regex.Match(x.Groups[1].Value, @"(\d+)").Groups[1].Value);

                                    return args.Length > i ? $"{args[i]}" : null;
                                }

                                if (Regex.IsMatch(m.Groups[1].Value, @".+?:\d+?\+"))
                                {
                                    var split = m.Groups[1].Value.Split(":");
                                    var indexAfter = int.Parse(Regex.Match(split[1], @"(\d+)").Groups[1].Value);
                                    return $"{(args.Length >= indexAfter ? split[0].Remove(0, 1) : "")}{string.Join(split[0], args.Skip(indexAfter))}";
                                }

                                return null;
                            });
#pragma warning restore CS8603 // Possible null reference return.

                        }
                    }).Where(x => x != null));
                    return $"({param})";
                });
            }
            else
            {
                return Regex.Replace(EdgeQLOperator, @"{(\d|\d\?|.+?:\d+?\+)+?}", (m) =>
                {
                    if (int.TryParse(m.Groups[1].Value, out var i))
                        return $"{args[i]}";

                    if (Regex.IsMatch(m.Groups[1].Value, @"\d\?"))
                    {
                        i = int.Parse(Regex.Match(m.Groups[1].Value, @"(\d+)").Groups[1].Value);

                        return args.Length < i ? $"{args[i]}" : "";
                    }

                    if(Regex.IsMatch(m.Groups[1].Value, @".+?:\d+?\+"))
                    {
                        var split = m.Groups[1].Value.Split(":");
                        var indexAfter = int.Parse(Regex.Match(split[1], @"(\d+)").Groups[1].Value);
                        return $"{(args.Length >= indexAfter ? split[0].Remove(0, 1) : "")}{string.Join(split[0], args.Skip(indexAfter))}";
                    }

                    return "";
                });
            }
        }
    }
}
