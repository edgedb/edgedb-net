using EdgeDB.DataTypes;
using EdgeDB.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///    Represents a 'INSERT' node.
    /// </summary>
    internal class InsertNode : QueryNode<InsertContext>
    {
        /// <summary>
        ///     A readonly struct representing a setter in an insert shape.
        /// </summary>
        private readonly struct ShapeSetter : IWriteable
        {
            /// <summary>
            ///     Whether or not the setter requires introspection.
            /// </summary>
            public readonly bool RequiresIntrospection;

            /// <summary>
            ///     A string-based setter.
            /// </summary>
            private readonly WriterProxy? _setter;

            /// <summary>
            ///     A function-based setter which requires introspection.
            /// </summary>
            private readonly Action<QueryWriter, SchemaInfo>? _setterBuilder;

            /// <summary>
            ///     Constructs a new <see cref="ShapeSetter"/>.
            /// </summary>
            /// <param name="setter">A string-based setter.</param>
            public ShapeSetter(WriterProxy setter)
            {
                _setter = setter;
                _setterBuilder = null;
                RequiresIntrospection = false;
            }

            /// <summary>
            ///     Constructs a new <see cref="ShapeSetter"/>.
            /// </summary>
            /// <param name="builder">A function-based setter that requires introspection.</param>
            public ShapeSetter(Action<QueryWriter, SchemaInfo> builder)
            {
                _setterBuilder = builder;
                _setter = null;
                RequiresIntrospection = true;
            }

            /// <summary>
            ///     Builds this <see cref="ShapeSetter"/> to a string form without introspection.
            /// </summary>
            /// <param name="writer">The query string writer to append this shape to.</param>
            /// <exception cref="InvalidOperationException">
            ///     The current setter requires introspection.
            /// </exception>
            public void Write(QueryWriter writer)
            {
                if (_setter is null)
                    throw new InvalidOperationException("Cannot build insert setter, a setter requires introspection");

                _setter(writer);
            }

            /// <summary>
            ///     Converts this <see cref="ShapeSetter"/> to a string form with introspection.
            /// </summary>
            /// <param name="writer">The query string writer to append the shape to.</param>
            /// <param name="info">The introspected schema info.</param>
            /// <returns>A stringified edgeql setter.</returns>
            public void Write(QueryWriter writer, SchemaInfo info)
            {
                if (RequiresIntrospection && _setterBuilder is not null)
                    _setterBuilder(writer, info);
                else
                    Write(writer);
            }

            public static implicit operator ShapeSetter(WriterProxy s) => new(s);
            public static implicit operator ShapeSetter(Action<QueryWriter, SchemaInfo?> s) => new(s);
        }

        /// <summary>
        ///     Represents a insert shape definition.
        /// </summary>
        private readonly struct ShapeDefinition
        {
            /// <summary>
            ///     Whether or not the setter requires introspection.
            /// </summary>
            public readonly bool RequiresIntrospection;

            /// <summary>
            ///     The raw string form shape definition, if any.
            /// </summary>
            private readonly WriterProxy? _rawShape;

            /// <summary>
            ///     The setters in this shape definition.
            /// </summary>
            private readonly ShapeSetter[] _shape;

            private readonly string _name;

            /// <summary>
            ///     Constructs a new <see cref="ShapeDefinition"/> with the given shape body.
            /// </summary>
            /// <param name="shape"></param>
            public ShapeDefinition(string name, WriterProxy shape)
            {
                _name = name;
                _rawShape = shape;
                _shape = Array.Empty<ShapeSetter>();
                RequiresIntrospection = false;
            }

            /// <summary>
            ///     Constructs a new <see cref="ShapeDefinition"/> with the given shape body.
            /// </summary>
            /// <param name="shape"></param>
            public ShapeDefinition(string name, IEnumerable<ShapeSetter> shape)
            {
                _name = name;
                _shape = shape.ToArray();
                _rawShape = null;
                RequiresIntrospection = _shape.Any(x => x.RequiresIntrospection);
            }

            /// <summary>
            ///     Builds this <see cref="ShapeDefinition"/> into the string form without using introspection.
            /// </summary>
            /// <returns>The string form of the shape definition.</returns>
            /// <exception cref="InvalidOperationException">The shape body requires introspection to build.</exception>
            public void Build(QueryWriter writer)
            {
                if (_rawShape is not null)
                {
                    _rawShape(writer);
                    return;
                }

                if (_shape.Any(x => x.RequiresIntrospection))
                    throw new InvalidOperationException(
                        "Cannot build insert shape, some properties require introspection");

                writer.Shape($"shape_{_name}", _shape);
            }

            /// <summary>
            ///     Builds this <see cref="ShapeDefinition"/> into a string using schema introspection.
            /// </summary>
            /// <param name="writer">The query string writer to append the built shape to.</param>
            /// <param name="info">The schema introspection info.</param>
            /// <returns>The string form of the shape definition.</returns>
            public void Build(QueryWriter writer, SchemaInfo? info)
            {
                if (_rawShape is not null)
                {
                    _rawShape(writer);
                    return;
                }

                if (RequiresIntrospection && info is null)
                    throw new InvalidOperationException("Introspection is required to build this shape definition");

                if (RequiresIntrospection)
                    writer.Shape($"shape_{_name}", _shape, (w, x) => x.Write(w, info!));
                else
                    Build(writer);
            }
        }

        /// <summary>
        ///     The insert shape definition.
        /// </summary>
        private ShapeDefinition _shape;

        /// <summary>
        ///     Whether or not to autogenerate the unless conflict clause.
        /// </summary>
        private bool _autogenerateUnlessConflict;

        private LambdaExpression? _unlessConflictExpression;

        /// <summary>
        ///     The else clause if any.
        /// </summary>
        private Action<QueryWriter>? _elseStatement;

        /// <summary>
        ///     The list of currently inserted types used to determine if
        ///     a nested query can be preformed.
        /// </summary>
        private readonly List<Type> _subQueryMap = [];

        /// <inheritdoc/>
        public InsertNode(NodeBuilder builder) : base(builder)
        {
        }

        /// <inheritdoc/>
        public override void Visit()
        {
            // add the current type to the sub query map
            _subQueryMap.Add(OperatingType);

            // build the insert shape
            _shape = Context.IsJsonVariable
                ? BuildJsonShape()
                : BuildInsertShape();

            RequiresIntrospection = _shape.RequiresIntrospection;
        }

        /// <inheritdoc/>
        public override void FinalizeQuery(QueryWriter writer)
        {
            if (Context is {SetAsGlobal: true, GlobalName: not null})
            {
                SetGlobal(Context.GlobalName, new SubQuery(writer => writer
                    .Wrapped(Value.Of(WriteInsertStatement))
                ), null);
            }
            else
            {
                WriteInsertStatement(writer);
            }
        }

        private void WriteInsertStatement(QueryWriter writer)
        {
            writer.Append("insert ", OperatingType.GetEdgeDBTypeName(), ' ');

            _shape.Build(writer, SchemaInfo);

            if (_autogenerateUnlessConflict)
            {
                if (SchemaInfo is null)
                    throw new InvalidOperationException(
                        "Cannot use autogenerated unless conflict on without schema interpolation");

                if (!SchemaInfo.TryGetObjectInfo(OperatingType, out var typeInfo))
                    throw new NotSupportedException($"Could not find type info for {OperatingType}");

                writer.Append(' ');
                ConflictUtils.GenerateExclusiveConflictStatement(writer, typeInfo, _elseStatement is not null);
            }

            _elseStatement?.Invoke(writer);
        }

        private void AppendJsonIteration(
            QueryWriter writer,
            PropertyInfo property,
            string edgedbName,
            string mappingName,
            string iterationName,
            int index,
            bool isArray)
        {
            writer.Assignment(edgedbName, Value.Of(writer => writer.Wrapped(writer => writer
                .Append("select ", mappingName, "_d", index + 1, " offset ")
                .TypeCast("int64")
            )));

            // return a slice operator for multi links or a index operator for single links
            if (isArray)
            {
                writer
                    .Function(
                        "json_get",
                        iterationName,
                        Value.Of(writer => writer
                            .SingleQuoted(property.Name)
                        ),
                        Value.Of(writer => writer
                            .SingleQuoted(
                                Value.Of(writer => writer
                                    .Append(mappingName, "_depth_from")
                                )
                            )
                        )
                    )
                    .Append(" ?? 0 limit ")
                    .TypeCast("int64")
                    .Function(
                        "json_get",
                        iterationName,
                        Value.Of(writer => writer
                            .SingleQuoted(property.Name)
                        ),
                        Value.Of(writer => writer
                            .SingleQuoted(
                                Value.Of(writer => writer
                                    .Append(mappingName, "_depth_to")
                                )
                            )
                        )
                    )
                    .Append(" ?? 0");
            }
            else
            {
                writer
                    .Function(
                        "json_get",
                        iterationName,
                        Value.Of(writer => writer
                            .SingleQuoted(property.Name)
                        ),
                        Value.Of(writer => writer
                            .SingleQuoted(
                                Value.Of(writer => writer
                                    .Append(mappingName, "_depth_index")
                                )
                            )
                        )
                    )
                    .Append(" limit 1 if ")
                    .Function(
                        "json_typeof",
                        Value.Of(writer => writer
                            .Function(
                                "json_get",
                                iterationName,
                                Value.Of(writer => writer
                                    .SingleQuoted(property.Name)
                                )
                            )
                        )
                    )
                    .Append(" != \'null\' else")
                    .TypeCast(property.PropertyType.GetEdgeDBTypeName())
                    .Append("{}");
            }
        }

        /// <summary>
        ///     Builds a json-based insert shape.
        /// </summary>
        /// <returns>
        ///     The insert shape for a json-based value.
        /// </returns>
        private ShapeDefinition BuildJsonShape()
        {
            var mappingName = QueryUtils.GenerateRandomVariableName();
            var jsonValue = (IJsonVariable)Context.Value!;
            var depth = jsonValue.Depth;

            // create a depth map that contains each nested level of types to be inserted
            var depthMap = JsonUtils
                .BuildDepthMap(mappingName, jsonValue)
                .GroupBy(x => x.Depth)
                .ToArray();

            // generate the global maps
            for (var i = depth; i != 0; i--)
            {
                var map = depthMap[i];
                var node = map.First();
                var iterationName = QueryUtils.GenerateRandomVariableName();
                var variableName = QueryUtils.GenerateRandomVariableName();
                var isLast = depthMap.Length == i + 1;

                // IMPORTANT: since we're using 'i' within the callback, we must
                // store a local here so we dont end up calling the last iterations 'i' value
                var indexCopy = i;

                // generate a introspection-dependant sub query for the insert or select
                var query = new SubQuery((info, writer) =>
                {
                    var allProps = QueryGenerationUtils.GetProperties(info, node.Type).ToArray();
                    var typeName = node.Type.GetEdgeDBTypeName();
                    var infoCopy = info;

                    writer.Wrapped(writer =>
                    {
                        writer
                            .Append("for ")
                            .Append(iterationName)
                            .Append(" in json_array_unpack(")
                            .QueryArgument("json", variableName)
                            .Append(") union ")
                            .Wrapped(writer =>
                            {
                                var exclusiveProperties =
                                    QueryGenerationUtils.GetProperties(info, node.Type, true).ToArray();

                                writer.Append("insert ")
                                    .Append(typeName)
                                    .Shape($"shape_{iterationName}_{typeName}", allProps, (writer, x) =>
                                    {
                                        var edgedbName = x.GetEdgeDBPropertyName();
                                        var isScalar =
                                            EdgeDBTypeUtils.TryGetScalarType(x.PropertyType, out var edgeqlType);

                                        // we need to add a callback for value types that are default to determine if we need to
                                        // add the setter
                                        if (isScalar && x.PropertyType is {IsValueType: true, IsEnum: false})
                                        {
                                            if (!infoCopy.TryGetObjectInfo(jsonValue.InnerType, out var info))
                                                throw new InvalidOperationException(
                                                    $"Could not find {jsonValue.InnerType.GetEdgeDBTypeName()} in schema info!");

                                            // get the property defined in the schema
                                            var edgedbProp = info.Properties!.FirstOrDefault(x => x.Name == edgedbName);

                                            if (edgedbProp is null)
                                                throw new InvalidOperationException(
                                                    $"Could not find property '{edgedbName}' on type {jsonValue.InnerType.GetEdgeDBTypeName()}");

                                            if (edgedbProp is {Required: true, HasDefault: false})
                                            {
                                                writer
                                                    .Append(edgedbName)
                                                    .Append(" := ")
                                                    .TypeCast(edgeqlType!.ToString())
                                                    .Function(
                                                        "json_get",
                                                        jsonValue.Name,
                                                        new Value(writer => writer
                                                            .Append('\'')
                                                            .Append(x.Name)
                                                            .Append('\'')
                                                        )
                                                    );
                                            }

                                            return;
                                        }

                                        // if its a link, add a ternary statement for pulling the value out of a sub-map
                                        if (EdgeDBTypeUtils.IsLink(x.PropertyType, out var isArray, out _))
                                        {
                                            // if we're in the last iteration of the depth map, we know for certian there
                                            // are no sub types within the current context, we can safely set the link to
                                            // an empty set
                                            if (isLast)
                                            {
                                                writer.Assignment(edgedbName, "{}");
                                                return;
                                            }

                                            AppendJsonIteration(
                                                writer, x, edgedbName, mappingName, iterationName,
                                                indexCopy, isArray
                                            );

                                            return;
                                        }

                                        // if its a scalar type, use json_get to pull the value and cast it to our property
                                        // type
                                        if (!isScalar)
                                            throw new NotSupportedException(
                                                $"Cannot use type {x.PropertyType} as there is no serializer for it");

                                        writer
                                            .Assignment(edgedbName, Value.Of(writer => writer
                                                .TypeCast(edgeqlType!.ToString())
                                                .Function(
                                                    "json_get",
                                                    iterationName,
                                                    Value.Of(writer => writer
                                                        .SingleQuoted(x.Name)
                                                    )
                                                )
                                            ));
                                    })
                                    .AppendIf(exclusiveProperties.Any, Value.Of(writer =>
                                    {
                                        writer.Append(" unless conflict on ");

                                        if (exclusiveProperties.Length is 1)
                                            writer.Append('.', exclusiveProperties[0].GetEdgeDBPropertyName());
                                        else
                                            writer.Shape(
                                                $"shape_{iterationName}_{typeName}_exclusive",
                                                exclusiveProperties,
                                                (writer, x) => writer.Append('.', x.GetEdgeDBPropertyName()),
                                                "()"
                                            );

                                        writer
                                            .Append(" else ").WrappedValues(values: ["select ", typeName]);
                                    }));
                            });
                    });
                });

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
                var edgedbName = property.GetEdgeDBPropertyName();
                var isScalar = EdgeDBTypeUtils.TryGetScalarType(property.PropertyType, out var edgeqlType);

                if (isScalar && property.PropertyType.IsValueType && !property.PropertyType.IsEnum)
                {
                    elements.Add(new ShapeSetter((writer, info) =>
                    {
                        if (!info.TryGetObjectInfo(jsonValue.InnerType, out var typeInfo))
                            throw new InvalidOperationException(
                                $"Could not find {jsonValue.InnerType.GetEdgeDBTypeName()} in schema info!");

                        // get the property defined in the schema
                        var edgedbProp = typeInfo.Properties!.FirstOrDefault(x => x.Name == edgedbName);

                        if (edgedbProp is null)
                            throw new InvalidOperationException(
                                $"Could not find property '{edgedbName}' on type {jsonValue.InnerType.GetEdgeDBTypeName()}");

                        writer.Assignment(edgedbName, Value.Of(writer => writer
                            .TypeCast(edgeqlType!.ToString())
                            .Function(
                                "json_get",
                                jsonValue.Name,
                                Value.Of(writer => writer
                                    .SingleQuoted(property.Name)
                                )
                            )
                        ));
                    }));

                    continue;
                }

                if (EdgeDBTypeUtils.IsLink(property.PropertyType, out var isArray, out _))
                {
                    elements.Add(new ShapeSetter(writer =>
                        AppendJsonIteration(writer, property, edgedbName, mappingName, jsonValue.Name, 0, isArray))
                    );
                    continue;
                }

                if (!isScalar)
                    throw new NotSupportedException(
                        $"Cannot use type {property.PropertyType} as there is no serializer for it");

                elements.Add(new ShapeSetter(writer => writer
                    .Assignment(edgedbName, Value.Of(writer => writer
                        .TypeCast(edgeqlType!.ToString()).Function(
                            "json_get",
                            jsonValue.Name,
                            Value.Of(writer => writer
                                .SingleQuoted(property.Name))
                        ))
                    )
                ));
            }

            // return out our insert shape
            return new ShapeDefinition($"json_{jsonValue.Name}:{jsonValue.VariableName}", elements);
        }

        /// <summary>
        ///     Builds an insert shape based on the given type and value.
        /// </summary>
        /// <param name="shapeType">The type to build the shape for.</param>
        /// <param name="shapeValue">The value to build the shape with.</param>
        /// <returns>The built insert shape.</returns>
        /// <exception cref="InvalidOperationException">
        ///     No serialization method could be found for a property.
        /// </exception>
        private ShapeDefinition BuildInsertShape(Type? shapeType = null, object? shapeValue = null)
        {
            List<ShapeSetter> setters = new();

            // use the provide shape and value if they're not null, otherwise
            // use the ones defined in context
            var type = shapeType ?? OperatingType;
            var value = shapeValue ?? Context.Value;

            // if the value is an expression we can just directly translate it
            if (value is LambdaExpression expression)
            {
                return new ShapeDefinition($"type_{type.GetEdgeDBTypeName()}:{type}", writer =>
                {
                    writer.Append('{');
                    TranslateExpression(expression, writer);
                    writer.Append('}');
                });
            }

            // get all properties that aren't marked with the EdgeDBIgnore attribute
            var map = EdgeDBPropertyMapInfo.Create(type);

            foreach (var property in map.Properties)
            {
                // define the type and whether or not it's a link
                var propValue = property.PropertyInfo.GetValue(value);
                var isScalar = EdgeDBTypeUtils.TryGetScalarType(property.Type, out var edgeqlType);

                if (property.CustomConverter is not null)
                {
                    // convert it and parameterize it
                    if (!EdgeDBTypeUtils.TryGetScalarType(property.CustomConverter.Target, out var scalar))
                        throw new ArgumentException(
                            $"Cannot resolve scalar type for {property.CustomConverter.Target}");
                    propValue = property.CustomConverter.ConvertTo(propValue);

                    var varName = QueryUtils.GenerateRandomVariableName();
                    SetVariable(varName, propValue);

                    setters.Add(new ShapeSetter(writer => writer
                        .Append(property.EdgeDBName)
                        .Append(" := ")
                        .QueryArgument(new(scalar), varName)
                    ));
                    continue;
                }

                // if its a default value of a struct, ignore it.
                if (isScalar &&
                    property.Type is {IsValueType: true, IsEnum: false} &&
                    (propValue?.Equals(ReflectionUtils.GetDefault(property.Type)) ?? false))
                {
                    setters.Add(new ShapeSetter((writer, info) =>
                    {
                        if (!info.TryGetObjectInfo(type!, out var typeInfo))
                            throw new InvalidOperationException(
                                $"Could not find {type!.GetEdgeDBTypeName()} in schema info!"
                            );

                        var edgedbProp = typeInfo.Properties!.FirstOrDefault(x => x.Name == property.EdgeDBName);

                        if (edgedbProp is null)
                            throw new InvalidOperationException(
                                $"Could not find property '{property.EdgeDBName}' on type {type!.GetEdgeDBTypeName()}"
                            );

                        if (edgedbProp.Required && !edgedbProp.HasDefault)
                        {
                            var varName = QueryUtils.GenerateRandomVariableName();
                            SetVariable(varName, propValue);

                            writer
                                .Append(property.EdgeDBName)
                                .Append(" := ")
                                .QueryArgument(new(edgeqlType), varName);
                        }
                    }));
                    continue;
                }

                var isLink = EdgeDBTypeUtils.IsLink(property.Type, out var isArray, out var innerType);

                // if a scalar type is found for the property type
                if (isScalar)
                {
                    // set it as a variable and continue the iteration
                    var varName = QueryUtils.GenerateRandomVariableName();
                    SetVariable(varName, propValue);
                    setters.Add(new ShapeSetter(writer => writer
                        .Append(property.EdgeDBName)
                        .Append(" := ")
                        .QueryArgument(new(edgeqlType), varName)
                    ));
                    continue;
                }

                // if the property is a link
                if (isLink)
                {
                    // if its null we can append an empty set
                    if (propValue is null)
                        setters.Add(new ShapeSetter(writer => writer
                            .Append(property.EdgeDBName).Append(" := {}")
                        ));
                    else if (isArray) // if its a multi link
                    {
                        if (propValue is not IEnumerable enumerable)
                            throw new InvalidOperationException(
                                $"Expected enumerable type for array, got {propValue.GetType()}"
                            );

                        setters.Add(new ShapeSetter(writer => writer
                            .Append(property.EdgeDBName)
                            .Append(" := ")
                            .Shape($"shape_{type.GetEdgeDBTypeName()}_prop_{property.EdgeDBName}", enumerable.Cast<object?>().ToArray(), (w, x) =>
                                BuildLinkResolver(w, innerType!, x)
                            )));
                    }
                    else // generate the link resolver and append it
                        setters.Add(new ShapeSetter(writer =>
                        {
                            writer
                                .Append(property.EdgeDBName)
                                .Append(" := ");

                            BuildLinkResolver(writer, property.Type, propValue);
                        }));

                    continue;
                }

                throw new InvalidOperationException(
                    $"Failed to find method to serialize the property \"{property.Type.Name}\" on type {type.Name}");
            }

            return new ShapeDefinition($"type_{type.GetEdgeDBTypeName()}:{type}", setters);
        }

        /// <summary>
        ///     Resolves a sub query for a link.
        /// </summary>
        /// <param name="writer">The query string writer to append the link resolver to.</param>
        /// <param name="type">The type of the link</param>
        /// <param name="value">The value of the link.</param>
        /// <returns>
        ///     A sub query or global name to reference the links value within the query.
        /// </returns>
        private void BuildLinkResolver(QueryWriter writer, Type type, object? value)
        {
            // if the value is null we can just return an empty set
            if (value is null)
            {
                writer.Append("{}");
                return;
            }

            // TODO: revisit references.
            //// is it a value that's been returned from a previous query?
            //if (QueryObjectManager.TryGetObjectId(value, out var id))
            //{
            //    // add a sub select statement
            //    return InlineOrGlobal(
            //        type,
            //        new SubQuery($"(select {type.GetEdgeDBTypeName()} filter .id = <uuid>\"{id}\")"),
            //        value);
            //}

            RequiresIntrospection = true;

            // add a insert select statement
            InlineOrGlobal(writer, type, new SubQuery((info, subqueryWriter) =>
            {
                var name = type.GetEdgeDBTypeName();
                var exclusiveProps = QueryGenerationUtils.GetProperties(info, type, true).ToArray();

                subqueryWriter
                    .Append("(insert ", name);

                BuildInsertShape(type, value).Build(subqueryWriter, info);

                if (!exclusiveProps.Any()) return;

                subqueryWriter
                    .Append(" unless conflict on ");

                if (exclusiveProps.Length is 1)
                    subqueryWriter.Append('.', exclusiveProps[0].GetEdgeDBPropertyName());
                else
                    subqueryWriter.Shape(
                        $"shape_{type.GetEdgeDBTypeName()}_prop_{name}",
                        exclusiveProps,
                        (writer, x) => writer.Append('.', x.GetEdgeDBPropertyName()),
                        "()"
                    );

                subqueryWriter.Append(" else (select ", name, ')');
            }), value);
        }

        /// <summary>
        ///     Adds a sub query as an inline query or as a global depending on if the current
        ///     query contains any statements for the provided type.
        /// </summary>
        /// <param name="writer">The query string writer to append the inlined query or global.</param>
        /// <param name="type">The returning type of the sub query.</param>
        /// <param name="value">The query itself.</param>
        /// <param name="reference">The optional reference object.</param>
        /// <returns>
        ///     A sub query or global name to reference the sub query.
        /// </returns>
        private void InlineOrGlobal(QueryWriter writer, Type type, SubQuery value, object? reference)
        {
            // if were in a query with the type or the query requires introspection add it as a global
            if (_subQueryMap.Contains(type) || value.RequiresIntrospection)
            {
                writer.Marker(MarkerType.Global, GetOrAddGlobal(reference, value));
                return;
            }

            // add it to our sub query map and return the inlined version
            _subQueryMap.Add(type);
            writer.Append(value.Query);
        }

        /// <summary>
        ///     Adds a unless conflict on (...) statement to the insert node
        /// </summary>
        /// <remarks>
        ///     This method requires introspection on the <see cref="FinalizeQuery"/> step.
        /// </remarks>
        public void UnlessConflict()
        {
            _autogenerateUnlessConflict = true;
            RequiresIntrospection = true;
        }

        /// <summary>
        ///     Adds a unless conflict on statement to the insert node
        /// </summary>
        /// <param name="selector">The property selector for the conflict clause.</param>
        public void UnlessConflictOn(LambdaExpression selector)
        {
            _unlessConflictExpression = selector;
        }

        /// <summary>
        ///     Adds the default else clause to the insert node that returns the conflicting object.
        /// </summary>
        public void ElseDefault()
        {
            if (_elseStatement is not null)
                throw new InvalidOperationException("An insert statement may only contain one else statement");

            _elseStatement = writer => writer
                .Append(" else ")
                .Wrapped(writer => writer
                    .Append("select ")
                    .Append(OperatingType.GetEdgeDBTypeName())
                );
        }

        /// <summary>
        ///     Adds a else statement to the insert node.
        /// </summary>
        /// <param name="builder">The builder that contains the else statement.</param>
        public void Else(IQueryBuilder builder)
        {
            if (_elseStatement is not null)
                throw new InvalidOperationException("An insert statement may only contain one else statement");

            _elseStatement = writer => writer
                .Append(" else ")
                .Wrapped(writer =>
                    builder.WriteTo(writer, this, new CompileContext()
                    {
                        IncludeAutogeneratedNodes = false
                    })
                );

            // // remove addon & autogen nodes.
            // var userNodes = builder.Nodes
            //     .Where(x => !builder.Nodes.Any(y => y.SubNodes.Contains(x)) || !x.IsAutoGenerated)
            //     .ToArray();
            //
            // // TODO: better checks for this, future should add a callback to add the
            // // node with its context so any parent builder can change contexts for nodes
            // foreach (var node in userNodes)
            //     node.Context.SetAsGlobal = false;
            //
            // foreach (var variable in builder.Variables)
            // {
            //     Builder.QueryVariables[variable.Key] = variable.Value;
            // }
            //
            // var newBuilder = new QueryBuilder<object?>(
            //     userNodes.ToList(), builder.Globals.ToList(),
            //     builder.Variables.ToDictionary(x => x.Key, x => x.Value)
            // );
            //
            // var result = newBuilder.BuildWithGlobals();
            //
            // _elseStatement = writer => writer
            //     .Append(" else ")
            //     .Wrapped(result.Query);
        }
    }
}
