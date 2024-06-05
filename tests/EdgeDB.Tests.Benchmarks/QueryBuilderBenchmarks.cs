using BenchmarkDotNet.Attributes;
using EdgeDB.Utils;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Text.RegularExpressions;

namespace EdgeDB.Tests.Benchmarks;

[EdgeDBType]
public class Person
{
    public string Name { get; set; }
    public string Email { get; set; }
    public Person BestFriend { get; set; }
    public List<Person> Friends { get; set; }
}

[MemoryDiagnoser]
public class QueryBuilderBenchmarks
{
    public string Prop { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var buff = new byte[16];
        Random.Shared.NextBytes(buff);
        Prop = HexConverter.ToHex(buff);
        TypeBuilder.SchemaNamingStrategy = INamingStrategy.CamelCaseNamingStrategy;
    }

    // | Method        | Mean     | Error    | StdDev   | Gen0    | Gen1   | Allocated |
    // |-------------- |---------:|---------:|---------:|--------:|-------:|----------:|
    //| GroupAdvanced | 794.4 us | 12.78 us | 13.68 us | 20.5078 | 6.8359 | 125.78 KB |
    [Benchmark]
    public CompiledQuery GroupAdvanced()
    {
        return QueryBuilder
            .With(ctx => new
            {
                People = ctx.SubQuerySingle(QueryBuilder.Select<Person>()),
                Groups = ctx.SubQuerySingle(
                    QueryBuilder
                        .Group(ctx => ctx.Global<Person>("People"))
                        .Using(person => new
                        {
                            Vowel = Regex.IsMatch(person.Name!, "(?i)^[aeiou]"),
                            NameLength = person.Name!.Length
                        })
                        .By(ctx => EdgeQL.Cube(new { ctx.Using.Vowel, ctx.Using.NameLength }))
                )
            })
            .SelectExpression(ctx => ctx.Variables.Groups, shape => shape
                .Explicitly((group, ctx) => new
                {
                    group.Key,
                    group.Grouping,
                    Count = EdgeQL.Count(group.Elements),
                    MeanNameLength = EdgeQL.Mean(ctx.Aggregate(group.Elements, element => (long)element.Name!.Length))//NameLength = 1//EdgeQL.Len(ctx.Ref(group.Elements).Name!)
                })
            )
            .OrderBy(x => EdgeQL.ArrayAgg(x.Grouping))
            .Compile();
    }

    //[Benchmark]
    public CompiledQuery SelectBasic()
    {
        return QueryBuilder
            .Select<Person>()
            .Compile();
    }

    //[Benchmark]
    public CompiledQuery SelectAdvanced()
    {
        return QueryBuilder
            .Select<Person>()
            .Filter(x => x.Name.Length > 4 && x.Name.StartsWith('z'))
            .Limit(4)
            .Offset(4)
            .Compile();
    }
}

public readonly struct OldValue(Action<StringBuilder> func)
{
    public void Run(StringBuilder builder)
    {
        func(builder);
    }
}

public readonly struct NewValue(Delegate func, object?[] args)
{
    public void Run(StringBuilder b)
    {
        var arr = new object?[args.Length + 1];
        arr[0] = b;
        args.CopyTo(arr[1..].AsSpan());

        func.Method.Invoke(func.Target, arr);
    }

    public static NewValue Create(Action<StringBuilder> func)
        => new(func, Array.Empty<object?>());

    public static NewValue Create<T>(Action<StringBuilder, T> func, T a)
        => new(func, [a]);

    public static NewValue Create<T, U>(Action<StringBuilder, T, U> func, T a, U b)
        => new(func, [a, b]);
}
