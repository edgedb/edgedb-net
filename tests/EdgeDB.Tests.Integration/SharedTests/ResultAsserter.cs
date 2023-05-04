using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    internal static class ResultAsserter
    {
        public static void AssertResult(object? expected, object? actual)
        {
            if(expected is IEnumerable)
            {
                var col = AssertCollection<IResultNode>(expected, actual);

                foreach(var item in col)
                {
                    AssertNode(item.Expected, item.Actual);
                }
            }
            else if (expected is IResultNode node)
            {
                AssertNode(node, actual);
            }
        }

        private static void AssertNode(IResultNode node, object? actual)
        {
            switch (node.Type)
            {
                case "object":
                    {
                        Assert.IsNotNull(node.Value);
                        Assert.IsNotNull(actual);

                        if (node.Value is not Dictionary<string, IResultNode> expectedDict)
                            throw new AssertFailedException($"Expected dictionary, got {node.Value.GetType().Name}");

                        var actualDict = ExtractObjectDictionary(actual);

                        Assert.AreEqual(expectedDict.Count, actualDict.Count);

                        var values = expectedDict.Zip(actualDict, (a, b) => (Expected: a.Value, Actual: b.Value));

                        foreach (var value in values)
                        {
                            AssertNode(value.Expected, value.Actual);
                        }
                    }
                    break;
                case "tuple":
                    {
                        Assert.IsNotNull(node.Value);
                        Assert.IsNotNull(actual);

                        var nodes = ((object?[])node.Value).Cast<IResultNode>();
                        var collection = ExtractCollection(actual);

                        Assert.AreEqual(nodes.Count(), collection.Count());

                        foreach (var value in nodes.Zip(collection))
                            AssertNode(value.First, value.Second);
                    }
                    break;
            }
        }

        private static IEnumerable<object?> ExtractCollection(object? actual)
        {
            Assert.IsNotNull(actual);

            if (actual is ITuple tuple)
            {
                for (int i = 0; i != tuple.Length; i++)
                    yield return tuple[i];
            }
            else if (actual is object?[] arr)
            {
                for (int i = 0; i != arr.Length; i++)
                    yield return arr[i];
            }
            else if (actual is IEnumerable e)
            {
                var enumerator = e.GetEnumerator();
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
            else
                throw new AssertFailedException($"Expected collection-like object, got {actual.GetType().Name}");
        }

        private static Dictionary<string, object?> ExtractObjectDictionary(object? actual)
        {
            Assert.IsNotNull(actual);

            if (actual is Dictionary<string, object?> dict)
                return dict;

            var type = actual.GetType();

            if (type.IsAssignableTo(typeof(IEnumerable<KeyValuePair<string, object?>>)))
                return new Dictionary<string, object?>((IEnumerable<KeyValuePair<string, object?>>)actual);

            if(type.Module.Name == "TestResults")
            {
                var props = type.GetProperties();

                return props.ToDictionary(x => x.Name, x => x.GetValue(actual));
            }

            throw new AssertFailedException($"Expected object-like type, but got {type}");
        }

        private static IEnumerable<(T Expected, object? Actual)> AssertCollection<T>(object? expected, object? actual)
        {
            Assert.IsNotNull(actual);
            Assert.IsNotNull(expected);

            if (expected is not IEnumerable a)
            {
                Assert.Fail("Expected is not a collection type");
                return null!;
            }

            if (actual is not IEnumerable b)
            {
                Assert.Fail("Actual is not a collection type");
                return null!;
            }

            var ac = a.Cast<T>();
            var bc = b.Cast<object?>();

            Assert.AreEqual(ac.Count(), bc.Count());

            return ac.Zip(bc);
        }
    }
}
