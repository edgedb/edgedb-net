using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EdgeDB.DocGenerator
{
    internal class Parser
    {
        public static DocMember[] Load(string file)
        {
            var serializer = new XmlSerializer(typeof(doc));
            using var reader = new System.IO.StreamReader(file);
            var t = (doc)serializer.Deserialize(reader)!;

            var members = t.members!.Select(x => DocMember.FromMember(x)).OrderByDescending(x => x.Type).ToArray();

            // get the edgebd.net assembly
            var edgedbAssembly = Assembly.GetAssembly(typeof(EdgeDB.EdgeDBClient))!;
            
            foreach(var member in members)
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
        public string? Name { get; set; }
        public Type? DotnetType { get; set; }

        public DocMember[]? Members { get; set; }

        public DocField[] Fields
            => Members!.Where(x => x is DocField).Cast<DocField>().ToArray();

        public DocProperty[] Properties
            => Members!.Where(x => x is DocProperty).Cast<DocProperty>().ToArray();

        public DocMethod[] Methods
            => Members!.Where(x => x is DocMethod).Cast<DocMethod>().ToArray();

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

        public override void Populate(DocMember[] members, Assembly assembly)
        {
            var s = NodeName.Split('.');
            Name = s.Last();

            DotnetType = assembly.GetType(NodeName);

            if(DotnetType is null)
            {
                // maybe its part of a parent type
                for(int i = s.Length - 1; i != 1; i--)
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

        public override void Finalize(DocMember[] members)
        {
            Members = members.Where(x => x.Parent == this).ToArray();
        }
    }

    public class DocMethod : DocMember
    {
        public MethodBase? Method { get; set; }
        public string? Name { get; set; }

        public DocMethod(docMember member) : base(member)
        {
        }

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

                    for (int i = 0; i != p.Length; i++)
                        if (p[i].ParameterType.FullName != args[i])
                            return false;

                    return true;
                });
            }
            else
            {
                // params
                if (Name.Contains("("))
                {
                    var args = Name[(Name.IndexOf('(') + 1)..^1].Split(',');

                    Method = Parent!.DotnetType!.GetTypeInfo().DeclaredMethods.FirstOrDefault(x =>
                    {
                        var p = x.GetParameters();

                        if (p.Length! != args.Length)
                            return false;

                        for (int i = 0; i != p.Length; i++)
                        {
                            var n = args[i];
                            var pr = p[i];
                            if (n.EndsWith('@'))
                            {
                                n = n[..^1] + "&"; // makes byref
                                if (!pr.IsOut)
                                    return false;
                            }

                            if (n != pr.ParameterType.FullName)
                                return false;
                        }

                        return true;
                    });
                }
                else
                    Method = Parent!.DotnetType!.GetTypeInfo().DeclaredMethods.FirstOrDefault(x => x.Name == Name) as MethodBase;
            }
        }
    }

    public class DocProperty : DocMember
    {
        public string? Name { get; set; }
        public PropertyInfo? PropertyInfo { get; set; }

        public DocProperty(docMember member) : base(member)
        {
        }

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
        public string? Name { get; set; }
        public FieldInfo? FieldInfo { get; set; }
        
        public DocField(docMember member) : base(member)
        {
        }

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
        public string NodeName { get; set; }
        public MemberType Type { get; set; }
        public DocType? Parent { get; set; }

        public object[] InlineDocItems { get; set; }

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
        
        public virtual void Populate(DocMember[] members, Assembly assembly)
        {
            
        }

        public virtual void Finalize(DocMember[] members) { }

        public static DocMember FromMember(docMember member)
        {
            var type = (MemberType)member.name![0];

            switch (type)
            {

                case MemberType.Type:
                    return new DocType(member);
                case MemberType.Method:
                    return new DocMethod(member);
                case MemberType.Property:
                    return new DocProperty(member);
                case MemberType.Field:
                    return new DocField(member);
                case MemberType.Event:
                    return new DocEvent(member);
                default:
                    throw new NotImplementedException();
            }
        }

        protected string GetNodeTermName()
        {
            bool esc = false;
            int i = 0;

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
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class doc
    {

        private docAssembly? assemblyField;

        private docMember[]? membersField;

        /// <remarks/>
        public docAssembly? assembly
        {
            get
            {
                return this.assemblyField;
            }
            set
            {
                this.assemblyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("member", IsNullable = false)]
        public docMember[]? members
        {
            get
            {
                return this.membersField;
            }
            set
            {
                this.membersField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docAssembly
    {

        private string? nameField;

        /// <remarks/>
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMember
    {

        private object[]? itemsField;

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("exception", typeof(docMemberException))]
        [System.Xml.Serialization.XmlElementAttribute("inheritdoc", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("param", typeof(docMemberParam))]
        [System.Xml.Serialization.XmlElementAttribute("remarks", typeof(docMemberRemarks))]
        [System.Xml.Serialization.XmlElementAttribute("returns", typeof(docMemberReturns))]
        [System.Xml.Serialization.XmlElementAttribute("summary", typeof(docMemberSummary))]
        [System.Xml.Serialization.XmlElementAttribute("typeparam", typeof(docMemberTypeparam))]
        public object[]? Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberException
    {

        private docMemberExceptionTypeparamref? typeparamrefField;

        private docMemberExceptionParamref? paramrefField;

        private docMemberExceptionSee? seeField;

        private string[]? textField;

        private string? crefField;

        private string? accessorField;

        /// <remarks/>
        public docMemberExceptionTypeparamref? typeparamref
        {
            get
            {
                return this.typeparamrefField;
            }
            set
            {
                this.typeparamrefField = value;
            }
        }

        /// <remarks/>
        public docMemberExceptionParamref? paramref
        {
            get
            {
                return this.paramrefField;
            }
            set
            {
                this.paramrefField = value;
            }
        }

        /// <remarks/>
        public docMemberExceptionSee? see
        {
            get
            {
                return this.seeField;
            }
            set
            {
                this.seeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[]? Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? cref
        {
            get
            {
                return this.crefField;
            }
            set
            {
                this.crefField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? accessor
        {
            get
            {
                return this.accessorField;
            }
            set
            {
                this.accessorField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberExceptionTypeparamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberExceptionParamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberExceptionSee
    {

        private string? crefField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? cref
        {
            get
            {
                return this.crefField;
            }
            set
            {
                this.crefField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberParam
    {

        private object[]? itemsField;

        private string[]? textField;

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("paramref", typeof(docMemberParamParamref))]
        [System.Xml.Serialization.XmlElementAttribute("see", typeof(docMemberParamSee))]
        [System.Xml.Serialization.XmlElementAttribute("typeparamref", typeof(docMemberParamTypeparamref))]
        public object[]? Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[]? Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberParamParamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberParamSee
    {

        private string? crefField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? cref
        {
            get
            {
                return this.crefField;
            }
            set
            {
                this.crefField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberParamTypeparamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberRemarks
    {

        private object[]? itemsField;

        private string[]? textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("b", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("br", typeof(object))]
        [System.Xml.Serialization.XmlElementAttribute("paramref", typeof(docMemberRemarksParamref))]
        [System.Xml.Serialization.XmlElementAttribute("see", typeof(docMemberRemarksSee))]
        [System.Xml.Serialization.XmlElementAttribute("typeparamref", typeof(docMemberRemarksTypeparamref))]
        public object[]? Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[]? Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberRemarksParamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberRemarksSee
    {

        private string? crefField;

        private string? langwordField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? cref
        {
            get
            {
                return this.crefField;
            }
            set
            {
                this.crefField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? langword
        {
            get
            {
                return this.langwordField;
            }
            set
            {
                this.langwordField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberRemarksTypeparamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberReturns
    {

        private object[]? itemsField;

        private string[]? textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("paramref", typeof(docMemberReturnsParamref))]
        [System.Xml.Serialization.XmlElementAttribute("see", typeof(docMemberReturnsSee))]
        [System.Xml.Serialization.XmlElementAttribute("typeparamref", typeof(docMemberReturnsTypeparamref))]
        public object[]? Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[]? Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberReturnsParamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberReturnsSee
    {

        private string? langwordField;

        private string? crefField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? langword
        {
            get
            {
                return this.langwordField;
            }
            set
            {
                this.langwordField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? cref
        {
            get
            {
                return this.crefField;
            }
            set
            {
                this.crefField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberReturnsTypeparamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberSummary
    {

        private object[]? itemsField;

        private ItemsChoiceType[]? itemsElementNameField;

        private string[]? textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("c", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("i", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("paramref", typeof(docMemberSummaryParamref))]
        [System.Xml.Serialization.XmlElementAttribute("see", typeof(docMemberSummarySee))]
        [System.Xml.Serialization.XmlElementAttribute("seealso", typeof(docMemberSummarySeealso))]
        [System.Xml.Serialization.XmlElementAttribute("typeparamref", typeof(docMemberSummaryTypeparamref))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[]? Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType[]? ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[]? Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberSummaryParamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberSummarySee
    {

        private string? crefField;

        private string? langwordField;

        private string? hrefField;

        private string? valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? cref
        {
            get
            {
                return this.crefField;
            }
            set
            {
                this.crefField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? langword
        {
            get
            {
                return this.langwordField;
            }
            set
            {
                this.langwordField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? href
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string? Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberSummarySeealso
    {

        private string? hrefField;

        private string? valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? href
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string? Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberSummaryTypeparamref
    {

        private string? nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {

        /// <remarks/>
        c,

        /// <remarks/>
        i,

        /// <remarks/>
        paramref,

        /// <remarks/>
        see,

        /// <remarks/>
        seealso,

        /// <remarks/>
        typeparamref,
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class docMemberTypeparam
    {

        private string? nameField;

        private string? valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string? name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string? Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}
