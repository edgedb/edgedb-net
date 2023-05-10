using EdgeDB.DataTypes;
using EdgeDB.TestGenerator.ValueProviders;
using EdgeDB.TestGenerator.ValueProviders.Impl;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace EdgeDB.TestGenerator
{
    internal class ValueFormatter
    {
        public static async Task<INode> FormatAsync(object edgedbValue, object generatedValue, Action progress, IValueProvider provider, IWrappingValueProvider? parent = null)
        {
            var name = provider.EdgeDBName;

            try
            {
                switch (name)
                {
                    case "namedtuple":
                        {
                            if (provider is not IWrappingValueProvider wrapping)
                                throw new InvalidOperationException("provider is not a wrapping provider");

                            if (generatedValue is not Dictionary<string, object> generatedDict)
                                throw new InvalidCastException("generated value is not a dictionary");

                            var generatedArr = generatedDict.ToArray();

                            // this value could be a set of tuples due to cartesian product of arguments.
                            if (edgedbValue is Array arr)
                            {
                                return new CollectionFormatNode
                                {
                                    ElementType = "namedtuple",
                                    Type = "set",
                                    Value = await FormatCollectionAsync(arr, (v, i) => FormatAsync(v, generatedDict, progress, provider, wrapping))
                                };
                            }

                            // named tuples can be flattened to normal tuples if the names don't match
                            if(edgedbValue is TransientTuple transientTuple && provider is not IIdenticallyNamedProvider)
                            {
                                // TODO: does edgedb ensure that tuple arity is maintained? 

                                return new FormatNode
                                {
                                    Type = "tuple",
                                    Value = await FormatCollectionAsync(
                                        transientTuple,
                                        (v, i) => FormatAsync(v, generatedArr[i].Value, progress, wrapping.Children![i], wrapping)
                                    )
                                };
                            }

                            if (edgedbValue is not IDictionary<string, object> dict)
                                throw new InvalidOperationException("value is not a dictionary");

                            var dictArr = dict.ToArray();

                            if (dictArr.Length != wrapping.Children!.Length)
                                throw new IndexOutOfRangeException("Data doesn't match children of wrapping provider");

                            return new FormatNode
                            {
                                Type = provider.EdgeDBName,
                                Value = new Dictionary<string, INode>(await FormatCollectionAsync(
                                    dictArr,
                                    (v, i) =>
                                        FormatAsync(v.Value, generatedArr[i].Value, progress, wrapping.Children[i], wrapping)
                                        .ContinueWith(r => new KeyValuePair<string, INode>(v.Key, r.Result))
                                ))
                            };
                        }
                    case "object":
                        {
                            if (provider is not IWrappingValueProvider wrapping)
                                throw new InvalidOperationException("provider is not a wrapping provider");

                            if (generatedValue is not Dictionary<string, object> generatedDict)
                                throw new InvalidCastException("generated value is not a dictionary");

                            var generatedArr = generatedDict.ToArray();

                            if (edgedbValue is not IDictionary<string, object> dict)
                                throw new InvalidOperationException("value is not a dictionary");

                            var dictArr = dict.ToArray();

                            if (dictArr.Length != wrapping.Children!.Length)
                                throw new IndexOutOfRangeException("Data doesn't match children of wrapping provider");

                            return new FormatNode
                            {
                                Type = provider.EdgeDBName,
                                Value = new Dictionary<string, INode>(await FormatCollectionAsync(
                                    dictArr,
                                    (v, i) =>
                                        FormatAsync(v.Value, generatedArr[i].Value, progress, wrapping.Children[i], wrapping)
                                        .ContinueWith(r => new KeyValuePair<string, INode>(v.Key, r.Result))
                                ))
                            };
                        }
                    case "tuple":
                        {
                            if (provider is not IWrappingValueProvider wrapping)
                                throw new InvalidOperationException("provider is not a wrapping provider");

                            if (generatedValue is not ITuple generatedTuple)
                                throw new InvalidOperationException("generated value is not a tuple");

                            // this value could be a set of tuples due to cartesian product of arguments.
                            if (edgedbValue is Array arr && arr.GetType().GetElementType()!.IsAssignableTo(typeof(ITuple)))
                            {
                                return new CollectionFormatNode
                                {
                                    ElementType = "tuple",
                                    Type = "set",
                                    Value = await FormatCollectionAsync(arr, (v, i) => FormatAsync(v, generatedTuple, progress, provider, wrapping))
                                };
                            }

                            if (edgedbValue is not ITuple tuple)
                            {
                                if (generatedTuple.Length == 1)
                                {
                                    // edgedb flattens the tuple to the inner value if
                                    // theres only one element

                                    return await FormatAsync(edgedbValue, generatedTuple[0]!, progress, wrapping.Children![0], wrapping);
                                }

                                throw new InvalidOperationException("value is not a tuple");
                            }

                            return new FormatNode
                            {
                                Type = provider.EdgeDBName,
                                Value = await FormatCollectionAsync(
                                    tuple,
                                    (v, i) => FormatAsync(v, generatedTuple[i]!, progress, wrapping.Children![i], wrapping)
                                )
                            };
                        }
                    case "set" or "array":
                        {
                            if (provider is not IWrappingValueProvider wrapping)
                                throw new InvalidOperationException("provider is not a wrapping provider");

                            var generatedArray = AsArray(generatedValue);

                            if (provider.EdgeDBName is "set")
                            {
                                if (parent is not null && (generatedArray.Length == 1 || parent?.EdgeDBName is "tuple" or "namedtuple" or "set"))
                                {
                                    // sets are flattened into one set,
                                    // or it could be a single element due to cartesian product of arguments,
                                    // or the single element set is flattened to the single value.
                                    return await FormatAsync(edgedbValue, generatedArray, progress, wrapping.Children![0], wrapping);
                                }
                                else if(parent is null && wrapping.Children![0].EdgeDBName == "set")
                                {
                                    var elementChild = wrapping.Children[0];

                                    while (elementChild is SetValueProvider set)
                                        elementChild = set.Children![0];

                                    var generatedArrayFlattened = new List<object?>();

                                    void AddElement(object? element)
                                    {
                                        Array? arr = element is Array a
                                            ? a
                                            : element is IEnumerable e
                                                ? e.Cast<object?>().ToArray()
                                                : null;

                                        if (arr is not null)
                                        {
                                            for (int i = 0; i != arr.Length; i++)
                                                AddElement(arr.GetValue(i));
                                        }
                                        else
                                            generatedArrayFlattened!.Add(element);
                                    }

                                    for (int i = 0; i != generatedArray.Length; i++)
                                        AddElement(generatedArray.GetValue(i));

                                    return new CollectionFormatNode()
                                    {
                                        ElementType = elementChild.EdgeDBName,
                                        Type = wrapping.EdgeDBName,
                                        Value = await FormatCollectionAsync(
                                            AsArray(edgedbValue),
                                            (v, i) => FormatAsync(v, generatedArrayFlattened[i]!, progress, elementChild, wrapping))
                                    };

                                    //return await FormatAsync(edgedbValue, generatedArray, progress, wrapping.Children![0], wrapping);
                                } 
                            }

                            var array = AsArray(edgedbValue);

                            if (array.Length != generatedArray.Length)
                                throw new IndexOutOfRangeException("Generated and resulting array lengths don't match");

                            var elementProvider = wrapping.Children![0];

                            return new CollectionFormatNode()
                            {
                                ElementType = elementProvider.EdgeDBName,
                                Type = wrapping.EdgeDBName,
                                Value = await FormatCollectionAsync(
                                    array,
                                    (v, i) => FormatAsync(v, generatedArray.GetValue(i)!, progress, elementProvider, wrapping)
                                )
                            };
                        }
                    case "range":
                        {
                            if (provider is not IWrappingValueProvider wrapping)
                                throw new InvalidOperationException("provider is not a wrapping provider");

                            return new CollectionFormatNode
                            {
                                Type = provider.EdgeDBName,
                                ElementType = wrapping.Children![0].EdgeDBName,
                                Value = edgedbValue
                            };
                        }
                    default:
                        {
                            return new FormatNode
                            {
                                Type = provider.EdgeDBName,
                                Value = edgedbValue
                            };
                        }
                }
            }
            finally
            {
                progress.Invoke();
            }
        }

        private static Array AsArray(object obj)
        {
            if (obj is Array arr)
                return arr;

            if (obj is IEnumerable<object> enumerable)
                return enumerable.ToArray();

            throw new InvalidOperationException($"object {obj.GetType()} is not an array");
        }

        private static async Task<T[]> FormatCollectionAsync<T>(Array array, Func<object, int, Task<T>> format)
        {
            var tasks = new Task<T>[array.Length];
            for(int i = 0; i != array.Length; i++)
            {
                tasks[i] = format(array.GetValue(i)!, i);
            }

            return await Task.WhenAll(tasks);
        }

        private static async Task<T[]> FormatCollectionAsync<T, U>(U[] array, Func<U, int, Task<T>> format)
        {
            var tasks = new Task<T>[array.Length];
            for (int i = 0; i != array.Length; i++)
            {
                tasks[i] = format(array[i], i);
            }

            return await Task.WhenAll(tasks);
        }

        private static async Task<T[]> FormatCollectionAsync<T>(ITuple array, Func<object, int, Task<T>> format)
        {
            var tasks = new Task<T>[array.Length];
            for (int i = 0; i != array.Length; i++)
            {
                tasks[i] = format(array[i]!, i);
            }

            return await Task.WhenAll(tasks);
        }
    }

    public interface INode
    {
        string Type { get; }
        object Value { get; }
    }

    public readonly struct FormatNode : INode
    {
        public required string Type { get; init; }
        public required object Value { get; init; }
    }

    public readonly struct CollectionFormatNode : INode
    {
        public required string Type { get; init; }
        public required object Value { get; init; }
        public required string ElementType { get; init; }
    }
}

