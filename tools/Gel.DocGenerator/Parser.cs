using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Gel.DocGenerator;

internal class Parser
{
    public static DocMember[] Load(string file)
    {
        var serializer = new XmlSerializer(typeof(Doc), new XmlRootAttribute("doc"));
        using var reader = new StreamReader(file);
        var t = (Doc)serializer.Deserialize(reader)!;

        var members = t.members!.Select(x => DocMember.FromMember(x)).OrderByDescending(x => x.Type).ToArray();

        // get the edgebd.net assembly
        var gelAssembly = Assembly.GetAssembly(typeof(GelClientPool))!;

        foreach (var member in members)
        {
            member.Populate(members, gelAssembly);
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

        if (DotnetType is null)
            throw new Exception($"DotnetType {Name} is null");
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
        List<string> termNames = GetNodeTermNames();
        Name = termNames[1];

        var parentName = termNames[0];

        Parent = members.FirstOrDefault(x => x.NodeName == parentName) as DocType ?? new DocType(parentName, assembly);

        if (Name.StartsWith("#ctor"))
        {
            string[] args = termNames.Count > 2 ? termNames[2][1..^1].Split(',') : [];
            Method = Parent!.DotnetType!.GetTypeInfo().DeclaredConstructors.FirstOrDefault(dotnetCtor =>
            {
                var dotnetParams = dotnetCtor.GetParameters();

                if (dotnetParams.Length! != args.Length)
                    return false;

                for (var i = 0; i != dotnetParams.Length; i++)
                {
                    var fmt = FormatRawArgument(dotnetCtor, args[i]);

                    if ((dotnetParams[i].ParameterType.FullName ?? dotnetParams[i].ParameterType.Name) != fmt
                        && dotnetParams[i].ToString().Split(' ')[0] != fmt)
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

            var termGenericsCount = 0;

            if (IsGenericName(Name))
            {
                termGenericsCount = GetGenericArgCount(Name);
            }

            // params
            if (termNames.Count > 2)
            {
                // OK, i hate doc strings: basic split of ',' wont work
                // because of generic type arguments. We need to traverse the args string
                // and control an escape flag to determine if the arg is generically escaped
                // or not.

                var termParams = PullArgs(termNames[2][1..^1]);

                var termMethodName = termNames[1];
                if (IsGenericName(Name))
                {
                    termMethodName = GetGenericBaseName(termMethodName);
                }

                Method = Parent!.DotnetType!.GetTypeInfo().DeclaredMethods.FirstOrDefault(dotnetMethod =>
                {
                    if (termMethodName != dotnetMethod.Name)
                        return false;

                    var dotnetParams = dotnetMethod.GetParameters();

                    if (dotnetParams.Length! != termParams.Count)
                        return false;

                    var dotnetGenericArgs = dotnetMethod.GetGenericArguments() ?? System.Type.EmptyTypes;

                    if (dotnetGenericArgs.Length != termGenericsCount)
                        return false;

                    // check generics

                    for (var i = 0; i != dotnetParams.Length; i++)
                    {
                        var termParam = termParams[i];
                        var dotnetParam = dotnetParams[i];
                        if (termParam.EndsWith('@'))
                        {
                            termParam = termParam[..^1] + "&"; // makes byref
                            if (!dotnetParam.IsOut)
                                return false;
                        }

                        // Generic parameters are prefixed with a number of "`" based on where
                        // they are declared (eg. class, inner class, method, etc.)
                        // There isn't a straight forward way to calculate this from reflection
                        // so just reduce all repeated "`" to a single one.
                        while (termParam.Contains("``"))
                        {
                            termParam = termParam.Replace("``", "`");
                        }

                        string dotnetTermName = DotnetTypeToTermName(dotnetParam.ParameterType);

                        if (termParam != dotnetTermName)
                            return false;
                    }

                    return true;
                });
            }
            else
            {
                var targetMethodName = termNames[1];
                if (IsGenericName(Name))
                {
                    targetMethodName = GetGenericBaseName(targetMethodName);
                }

                Method = Parent!.DotnetType!.GetTypeInfo().DeclaredMethods.FirstOrDefault(x =>
                    x.Name == targetMethodName &&
                    x.GetGenericArguments().Length == termGenericsCount);
            }
        }

        if (Method is null)
            throw new Exception($"Method {Name} is null");
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

    private static bool IsGenericName(string name)
    {
        return name.Contains("`");
    }

    private static string GetGenericBaseName(string name)
    {
        return name[0 .. name.IndexOf("`")];
    }

    private static int GetGenericArgCount(string name)
    {
        return int.Parse(name[(name.LastIndexOf("`") + 1) .. ]);
    }

    private static string DotnetTypeToTermName(Type typeInfo)
    {
        if (typeInfo.IsGenericParameter)
        {
            return "`" + typeInfo.GenericParameterPosition.ToString();
        }
        else if (typeInfo.IsGenericType)
        {
            return typeInfo.Namespace
                + "."
                + typeInfo.Name[0..typeInfo.Name.IndexOf('`')]
                + "{"
                + (typeInfo.GenericTypeArguments.Count() > 0
                    ? string.Join(
                        ",",
                        typeInfo.GenericTypeArguments.Select(
                            x => DotnetTypeToTermName(x))
                    )
                    : string.Join(
                        ",",
                        Enumerable.Range(0, GetGenericArgCount(typeInfo.Name))
                        .Select(x => "`" + x.ToString())
                    )
                )
                + "}";
        }
        else
        {
            // Normally FullName will use "+" instead of "." if `typeInfo.IsNested`
            return typeInfo.FullName!.Replace('+', '.');
        }
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
        List<string> termNames = GetNodeTermNames();
        Name = termNames[1];

        // Term name uses '#' for some reason
        Name = Name.Replace('#', '.');

        var parentName = termNames[0];

        //special case for indexing
        if (Regex.IsMatch(Name, @".*?\(.*?\)"))
            Name = Name[..Name.IndexOf('(')];

        Parent = members.FirstOrDefault(x => x.NodeName == parentName) as DocType ?? new DocType(parentName, assembly);

        PropertyInfo = Parent!.DotnetType!.GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x.Name == Name);

        if (PropertyInfo is null)
            throw new Exception($"PropertyInfo {Name} is null");
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
        List<string> termNames = GetNodeTermNames();
        Name = termNames[1];

        var parentName = termNames[0];

        //special case for indexing
        if (Regex.IsMatch(Name, @".*?\(.*?\)"))
            Name = Name[..Name.IndexOf('(')];

        Parent = members.FirstOrDefault(x => x.NodeName == parentName) as DocType ?? new DocType(parentName, assembly);

        FieldInfo = Parent!.DotnetType!.GetTypeInfo().DeclaredFields.FirstOrDefault(x => x.Name == Name);

        if (FieldInfo is null)
            throw new Exception($"FieldInfo {Name} is null");
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

    protected List<string> GetNodeTermNames()
    {
        try
        {
            int paren = NodeName.IndexOf('(');
            if (paren >= 0)
            {
                int dot = NodeName.LastIndexOf('.', paren - 1);

                return [
                    NodeName[0 .. dot],
                    NodeName[(dot + 1) .. paren],
                    NodeName[paren ..],
                ];
            }
            else
            {
                int dot = NodeName.LastIndexOf('.');

                return [
                    NodeName[0 .. dot],
                    NodeName[(dot + 1) ..]
                ];
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"  {NodeName}");
            throw;
        }
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
public class Doc
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
