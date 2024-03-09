using EdgeDB.DataTypes;
using EdgeDB.Schema;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.QueryNodes;

internal partial class InsertNode
{
    private WriterProxy JsonSetterPath(PropertyInfo propertyInfo, string? parentReference)
    {
        if (EdgeDBTypeUtils.TryGetScalarType(propertyInfo.PropertyType, out var edgeqlType))
        {
            return writer => writer
                .TypeCast(edgeqlType.ToString())
                .Append(Value.Of(writer =>
                    writer.Function(
                        "json_get",
                        'x',
                        Value.Of(writer => writer.SingleQuoted(propertyInfo.Name))
                    ))
                );
        }

        if (EdgeDBTypeUtils.IsLink(propertyInfo.PropertyType, out var isArray, out var innerLinkType))
        {
            return JsonLinkLookup(parentReference, isArray, innerLinkType, propertyInfo);
        }

        throw new InvalidOperationException(
            $"Unknown type on property '{propertyInfo.Name}': {propertyInfo.PropertyType.Name}");
    }

    private void GenerateJsonMapping(QueryWriter writer, string varName, string? parentReference, Type type, SchemaInfo info)
    {
        var shape = EdgeDBPropertyMapInfo.Create(type).Properties
            .ToDictionary<EdgeDBPropertyInfo?, string, object?>(
                edgedbProp => edgedbProp!.EdgeDBName,
                edgedbProp => JsonSetterPath(edgedbProp!.PropertyInfo, parentReference)
            );

        writer.Wrapped(writer => QueryBuilder.For(
            ctx => EdgeQL.JsonArrayUnpack(ctx.QueryArgument<Json>(varName)),
            x => QueryBuilder.Insert(type, shape, false).UnlessConflict()
        ).WriteTo(writer, this, new CompileContext() {SchemaInfo = SchemaInfo}));
    }

    private WriterProxy JsonLinkLookup(string? parentReference, bool isArray, Type? innerType, PropertyInfo propertyInfo)
    {
        if (parentReference is null)
        {
            return writer => writer.Append("{}");
        }

        WriterProxy propReference = writer => writer.SingleQuoted(propertyInfo.Name);

        WriterProxy inner = isArray
            ? writer => writer
                .Function(
                    "json_get",
                    'x',
                    Value.Of(propReference),
                    "'from'"
                )
                .Append(" limit ")
                .TypeCast("int64")
                .Function("json_get", 'x', Value.Of(propReference), "'to'")
            : writer => writer
                .Function(
                    "json_get",
                    'x',
                    Value.Of(propReference),
                    "'index'"
                )
                .Append(" limit 1");

        return writer => writer
            .Wrapped(writer => writer
                .Append("select ", parentReference, " offset ")
                .TypeCast("int64")
                .Append(Value.Of(inner))
            )
            .Append(" if ")
            .Function(
                "json_typeof",
                Value.Of(writer => writer
                    .Function("json_get", 'x', Value.Of(propReference))
                )
            )
            .Append(" != 'null' else ")
            .TypeCast(innerType?.GetEdgeDBTypeName() ?? propertyInfo.PropertyType.GetEdgeDBTypeName())
            .Append("{}");
    }

    /// <summary>
    ///     Builds a json-based insert shape.
    /// </summary>
    /// <returns>
    ///     The insert shape for a json-based value.
    /// </returns>
    private ShapeDefinition BuildJsonShape()
    {
        if (Context.Value is null || !Context.Value.Is<IJsonVariable>(out var jsonValue))
            throw new InvalidOperationException("Expecting insert value to be a json variable");

        var mappingName = QueryUtils.GenerateRandomVariableName();
        var depth = jsonValue.Depth;

        // create a depth map that contains each nested level of types to be inserted
        var depthMap = JsonUtils
            .BuildDepthMap(jsonValue)
            .GroupBy(x => x.Depth)
            .ToArray();

        // generate the global maps
        for (var i = depth; i != 0; i--)
        {
            var map = depthMap[i];
            var node = map.First();
            var variableName = QueryUtils.GenerateRandomVariableName();

            var parent = i == depth ? null : $"{mappingName}_d{i + 1}";

            // generate a introspection-dependant sub query for the insert or select
            var query = new SubQuery((info, writer) =>
                GenerateJsonMapping(writer, variableName, parent, node.Type, info));

            // tell the builder that this query requires introspection
            RequiresIntrospection = true;

            // serialize this depths values and set the variable & global for the sub-query
            var iterationJson = JsonConvert.SerializeObject(map.Select(x => x.JsonNode));

            SetVariable(variableName, new Json(iterationJson));

            SetGlobal($"{mappingName}_d{i}", query, null);
        }

        // replace the json variables content with the root depth map
        SetVariable(jsonValue.VariableName,
            new Json(JsonConvert.SerializeObject(depthMap[0].Select(x => x.JsonNode))));

        var elements = new List<ShapeSetter>();

        foreach (var property in jsonValue.InnerType.GetEdgeDBTargetProperties(excludeId: true))
        {
            elements.Add(new ShapeSetter(writer => writer
                .Assignment(property.GetEdgeDBPropertyName(), JsonSetterPath(property, $"{mappingName}_d1")))
            );
        }

        // return out our insert shape
        return new ShapeDefinition($"json_{jsonValue.Name}:{jsonValue.VariableName}", elements);
    }
}
