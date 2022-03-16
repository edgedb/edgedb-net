using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class ObjectBuilder
    {
        private static Dictionary<(Guid TypeDescriptor, Guid TypeId), Type> _runtimeTypemap { get; set; } = new();
        private struct EdgeDBPropertyInfo
        {
            public string EdgeDBName { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
            public bool ShouldMakeSubType { get; set; }
        }

        public static TType? BuildResult<TType>(Guid typeDescriptorId, IDictionary<string, object?> rawResult)
            => (TType?)BuildResult(typeDescriptorId, typeof(TType), rawResult);

        public static TType? BuildResult<TType>(Guid typeDescriptorId, object? value)
        {
            if (value is IDictionary<string, object?> raw)
                return BuildResult<TType>(typeDescriptorId, raw);

            return (TType?)ConvertTo(typeDescriptorId, typeof(TType), value);
        }

        public static object? BuildResult(Guid typeDescriptorId, Type targetType, object? raw)
        {
            if (raw == null)
                return null;

            if (targetType == typeof(object))
                return raw;

            if (raw is not IDictionary<string, object?> rawResult)
                throw new ArgumentException($"Cannot use {raw.GetType()} for building");

            if (rawResult.GetType() == targetType)
                return rawResult;

            if (targetType.IsAssignableTo(typeof(IEnumerable)))
                return ConvertTo(typeDescriptorId, targetType, new object[] { rawResult });

            if (!IsValidTargetType(targetType))
                throw new EdgeDBException($"Invalid type {targetType} for building");



            // build our type properties
            List<EdgeDBPropertyInfo> properties = new();

            string typeName = targetType.GetCustomAttribute<EdgeDBType>()?.Name ?? targetType.Name;

            if (_runtimeTypemap.TryGetValue((typeDescriptorId, targetType.GUID), out var runtimeType))
                targetType = runtimeType;
            else
            {
                var builder = ReflectionUtils.GetTypeBuilder($"{targetType.Name}_Runtime_{typeDescriptorId}_{targetType.GUID}");
                builder.SetCustomAttribute(new CustomAttributeBuilder(typeof(EdgeDBType).GetConstructor(new Type[] { typeof(string) })!, new object[] { typeName }));
                builder.SetParent(targetType);
                // add id property if it isn't there with the QueryResult interface
                builder.AddInterfaceImplementation(typeof(DataTypes.IQueryResultObject));
                foreach (var item in targetType.GetProperties().Where(x => ShouldCreateSubType(x)))
                {
                    var propBuilder = ReflectionUtils.CreateProperty(builder, item.Name, item.PropertyType, targetType.GetMethod($"get_{item.Name}"), newProp: false);
                }

                // create the id property
                var idPropName = Guid.NewGuid().ToString().Replace("-", "");
                var idProp = ReflectionUtils.CreateProperty(builder, idPropName, typeof(Guid));
                idProp!.SetCustomAttribute(new CustomAttributeBuilder(typeof(EdgeDBProperty).GetConstructor(new Type[] { typeof(string)})!, new object[] { "id"}));

                var getObjectMethodImpl = builder.DefineMethod("EdgeDB.DataTypes.IQueryResultObject.GetObjectId",
                    MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
                    MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis, typeof(Guid), Type.EmptyTypes);

                var getObjectGen = getObjectMethodImpl.GetILGenerator();
                var objectLocal = getObjectGen.DeclareLocal(typeof(Guid));
                var objectEndlabel = getObjectGen.DefineLabel();
                getObjectGen.Emit(OpCodes.Nop);
                getObjectGen.Emit(OpCodes.Ldarg_0);
                getObjectGen.Emit(OpCodes.Call, idProp!.GetMethod!);
                getObjectGen.Emit(OpCodes.Stloc_0);
                getObjectGen.Emit(OpCodes.Br_S, objectEndlabel);
                getObjectGen.MarkLabel(objectEndlabel);
                getObjectGen.Emit(OpCodes.Ldloc_0);
                getObjectGen.Emit(OpCodes.Ret);

                builder.DefineMethodOverride(getObjectMethodImpl, typeof(DataTypes.IQueryResultObject).GetMethod("GetObjectId")!);

                var type = builder.CreateType()!;
                _runtimeTypemap.Add((typeDescriptorId, targetType.GUID), type);
                targetType = type;
            }

            foreach (var prop in targetType.GetProperties())
            {
                var shouldMakeSubType = ShouldCreateSubType(prop);
                if (!IsValidProperty(prop) && !shouldMakeSubType)
                    continue;

                var name = prop.GetCustomAttribute<EdgeDBProperty>()?.Name ?? prop.Name;

                //properties.Add(name, (type, obj) => type.SetValue(instance, ConvertTo(type.PropertyType, obj)));
                var propInfo = new EdgeDBPropertyInfo
                {
                    EdgeDBName = name,
                    PropertyInfo = prop,
                    ShouldMakeSubType = shouldMakeSubType
                };
                properties.Add(propInfo);
            }

            var instance = Activator.CreateInstance(targetType);

            var objectProps = targetType.GetProperties().OrderBy(x => x.DeclaringType == targetType ? 0 : 1);

            foreach (var result in rawResult)
            {
                var prop = properties.FirstOrDefault(x => x.EdgeDBName == result.Key);

                if (prop.EdgeDBName != null)
                {
                    var other = objectProps.FirstOrDefault(x => x.Name == prop.PropertyInfo.Name);
                    if (other != null)
                    {
                        other.SetValue(instance, ConvertTo(typeDescriptorId, other.PropertyType, result.Value));
                    }

                }
            }

            return instance;
        }

        private static bool ShouldCreateSubType(PropertyInfo info)
        {
            if (info.PropertyType.Name == "ComputedValue`1")
                return true;

            return false;
        }

        private static object? ConvertTo(Guid descriptorId, Type type, object? value)
        {
            if (value == null)
            {
                return GetDefault(type);
            }

            var valueType = value.GetType();

            if (valueType.IsAssignableTo(type))
                return value;

            // check for edgeql types
            if (type.GetCustomAttribute<EdgeDBType>() != null && value is IDictionary<string, object?> dict)
                return BuildResult(descriptorId, type, dict);

            // check for sets
            if (valueType.Name == typeof(DataTypes.Set<>).Name) // TODO: better compare
            {
                return ConvertCollection(descriptorId, type, valueType, value);
            }

            // check for arrays
            if (valueType.IsArray)
            {
                return ConvertCollection(descriptorId, type, valueType, value);
            }

            // check for computed values
            if (type.Name == "ComputedValue`1" && type.GenericTypeArguments[0] == valueType)
            {
                var method = type.GetRuntimeMethod("op_Implicit", new Type[] { valueType });

                if (method != null)
                    return method.Invoke(null, new object[] { value });
            }

            try
            {
                return Convert.ChangeType(value, type);
            }
            catch
            {
                try
                {
                    return DynamicCast(value, type);
                }
                catch { return value; }
            }
        }

        private static object? ConvertCollection(Guid descriptorId, Type targetType, Type valueType, object value)
        {
            try
            {
                List<object?> converted = new();
                var strongInnerType = targetType.GenericTypeArguments.FirstOrDefault();

                foreach (var val in (IEnumerable)value)
                {
                    if (val is IDictionary<string, object?> raw)
                    {
                        converted.Add(strongInnerType != null ? BuildResult(descriptorId, strongInnerType, raw) : val);
                    }
                    else
                        converted.Add(strongInnerType != null ? ConvertTo(descriptorId, strongInnerType, val) : val);

                }

                var arr = Array.CreateInstance(strongInnerType ?? valueType.GenericTypeArguments[0], converted.Count);
                Array.Copy(converted.ToArray(), arr, converted.Count);

                switch (targetType)
                {
                    case Type when targetType.Name == typeof(List<>).Name:
                        {
                            var l = typeof(List<>).MakeGenericType(strongInnerType ?? valueType.GenericTypeArguments[0]);
                            return Activator.CreateInstance(l, arr);
                        }
                    case Type when targetType.IsArray:
                        {
                            return arr;
                        }
                    case Type when targetType.Name == typeof(DataTypes.Set<>).Name:
                        {
                            var l = typeof(DataTypes.Set<>).MakeGenericType(strongInnerType ?? valueType.GenericTypeArguments[0]);
                            return Activator.CreateInstance(l, arr, true);
                        }
                    default:
                        {
                            if (arr.GetType().IsAssignableTo(targetType))
                                return DynamicCast(arr, targetType);

                            throw new EdgeDBException($"Couldn't convert {valueType} to {targetType}");
                        }
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x);
                return null;
            }
        }

        private static object? DynamicCast(object? entity, Type to)
            => typeof(ObjectBuilder).GetMethod("Cast", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(to).Invoke(null, new object?[] { entity });

        private static T? Cast<T>(object? entity)
            => (T?)entity;

        private static object? GetDefault(Type t)
            => typeof(ObjectBuilder).GetMethod("GetDefaultGeneric", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(t).Invoke(null, null);

        private static object? GetDefaultGeneric<T>() => default(T);

        private static bool IsValidProperty(PropertyInfo type)
        {
            var shouldIgnore = type.GetCustomAttribute<EdgeDBIgnore>() != null;

            return !shouldIgnore && type.GetSetMethod() != null;
        }

        private static bool IsValidTargetType(Type type)
        {
            // TODO: check constructor
            return type.IsPublic && (type.IsClass || type.IsValueType) && !type.IsSealed;
        }
    }
}
