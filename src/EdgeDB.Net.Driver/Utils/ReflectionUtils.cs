using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class ReflectionUtils
    {
        internal static ModuleBuilder? ModuleBuilder;

        public static bool IsSubclassOfRawGeneric(Type generic, Type? toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static TypeBuilder GetTypeBuilder(string name, TypeAttributes? attributes = null)
        {
            if (ModuleBuilder == null)
            {
                var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("EdgeDB.Runtime")
                {
                    Version = Assembly.GetExecutingAssembly().GetName().Version,
                }, AssemblyBuilderAccess.Run);

                ModuleBuilder = assembly.DefineDynamicModule("MainModule");
            }

            attributes ??= TypeAttributes.Public |
                   TypeAttributes.Class |
                   TypeAttributes.AutoClass |
                   TypeAttributes.AnsiClass |
                   TypeAttributes.BeforeFieldInit |
                   TypeAttributes.AutoLayout;

            return ModuleBuilder.DefineType(name, attributes.Value, null);
        }

        public static PropertyBuilder? CreateProperty(TypeBuilder tb, string propertyName, Type propertyType, MethodInfo? overrideGetMethod = null, MethodInfo? overrideSetMethod = null, bool newProp = false)
        {
            var fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            var flags = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            if (!newProp)
                flags |= MethodAttributes.Virtual;

            var getPropMthdBldr = tb.DefineMethod("get_" + propertyName, flags, propertyType, Type.EmptyTypes);
            var getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            var setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.Virtual |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            var setIl = setPropMthdBldr.GetILGenerator();
            var modifyProperty = setIl.DefineLabel();
            var exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);

            if (overrideGetMethod != null)
            {
                tb.DefineMethodOverride(getPropMthdBldr, overrideGetMethod);
            }

            if (overrideSetMethod != null)
            {
                tb.DefineMethodOverride(setPropMthdBldr, overrideSetMethod);
            }

            return propertyBuilder;
        }

        public static CustomAttributeBuilder? BuildCustomAttribute(object customAttribute)
        {
            ConstructorInfo? longestCtor = null;
            // Get constructor with the largest number of parameters
            foreach (var cInfo in customAttribute.GetType().GetConstructors().
                                                  Where(cInfo => longestCtor == null || longestCtor.GetParameters().Length < cInfo.GetParameters().Length))
                longestCtor = cInfo;

            if (longestCtor == null)
            {
                return null;
            }

            // For each constructor parameter, get corresponding (by name similarity) property and get its value
            var args = new object?[longestCtor.GetParameters().Length];
            var position = 0;
            foreach (var consParamInfo in longestCtor.GetParameters())
            {
                var attrPropInfo = customAttribute.GetType().GetProperty(consParamInfo.Name!, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (attrPropInfo != null)
                {
                    args[position] = attrPropInfo.GetValue(customAttribute, null);
                }
                else
                {
                    args[position] = null;
                    var attrFieldInfo = customAttribute.GetType().GetField(consParamInfo.Name!, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (attrFieldInfo == null)
                    {
                        if (consParamInfo.ParameterType.IsValueType)
                        {
                            args[position] = Activator.CreateInstance(consParamInfo.ParameterType);
                        }
                    }
                    else
                    {
                        args[position] = attrFieldInfo.GetValue(customAttribute);
                    }
                }
                ++position;
            }

            var propList = new List<PropertyInfo>();
            var propValueList = new List<object?>();
            foreach (var attrPropInfo in customAttribute.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!attrPropInfo.CanWrite)
                {
                    continue;
                }
                object? defaultValue = null;
                var defaultAttributes = attrPropInfo.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                if (defaultAttributes.Length > 0)
                {
                    defaultValue = ((DefaultValueAttribute)defaultAttributes[0]).Value;
                }
                var value = attrPropInfo.GetValue(customAttribute, null);
                if (value == defaultValue)
                {
                    continue;
                }
                propList.Add(attrPropInfo);
                propValueList.Add(value);
            }
            return new CustomAttributeBuilder(longestCtor, args, propList.ToArray(), propValueList.ToArray());
        }
    }
}
