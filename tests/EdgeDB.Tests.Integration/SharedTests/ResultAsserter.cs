using EdgeDB.DataTypes;
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
        private interface IReducerContainer
        {
            Type ReducingTo { get; }
            bool CanReduce(Type a, Type b);
            (object? First, object? Second) Reduce(object? a, object? b);
        }
        private readonly struct ReducerContainer<T> : IReducerContainer
        {
            private readonly Dictionary<Type, IReducer> _reducers;

            public interface IReducer
            {
                Type Type { get; }
                T Reduce(object? value);
            }

            public readonly struct Reducer<U> : IReducer
            {
                private readonly Func<U?, T> _reducer;

                public Reducer(Func<U?, T> reducer)
                {
                    _reducer = reducer;
                }

                public T Reduce(U? value)
                   => _reducer(value);

                public Type Type
                    => typeof(U);

                public T Reduce(object? value)
                    => Reduce((U?)value);
            }

            public ReducerContainer(List<IReducer> reducers)
            {
                _reducers = reducers.ToDictionary(x => x.Type, x => x);
            }

            public bool CanReduce(Type a, Type b)
                => _reducers.ContainsKey(a) && _reducers.ContainsKey(b);

            public (T First, T Second) Reduce(object? first, object? second)
                => (_reducers[first!.GetType()].Reduce(first), _reducers[second!.GetType()].Reduce(second));

            (object? First, object? Second) IReducerContainer.Reduce(object? a, object? b) => Reduce(a, b);

            public Type ReducingTo
                => typeof(T);
        }

        private static readonly List<IReducerContainer> _reducers;

        static ResultAsserter()
        {
            // TODO: reduce system & custom temporal types to precision scalar, then compare them
            _reducers = new List<IReducerContainer>()
            {
                new ReducerContainer<long>(new List<ReducerContainer<long>.IReducer>() // datetime microseconds
                {
                    new ReducerContainer<long>.Reducer<DataTypes.DateTime>(v => v.Microseconds),
                    new ReducerContainer<long>.Reducer<DataTypes.LocalDateTime>(v => v.Microseconds),
                    new ReducerContainer<long>.Reducer<System.DateTime>(v => TemporalCommon.ToMicroseconds(v)),
                    new ReducerContainer<long>.Reducer<System.DateTimeOffset>(TemporalCommon.ToMicroseconds)
                }),
                new ReducerContainer<int>(new List<ReducerContainer<int>.IReducer>() // date days
                {
                    new ReducerContainer<int>.Reducer<DataTypes.LocalDate>(v => v.Days),
                    new ReducerContainer<int>.Reducer<System.DateOnly>(v => TemporalCommon.ToDays(v))
                }),
                new ReducerContainer<long>(new List<ReducerContainer<long>.IReducer>() // time microseconds
                {
                    new ReducerContainer<long>.Reducer<DataTypes.LocalTime>(v => v.Microseconds),
                    new ReducerContainer<long>.Reducer<System.TimeOnly>(TemporalCommon.ToMicroseconds),
                    new ReducerContainer<long>.Reducer<System.TimeSpan>(TemporalCommon.ToMicroseconds)
                }),
                new ReducerContainer<long>(new List<ReducerContainer<long>.IReducer>() // duration microseconds
                {
                    new ReducerContainer<long>.Reducer<DataTypes.Duration>(v => v.Microseconds),
                    new ReducerContainer<long>.Reducer<System.TimeSpan>(TemporalCommon.ToMicroseconds)
                }),
                new ReducerContainer<long>(new List<ReducerContainer<long>.IReducer>() // duration days+months
                {
                    new ReducerContainer<long>.Reducer<DataTypes.DateDuration>(v => v.TimeSpan.Ticks),
                    new ReducerContainer<long>.Reducer<System.TimeSpan>(v => v.Ticks)
                }),
                new ReducerContainer<long>(new List<ReducerContainer<long>.IReducer>()
                {
                    new ReducerContainer<long>.Reducer<DataTypes.RelativeDuration>(v => (long)(v.Microseconds + (v.Days + v.Months * 31) * 8.64e+10)),
                    new ReducerContainer<long>.Reducer<System.TimeSpan>(v =>
                    {
                        var (microseconds, days, months) = TemporalCommon.ToComponents(v);
                        return (long)(microseconds + (days + months * 31) * 8.64e+10);
                    })
                }),
                new ReducerContainer<(int a, int b)?>(new List<ReducerContainer<(int a, int b)?>.IReducer>
                {
                    new ReducerContainer<(int a, int b)?>.Reducer<DataTypes.Range<int>>(v => v.Lower.HasValue && v.Upper.HasValue ? (v.Lower.Value, v.Upper.Value) : null),
                    new ReducerContainer<(int a, int b)?>.Reducer<Range>(v => (v.Start.Value, v.End.Value))
                })
            };
        }

        public static void AssertResult(object? expected, object? actual)
        {
            if(expected is IEnumerable)
            {
                var col = AssertCollection<IResultNode>(expected, actual);

                foreach(var (Expected, Actual) in col)
                {
                    AssertNode(Expected, Actual);
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

                        foreach (var (Expected, Actual) in values)
                        {
                            AssertNode(Expected, Actual);
                        }
                    }
                    break;
                case "namedtuple":
                    {
                        Assert.IsNotNull(node.Value);
                        Assert.IsNotNull(actual);

                        if (node.Value is not Dictionary<string, IResultNode> expectedDict)
                            throw new AssertFailedException($"Expected dictionary, got {node.Value.GetType().Name}");

                        if (actual is IDictionary<string, object?> actualDict)
                        {
                            Assert.AreEqual(expectedDict.Count, actualDict.Count);

                            var values = expectedDict.Zip(actualDict, (a, b) => (Expected: a.Value, Actual: b.Value));

                            foreach (var (Expected, Actual) in values)
                            {
                                AssertNode(Expected, Actual);
                            }
                        }
                        else
                        {
                            var expectedNodes = expectedDict.Values;
                            var actualCollection = ExtractCollection(actual);

                            Assert.AreEqual(expectedNodes.Count, actualCollection.Count());

                            foreach (var (Expected, Actual) in expectedNodes.Zip(actualCollection))
                                AssertNode(Expected, Actual);
                        }
                    }
                    break;
                case "tuple" or "set" or "array":
                    {
                        Assert.IsNotNull(node.Value);
                        Assert.IsNotNull(actual);

                        var expectedNodes = (IResultNode[])node.Value;
                        var actualCollection = ExtractCollection(actual);

                        Assert.AreEqual(expectedNodes.Length, actualCollection.Count());

                        foreach (var (Expected, Actual) in expectedNodes.Zip(actualCollection))
                            AssertNode(Expected, Actual);
                    }
                    break;
                default:
                    {
                        var expected = node.Value;
                        Assert.IsNotInstanceOfType(expected, typeof(IResultNode));

                        if (expected is null)
                            Assert.IsNull(actual);
                        else
                            Assert.IsNotNull(actual);

                        if (expected is null)
                            break;

                        var reducer = _reducers.FirstOrDefault(x => x.CanReduce(expected.GetType(), actual!.GetType()));

                        if (reducer is not null)
                        {
                            var (ReducedExpected, ReducedActual) = reducer.Reduce(expected, actual);

                            Assert.AreEqual(ReducedExpected, ReducedActual);
                        }
                        else if (expected is IEnumerable a && actual is IEnumerable b)
                        {
                            Assert.IsTrue(a.Cast<object>().SequenceEqual(b.Cast<object>()));
                        }
                        else
                        {
                            Assert.AreEqual(expected, actual);
                        }
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

            if(type.Assembly.GetName().Name == "EdgeDB.Runtime")
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
