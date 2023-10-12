using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace EdgeDB.DocGenerator;

internal class Parser
{
    public static DocMember[] Load(string file)
    {
        var serializer = new XmlSerializer(typeof(doc));
        using var reader = new StreamReader(file);
        var t = (doc)serializer.Deserialize(reader)!;

        var members = t.members!.Select(x => DocMember.FromMember(x)).OrderByDescending(x => x.Type).ToArray();

        // get the edgebd.net assembly
        var edgedbAssembly = Assembly.GetAssembly(typeof(EdgeDBClient))!;

        foreach (var member in members)
        {
            member.Populate(members, edgedbAssembly);
        }

        foreach (var member in members)
        {
            member.Finalize(members);
        }

        return members;
    }
}

public class DocType : DocMember
{
    public DocType(docMember member)
        : base(member)
    {
    }

    public DocType(string name, Assembly assembly)
        : base(MemberType.Type, name)
    {
        Name = name;
        DotnetType = assembly.GetType(name);

        if (DotnetType is null)
            throw new NullReferenceException();
    }

    public string? Name { get; set; }
    public Type? DotnetType { get; set; }

    public DocMember[]? Members { get; set; }

    public DocField[] Fields
        => Members!.Where(x => x is DocField).Cast<DocField>().ToArray();

    public DocProperty[] Properties
        => Members!.Where(x => x is DocProperty).Cast<DocProperty>().ToArray();

    public DocMethod[] Methods
        => Members!.Where(x => x is DocMethod m && m.Method is MethodInfo).Cast<DocMethod>().ToArray();

    public DocMethod[] Constructors
        => Members!.Where(x => x is DocMethod m && m.Method is ConstructorInfo).Cast<DocMethod>().ToArray();

    public override void Populate(DocMember[] members, Assembly assembly)
    {
        var s = NodeName.Split('.');
        Name = s.Last();

        DotnetType = assembly.GetType(NodeName);

        if (DotnetType is null)
        {
            // maybe its part of a parent type
            for (var i = s.Length - 1; i != 1; i--)
            {
                DotnetType = assembly.GetType(string.Join(".", s.Take(i)));

                if (DotnetType is not null)
                {
                    var properName = DotnetType.FullName + "+" + string.Join('+', s.Skip(i).Take(s.Length - i));
                    DotnetType = assembly.GetType(properName);
                    break;
                }
            }
        }
    }

    public override void Finalize(DocMember[] members) => Members = members.Where(x => x.Parent == this).ToArray();
}

public class DocMethod : DocMember
{
    public DocMethod(docMember member) : base(member)
    {
    }

    public MethodBase? Method { get; set; }
    public string? Name { get; set; }
    public bool Ignored { get; set; }

    public override void Populate(DocMember[] members, Assembly assembly)
    {
        Name = GetNodeTermName();
        var parentName = NodeName[..(NodeName.Length - Name.Length - 1)];
        Parent = members.FirstOrDefault(x => x.NodeName == parentName) as DocType ?? new DocType(parentName, assembly);

        if (Name.StartsWith("#ctor"))
        {
            var args = Name == "#ctor" ? Array.Empty<string>() : Name[6..^1].Split(',');
            Method = Parent!.DotnetType!.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x =>
            {
                var p = x.GetParameters();

                if (p.Length! != args.Length)
                    return false;

                for (var i = 0; i != p.Length; i++)
                {
                    var fmt = FormatRawArgument(x, args[i]);

                    if ((p[i].ParameterType.FullName ?? p[i].ParameterType.Name) != fmt
                        && p[i].ToString().Split(' ')[0] != fmt)
                        return false;
                }

                return true;
            });
        }
        else
        {
            if (Name.Contains('#'))
            {
                // explicit interface declaration -- ignore
                Ignored = true;
                return;
            }

            // GENERIC HELL ALERT:
            // generics are formatted in the name as 'Name``n', where
            // n is the number of generic arguments in the function definition.
            // When these generic arguments are used as method arguments, the *index*
            // is used to define *which* generic is used.

            var methodGenericMatch = Regex.Match(Name, @"(.*?)``(\d+?)(?>$|\()");

            var methodGenericCount = 0;

            if (methodGenericMatch.Success)
            {
                methodGenericCount = int.Parse(methodGenericMatch.Groups[2].Value);
            }

            // params
            if (Name.Contains('('))
            {
                // OK, i hate doc strings: basic split of ',' wont work
                // because of generic type arguments. We need to traverse the args string
                // and control an escape flag to determine if the arg is generically escaped
                // or not.

                var paramString = Regex.Match(Name, @".*?\((.*?)\)").Groups[1].Value;

                var args = PullArgs(paramString);

                var targetMethodName = Regex.Replace(Name.Split('(')[0], @"(`{1,2}\d+)", m => "");

                Method = Parent!.DotnetType!.GetTypeInfo().DeclaredMethods.FirstOrDefault(x =>
                {
                    var p = x.GetParameters();

                    if (p.Length! != args.Count)
                        return false;

                    var generics = x.GetGenericArguments() ?? System.Type.EmptyTypes;

                    if (generics.Length != methodGenericCount)
                        return false;

                    // check generics

                    for (var i = 0; i != p.Length; i++)
                    {
                        var n = args[i];
                        var pr = p[i];
                        if (n.EndsWith('@'))
                        {
                            n = n[..^1] + "&"; // makes byref
                            if (!pr.IsOut)
                                return false;
                        }

                        // reformat our docstring parameter name to match a dotnet parameter name
                        var fmt = FormatRawArgument(x, n);

                        if (fmt != (pr.ParameterType.FullName ?? pr.ParameterType.Name) &&
                            fmt != pr.ToString().Split(' ')[0])
                            return false;
                    }

                    return targetMethodName == x.Name;
                });
            }
            else
            {
                Method = Parent!.DotnetType!.GetTypeInfo().DeclaredMethods.FirstOrDefault(x =>
                    x.Name == (methodGenericMatch.Success ? methodGenericMatch.Groups[1].Value : Name) &&
                    x.GetGenericArguments().Length == methodGenericCount);
            }
        }

        if (Method is null)
            throw new Exception("Method is null");
    }

    private static List<string> PullArgs(string s)
    {
        var args = new List<string>();
        var currentArg = "";
        var escaped = false;
        var lvl = 0;
        foreach (var c in s)
        {
            switch (c)
            {
                case '{':
                    escaped = true;
                    lvl++;
                    currentArg += c;
                    break;
                case '}':
                    lvl--;
                    escaped = lvl > 0;
                    currentArg += c;
                    break;
                case ',' when !escaped:
                    args.Add(currentArg);
                    currentArg = "";
                    break;
                default:
                    currentArg += c;
                    break;
            }
        }

        if (!string.IsNullOrEmpty(currentArg))
            args.Add(currentArg);

        return args;
    }

    private static string FormatRawArgument(MethodBase method, string section)
    {
        var subSection = Regex.Match(section, @"(.*?){(.*?)}$");
        var mgArgTL = Regex.Match(section, @"``(\d+)");
        var dgArgTL = Regex.Match(section, @"`(\d+)");

        if (mgArgTL.Success && !subSection.Success)
            return method.GetGenericArguments()[int.Parse(mgArgTL.Groups[1].Value)].Name;
        if (dgArgTL.Success && !subSection.Success)
            return method.DeclaringType!.GetGenericArguments()[int.Parse(dgArgTL.Groups[1].Value)].Name;
        if (!subSection.Success)
            return section;

        List<string> result = new();

        var args = PullArgs(subSection.Groups[2].Value);

        var wrappingType = $"{subSection.Groups[1].Value}`{args.Count}";

        foreach (var arg in args)
        {
            var argCopy = arg;
            var mgArg = Regex.Match(arg, @"``(\d+)");
            var dgArg = Regex.Match(arg, @"`(\d+)");

            // incase of wrapping types, preform a replace
            if (mgArg.Success)
            {
                var substitute = method.GetGenericArguments()[int.Parse(mgArg.Groups[1].Value)].Name;
                argCopy = arg.Replace($"``{mgArg.Groups[1].Value}", substitute);
            }
            else if (dgArg.Success)
            {
                var substitute = method.DeclaringType!.GetGenericArguments()[int.Parse(dgArg.Groups[1].Value)].Name;
                argCopy = arg.Replace($"`{dgArg.Groups[1].Value}", substitute);
            }

            result.Add(FormatRawArgument(method, argCopy));
        }

        return $"{wrappingType}[{string.Join(',', result)}]";
    }
}

public class DocProperty : DocMember
{
    public DocProperty(docMember member) : base(member)
    {
    }

    public string? Name { get; set; }
    public PropertyInfo? PropertyInfo { get; set; }

    public override void Populate(DocMember[] members, Assembly assembly)
    {
        Name = GetNodeTermName();

        var parentName = NodeName[..(NodeName.Length - Name.Length - 1)];

        //special case for indexing
        if (Regex.IsMatch(Name, @".*?\(.*?\)"))
            Name = Name[..Name.IndexOf('(')];

        Parent = members.FirstOrDefault(x => x.NodeName == parentName) as DocType ?? new DocType(parentName, assembly);

        PropertyInfo = Parent!.DotnetType!.GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x.Name == Name);
    }
}

public class DocField : DocMember
{
    public DocField(docMember member) : base(member)
    {
    }

    public string? Name { get; set; }
    public FieldInfo? FieldInfo { get; set; }

    public override void Populate(DocMember[] members, Assembly assembly)
    {
        Name = GetNodeTermName();

        var parentName = NodeName[..(NodeName.Length - Name.Length - 1)];

        //special case for indexing
        if (Regex.IsMatch(Name, @".*?\(.*?\)"))
            Name = Name[..Name.IndexOf('(')];

        Parent = members.FirstOrDefault(x => x.NodeName == parentName) as DocType ?? new DocType(parentName, assembly);

        FieldInfo = Parent!.DotnetType!.GetTypeInfo().DeclaredFields.FirstOrDefault(x => x.Name == Name);
    }
}

public class DocEvent : DocMember
{
    public DocEvent(docMember member) : base(member)
    {
    }
}

public class DocMember
{
    public DocMember(docMember member)
    {
        NodeName = member.name![2..];
        Type = (MemberType)member.name[0];
        InlineDocItems = member.Items!;
    }

    public DocMember(MemberType type, string name)
    {
        NodeName = $"{type}:{name}";
        Type = type;
        InlineDocItems = Array.Empty<object>();
    }

    public string NodeName { get; set; }
    public MemberType Type { get; set; }
    public DocType? Parent { get; set; }

    public object[] InlineDocItems { get; set; }

    public virtual void Populate(DocMember[] members, Assembly assembly)
    {
    }

    public virtual void Finalize(DocMember[] members) { }

    public static DocMember FromMember(docMember member)
    {
        var type = GetTypeOfDefName(member.name!);

        return type switch
        {
            MemberType.Type => new DocType(member),
            MemberType.Method => new DocMethod(member),
            MemberType.Property => new DocProperty(member),
            MemberType.Field => new DocField(member),
            MemberType.Event => new DocEvent(member),
            _ => throw new NotImplementedException()
        };
    }

    public static MemberType GetTypeOfDefName(string name) => (MemberType)name[0];

    protected string GetNodeTermName()
    {
        var esc = false;
        var i = 0;

        return new string(NodeName.Reverse().TakeWhile(x =>
        {
            if (x == ')')
            {
                esc = true;
                i++;
            }
            else if (x == '(')
            {
                i--;
                esc = i != 0;
            }

            return esc || x != '.';
        }).Reverse().ToArray());
    }
}

public enum MemberType
{
    Type = 'T',
    Method = 'M',
    Property = 'P',
    Field = 'F',
    Event = 'E'
}

// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
[XmlRootAttribute(Namespace = "", IsNullable = false)]
public class doc
{
    private docAssembly? assemblyField;

    private docMember[]? membersField;

    /// <remarks />
    public docAssembly? assembly
    {
        get => assemblyField;
        set => assemblyField = value;
    }

    /// <remarks />
    [XmlArrayItemAttribute("member", IsNullable = false)]
    public docMember[]? members
    {
        get => membersField;
        set => membersField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docAssembly
{
    private string? nameField;

    /// <remarks />
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMember
{
    private object[]? itemsField;

    private string? nameField;

    /// <remarks />
    [XmlElementAttribute("exception", typeof(docMemberException))]
    [XmlElementAttribute("inheritdoc", typeof(docMemberInheritDoc))]
    [XmlElementAttribute("param", typeof(docMemberParam))]
    [XmlElementAttribute("remarks", typeof(docMemberRemarks))]
    [XmlElementAttribute("returns", typeof(docMemberReturns))]
    [XmlElementAttribute("summary", typeof(docMemberSummary))]
    [XmlElementAttribute("typeparam", typeof(docMemberTypeparam))]
    public object[]? Items
    {
        get => itemsField;
        set => itemsField = value;
    }

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberInheritDoc
{
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberException : docMemberSummary
{
    private string? accessorField;

    private string? crefField;


    /// <remarks />
    [XmlAttributeAttribute]
    public string? cref
    {
        get => crefField;
        set => crefField = value;
    }

    /// <remarks />
    [XmlAttributeAttribute]
    public string? accessor
    {
        get => accessorField;
        set => accessorField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberExceptionTypeparamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberExceptionParamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberExceptionSee
{
    private string? crefField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? cref
    {
        get => crefField;
        set => crefField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberParam : docMemberSummary
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberParamParamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberParamSee
{
    private string? crefField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? cref
    {
        get => crefField;
        set => crefField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberParamTypeparamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberRemarks : docMemberSummary
{
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberRemarksParamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberRemarksSee
{
    private string? crefField;

    private string? langwordField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? cref
    {
        get => crefField;
        set => crefField = value;
    }

    /// <remarks />
    [XmlAttributeAttribute]
    public string? langword
    {
        get => langwordField;
        set => langwordField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberRemarksTypeparamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberReturns : docMemberSummary
{
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberReturnsParamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberReturnsSee
{
    private string? crefField;

    private string? langwordField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? langword
    {
        get => langwordField;
        set => langwordField = value;
    }

    /// <remarks />
    [XmlAttributeAttribute]
    public string? cref
    {
        get => crefField;
        set => crefField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberReturnsTypeparamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberSummary
{
    private ItemsChoiceType[]? itemsElementNameField;

    private object[]? itemsField;

    private string[]? textField;

    /// <remarks />
    [XmlElementAttribute("c", typeof(string))]
    [XmlElementAttribute("i", typeof(string))]
    [XmlElementAttribute("paramref", typeof(docMemberSummaryParamref))]
    [XmlElementAttribute("see", typeof(docMemberSummarySee))]
    [XmlElementAttribute("seealso", typeof(docMemberSummarySeealso))]
    [XmlElementAttribute("typeparamref", typeof(docMemberSummaryTypeparamref))]
    [XmlElementAttribute("br", typeof(docMemberSummaryBr))]
    [XmlChoiceIdentifierAttribute("ItemsElementName")]
    public object[]? Items
    {
        get => itemsField;
        set => itemsField = value;
    }

    /// <remarks />
    [XmlElementAttribute("ItemsElementName")]
    [XmlIgnoreAttribute]
    public ItemsChoiceType[]? ItemsElementName
    {
        get => itemsElementNameField;
        set => itemsElementNameField = value;
    }

    /// <remarks />
    [XmlTextAttribute]
    public string[]? Text
    {
        get => textField;
        set => textField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberSummaryParamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberSummarySee
{
    private string? crefField;

    private string? hrefField;

    private string? langwordField;

    private string? valueField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? cref
    {
        get => crefField;
        set => crefField = value;
    }

    /// <remarks />
    [XmlAttributeAttribute]
    public string? langword
    {
        get => langwordField;
        set => langwordField = value;
    }

    /// <remarks />
    [XmlAttributeAttribute]
    public string? href
    {
        get => hrefField;
        set => hrefField = value;
    }

    /// <remarks />
    [XmlTextAttribute]
    public string? Value
    {
        get => valueField;
        set => valueField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberSummarySeealso
{
    private string? hrefField;

    private string? valueField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? href
    {
        get => hrefField;
        set => hrefField = value;
    }

    /// <remarks />
    [XmlTextAttribute]
    public string? Value
    {
        get => valueField;
        set => valueField = value;
    }
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberSummaryTypeparamref
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}

[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberSummaryBr
{
}

/// <remarks />
[SerializableAttribute]
[XmlTypeAttribute(IncludeInSchema = false)]
public enum ItemsChoiceType
{
    /// <remarks />
    c,

    /// <remarks />
    i,

    /// <remarks />
    paramref,

    /// <remarks />
    see,

    /// <remarks />
    seealso,

    /// <remarks />
    typeparamref,
    br
}

/// <remarks />
[SerializableAttribute]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class docMemberTypeparam : docMemberSummary
{
    private string? nameField;

    /// <remarks />
    [XmlAttributeAttribute]
    public string? name
    {
        get => nameField;
        set => nameField = value;
    }
}
