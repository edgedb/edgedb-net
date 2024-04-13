using BenchmarkDotNet.Running;
using EdgeDB;
using EdgeDB.Tests.Benchmarks;
using System.Text.RegularExpressions;

// | Method        | Mean     | Error    | StdDev   | Gen0    | Gen1   | Allocated |
//     |-------------- |---------:|---------:|---------:|--------:|-------:|----------:|
//     | GroupAdvanced | 794.4 us | 12.78 us | 13.68 us | 20.5078 | 6.8359 | 125.78 KB |

// QueryBuilder
//     .With(ctx => new
//     {
//         People = ctx.SubQuerySingle(QueryBuilder.Select<Person>()),
//         Groups = ctx.SubQuerySingle(
//             QueryBuilder
//                 .Group(ctx => ctx.Global<Person>("People"))
//                 .Using(person => new
//                 {
//                     Vowel = Regex.IsMatch(person.Name!, "(?i)^[aeiou]"),
//                     NameLength = person.Name!.Length
//                 })
//                 .By(ctx => EdgeQL.Cube(new { ctx.Using.Vowel, ctx.Using.NameLength }))
//         )
//     })
//     .SelectExpression(ctx => ctx.Variables.Groups, shape => shape
//         .Explicitly((ctx, group) => new
//         {
//             group.Key,
//             group.Grouping,
//             Count = EdgeQL.Count(group.Elements),
//             MeanNameLength = EdgeQL.Mean(ctx.Aggregate(group.Elements, element => (long)element.Name!.Length))//NameLength = 1//EdgeQL.Len(ctx.Ref(group.Elements).Name!)
//         })
//     )
//     .OrderBy(x => EdgeQL.ArrayAgg(x.Grouping))
//     .Compile();

BenchmarkRunner.Run<QueryBuilderBenchmarks>();

//await Task.Delay(-1);

// while(!GC.TryStartNoGCRegion((long)1.28e+8)) {}
//
// for (int i = 0; i != 694201337; i++)
// {
//     var x = QueryBuilder
//         .Select<Person>()
//         .Compile();
//
//     var y = QueryBuilder
//         .Select<Person>()
//         .Filter(x => x.Name.Length > 4 && x.Name.StartsWith('z'))
//         .Limit(4)
//         .Offset(4)
//         .Compile();
// }
//
//
// await Task.Delay(-1);
