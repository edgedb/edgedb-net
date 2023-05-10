using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    internal static class ResultTypeBuilder
    {
        private static readonly AssemblyBuilder _assemblyBuilder;
        private static readonly ModuleBuilder _moduleBuilder;

        static ResultTypeBuilder()
        {
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new System.Reflection.AssemblyName("EdgeDB.Runtime"),
                AssemblyBuilderAccess.Run
            );

            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("TestResults");
        }

        [return: NotNullIfNotNull(nameof(name))]
        public static bool TryGetScalarType(string? name, [NotNullWhen(true)] out Type? type)
        {
            if(string.IsNullOrEmpty(name))
            {
                type = null;
                return false;
            }    

            if (name.Contains("::"))
                name = name.Split("::")[1];

            type = name switch
            {
                "bool" => typeof(bool),
                "bytes" => typeof(byte[]),
                "str" => typeof(string),
                "local_date" => typeof(DataTypes.LocalDate),
                "local_time" => typeof(DataTypes.LocalTime),
                "local_datetime" => typeof(DataTypes.LocalDateTime),
                "relative_duration" => typeof(DataTypes.RelativeDuration),
                "datetime" => typeof(DataTypes.DateTime),
                "duration" => typeof(DataTypes.Duration),
                "date_duration" => typeof(DataTypes.DateDuration),
                "float32" => typeof(string),
                "float64" => typeof(string),
                "int16" => typeof(short),
                "int32" => typeof(int),
                "int64" => typeof(long),
                "decimal" => typeof(string),
                "bigint" => typeof(BigInteger),
                "json" => typeof(DataTypes.Json),
                "uuid" => typeof(Guid),
                _ => null
            };

            return type is not null;
        }

        public static IEnumerable<Type> CreateResultTypes(object obj)
        {
            if (obj is object[] arr)
            {
                return arr.Cast<IResultNode>().SelectMany(x => CreateResultTypes(x, root: true));
            }
            else if (obj is IResultNode node)
                return CreateResultTypes(node, root: true);
            else
                throw new InvalidOperationException($"unknown object type '{obj.GetType().Name}'");
        }

        public static IEnumerable<Type> CreateResultTypes(IResultNode node, bool root = false)
        {
            switch (node.Type)
            {
                case "object":
                    foreach (var result in CreateConcreteDefinition(node))
                        yield return result;
                    break;
                case "tuple" or "namedtuple":
                    {
                        yield return typeof(TransientTuple);

                        IEnumerable<IResultNode> children = node.Type is "tuple"
                            ? (IResultNode[])node.Value!
                            : ((Dictionary<string, IResultNode>)node.Value!).Values;

                        var combos = CreateTupleDefinitions(children);

                        foreach (var combo in combos)
                        {
                            var arr = combo.ToArray();

                            yield return TransientTuple.CreateValueTupleType(arr);
                            yield return TransientTuple.CreateReferenceTupleType(arr);
                        }

                        if (node.Type is "namedtuple")
                            yield return typeof(Dictionary<string, object>);
                    }
                    break;
                case "range":
                    yield return node.Value!.GetType();
                    if (node.Value.GetType() == typeof(EdgeDB.DataTypes.Range<int>))
                        yield return typeof(System.Range);
                    break;
                case "set" when root:
                    {
                        // sets of sets are collapsed into one set
                        if (node.Value is not Array arr)
                            throw new InvalidOperationException("Node value must be an array");
                        var elementTypes = new List<Type>();

                        if (arr.Length == 0)
                            elementTypes.Add(typeof(object));
                        else
                        {
                            // peek child
                            elementTypes.AddRange(CreateResultTypes((IResultNode)arr.GetValue(0)!));
                        }

                        foreach (var elementType in elementTypes)
                            yield return elementType;
                    }
                    break;
                case "set" or "array" when !root || node.Type != "set":
                    {
                        if (node.Value is not Array arr)
                            throw new InvalidOperationException("Node value must be an array");

                        var elementTypes = new List<Type>();

                        if (arr.Length == 0)
                            elementTypes.Add(typeof(object));
                        else
                        {
                            // peek child
                            elementTypes.AddRange(CreateResultTypes((IResultNode)arr.GetValue(0)!));
                        }

                        foreach(var elementType in elementTypes)
                        {
                            yield return typeof(List<>).MakeGenericType(elementType);
                            yield return Array.CreateInstance(elementType, 0).GetType();
                            yield return typeof(IEnumerable<>).MakeGenericType(elementType);
                        }
                    }
                    break;
                case "cal::date_duration":
                    yield return typeof(DataTypes.DateDuration);
                    yield return typeof(TimeSpan);
                    break;
                case "std::datetime":
                    yield return typeof(DataTypes.DateTime);
                    yield return typeof(System.DateTime);
                    yield return typeof(System.DateTimeOffset);
                    break;
                case "std::duration":
                    yield return typeof(DataTypes.Duration);
                    yield return typeof(TimeSpan);
                    break;
                case "cal::local_date":
                    yield return typeof(DataTypes.LocalDate);
                    yield return typeof(DateOnly);
                    break;
                case "cal::local_datetime":
                    yield return typeof(DataTypes.LocalDateTime);
                    yield return typeof(System.DateTime);
                    yield return typeof(System.DateTimeOffset);
                    break;
                case "cal::local_time":
                    yield return typeof(DataTypes.LocalTime);
                    yield return typeof(TimeOnly);
                    yield return typeof(TimeSpan);
                    break;
                case "cal::relative_duration":
                    yield return typeof(DataTypes.RelativeDuration);
                    yield return typeof(TimeSpan);
                    break;
                default:
                    if (node.Value is IResultNode i)
                        throw new InvalidOperationException($"Expecting scalar node, but got node value of {i.Type}");
                    yield return node.Value!.GetType();
                    break;
            }
        }

        private static IEnumerable<Type> CreateConcreteDefinition(IResultNode node)
        {
            if (node.Value is not Dictionary<string, IResultNode> dict)
                throw new InvalidOperationException("Value must be a dictionary");

            foreach(var propertyDef in CreatePropertyDefinitions(dict))
            {
                var type = _moduleBuilder.DefineType(
                    GetTypeName(node),
                    TypeAttributes.Public | TypeAttributes.Class
                );

                foreach(var prop in propertyDef)
                {
                    DefineDefaultProperty(prop.Key, prop.Value, type);
                }

                yield return type.CreateType();
            }
        }

        private static void DefineDefaultProperty(string name, Type type, System.Reflection.Emit.TypeBuilder builder)
        {
            var prop = builder.DefineProperty(name, PropertyAttributes.None, type, null);

            var backingField = builder.DefineField($"m_{name}", type, FieldAttributes.Private);

            var getter = builder.DefineMethod(
                $"get_{name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                type,
                Type.EmptyTypes
            );

            var getterIL = getter.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, backingField);
            getterIL.Emit(OpCodes.Ret);

            var setter = builder.DefineMethod(
                $"set_{name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new Type[] {type}
            );

            var setterIL = setter.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, backingField);
            setterIL.Emit(OpCodes.Ret);

            prop.SetSetMethod(setter);
            prop.SetGetMethod(getter);
        }

        private static List<IEnumerable<Type>> CreateTupleDefinitions(IEnumerable<IResultNode> nodes)
        {
            var result = new List<IEnumerable<Type>>();

            foreach(var node in nodes)
            {
                var types = CreateResultTypes(node);

                foreach(var type in types)
                {
                    result.AddRange(result.Select(x => new List<Type>(x) { type }));
                }
            }

            return result;
        }

        private static List<Dictionary<string, Type>> CreatePropertyDefinitions(Dictionary<string, IResultNode> props)
        {
            var maps = new List<Dictionary<string, Type>>();

            foreach(var prop in props)
            {
                var types = CreateResultTypes(prop.Value);
                var dict = new List<Dictionary<string, Type>>();

                if(maps.Any())
                {
                    foreach (var map in maps)
                    {
                        foreach (var type in types)
                        {
                            dict.Add(new Dictionary<string, Type>(map) { { prop.Key, type } });
                        }
                    }
                }
                else
                {
                    foreach (var type in types)
                    {
                        dict.Add(new Dictionary<string, Type>() { { prop.Key, type } });
                    }
                }

                maps = dict;
            }

            return maps;
        }

        private static string GetTypeName(IResultNode node)
        {
            var sb = new StringBuilder();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            sb.Append(Enumerable.Repeat(chars, 10)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());

            sb.Append(node.GetHashCode());

            var str = sb.ToString();
            return str;
        }
    }
}
