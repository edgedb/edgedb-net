using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.StandardLibGenerator.Models
{
    [EdgeDBType(ModuleName = "schema")]
    internal class Type
    {
        public string Name { get; set; }
        public bool IsAbstract { get; set; }

        [EdgeDBIgnore]
        public string TypeOfSelf { get; set; }

        [EdgeDBIgnore]
        public Guid Id { get; set; }

        [EdgeDBDeserializer]
        public Type(IDictionary<string, object?> raw)
        {
            Name = (string)raw["name"]!;
            IsAbstract = (bool)raw["is_abstract"]!;
            TypeOfSelf = (string)raw["__tname__"]!;
            Id = (Guid)raw["id"]!;
        }

        public async Task<MetaType> GetMetaInfoAsync(EdgeDBClient client)
        {
            //var result = await QueryBuilder.Select<MetaType>((ctx) => new MetaType
            //{
            //    Pointers = ctx.Raw<Pointer[]>("[is schema::ObjectType].pointers { name, target: {name, is_abstract}}"),
            //    EnumValues = ctx.Raw<string[]?>("[is schema::ScalarType].enum_values")
            //}).Filter(x => x.Id == Id).ExecuteAsync(client);
            //return result.First()!;
            return null!;
        }
    }

    [EdgeDBType("Type", ModuleName = "schema")]
    internal class MetaType
    {
        public Guid Id { get; set; }
        public Pointer[]? Pointers { get; set; }
        public string[]? EnumValues { get; set; }

        [EdgeDBIgnore]
        public MetaInfoType Type
            => Pointers?.Any() ?? false
                ? MetaInfoType.Object
                : EnumValues?.Any() ?? false
                    ? MetaInfoType.Enum
                    : MetaInfoType.Unknown;
    }

    public enum MetaInfoType
    {
        Enum,
        Object,
        Unknown
    }

    internal class Pointer
    {
        public string? Name { get; set; }
        public Type? Type { get; set; }
    }
}
