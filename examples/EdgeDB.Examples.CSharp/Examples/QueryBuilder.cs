using EdgeDB.DataTypes;
using EdgeDB.QueryNodes;
using EdgeDB.Schema;
using EdgeDB.Schema.DataTypes;
using EdgeDB.Translators.Methods;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class QueryBuilderExample : IExample
    {
        public ILogger? Logger { get; set; }

        public class Person
        {
            public string? Name { get; set; }
            public string? Email { get; set; }

            public Person? BestFriend { get; set; }

            public List<Person>? Friends { get; set; }
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            try
            {
                await QueryBuilderDemo(client);
            }
            catch (Exception x)
            {
                throw;
            }
        }

        private static async Task QueryBuilderDemo(EdgeDBClient client)
        {
            // Selecting a type with autogen shape
            var query = QueryBuilder.Select<Person>().Compile(true);

            // Adding a filter, orderby, offset, and limit
            query = QueryBuilder
                .Select<Person>()
                .Filter(x => EdgeQL.ILike(x.Name, "e%"))
                .OrderByDesending(x => x.Name)
                .Offset(2)
                .Limit(10)
                .Compile(true);

            // Specifying a shape
            query = QueryBuilder.Select<Person>(shape =>
                shape
                    .Explicitly(p => new { p.Name, p.Email, p.BestFriend })
            ).Compile(true);

            // selecting things that are not types
            query = QueryBuilder.SelectExpression(() =>
                EdgeQL.Count(QueryBuilder.Select<Person>())
            ).Compile(true);

            // selecting 'free objects'
            query = QueryBuilder.SelectExpression(ctx => new
            {
                MyString = "This is a string",
                MyNumber = 42,
                SeveralNumbers = new long[] { 1, 2, 3 },
                People = ctx.SubQuery(QueryBuilder.Select<Person>())
            }).Compile(true);

            // Backlinks
            query = QueryBuilder.Select<Person>(shape => shape
                .IncludeMultiLink(x => x.Friends)
                .Computeds((ctx, _) => new
                {
                    // The 'ReferencedFriends' will be equal to '.<best_friends[is MultiLinkPerson] { name, email }'
                    // The '[is x]' statement is only inserted when a property selector is used with the generic,
                    // you can pass in a string instead of an expression to select out a 'EdgeDBObject' type.
                    ReferencedFriends = ctx.BackLink<Person>(x => x.Friends)
                })
            ).Compile(true);

            // With object variables
            query = QueryBuilder
                .With(new { Args = EdgeQL.AsJson(new { Name = "Example", Email = "example@example.com" }) })
                .SelectExpression(ctx => new
                {
                    PassedName = ctx.Variables.Args.Value.Name, PassedEmail = ctx.Variables.Args.Value.Email
                }).Compile(true);

            // Inserting a new type
            var person = new Person { Email = "example@example.com", Name = "example" };

            query = QueryBuilder.Insert(person).Compile(true);

            // Complex insert with links & dealing with conflicts
            query = await QueryBuilder
                .Insert(new Person {BestFriend = person, Name = "example2", Email = "example2@example.com"})
                .UnlessConflict()
                .ElseReturn()
                .CompileAsync(client, true);

            // Manual conflicts
            query = QueryBuilder
                .Insert(person)
                .UnlessConflictOn(x => x.Email)
                .ElseReturn()
                .Compile(true);

            // Autogenerating unless conflict with introspection
            query = await QueryBuilder
                .Insert(person)
                .UnlessConflict()
                .ElseReturn()
                .CompileAsync(client, true);

            // Bulk inserts
            var data = new Person[]
            {
                new Person { Email = "test1@mail.com", Name = "test1", },
                new Person
                {
                    Email = "test2@mail.com",
                    Name = "test2",
                    BestFriend = new Person { Email = "test3@mail.com", Name = "test3", }
                }
            };

            query = await QueryBuilder.For(data,
                x => QueryBuilder.Insert(x)
            ).CompileAsync(client, true);

            // Else statements (upsert demo)
            query = await QueryBuilder
                .Insert(person)
                .UnlessConflict()
                .Else(q =>
                    q.Update(old => new Person {Name = old!.Name!.ToLower()})
                )
                .CompileAsync(client, true);

            // Updating a type
            query = QueryBuilder
                .Update<Person>(old => new Person { Name = "example new name" })
                .Filter(x => x.Email == "example@example.com")
                .Compile(true);

            // Deleting types
            query = QueryBuilder
                .Delete<Person>()
                .Filter(x => EdgeQL.ILike(x.Name, "e%"))
                .Compile(true);

            // grouping
            query = QueryBuilder
                .Group<Person>()
                .By(x => x.Name)
                .Compile(true);

            // grouping by expressions
            query = QueryBuilder
                .Group<Person>()
                .Using(person => new {StartsWithVowel = Regex.IsMatch(person.Name!, "(?i)^[aeiou]")})
                .By(ctx => ctx.Using.StartsWithVowel)
                .Compile(true);

            // grouping by scalars
            query = QueryBuilder
                .With(ctx => new {People = ctx.SubQuerySingle(QueryBuilder.Select<Person>())})
                .Group(ctx => ctx.Variables.People.Name)
                .Using(name => new {StartsWithVowel = Regex.IsMatch(name!, "(?i)^[aeiou]")})
                .By(group => group.Using.StartsWithVowel)
                .Compile(true);

            // grouping result as select
            query = QueryBuilder
                .With(ctx => new
                {
                    People = ctx.SubQuerySingle(QueryBuilder.Select<Person>()),
                    Groups = ctx.SubQuerySingle(
                        QueryBuilder
                            .Group(ctx => ctx.Global<Person>("People"))
                            .Using(person => new {Vowel = Regex.IsMatch(person.Name!, "(?i)^[aeiou]")})
                            .By(ctx => ctx.Using.Vowel)
                    )
                })
                .SelectExpression(ctx => ctx.Variables.Groups, shape => shape
                    .Computeds((ctx, group) => new
                    {
                        StartsWithVowel = group.Key,
                        Count = EdgeQL.Count(group.Elements),
                        NameLength = 1//EdgeQL.Len(ctx.Ref(group.Elements).Name!)
                    })
                )
                .Compile(true);

            // grouping by more than 1 parameter
            query = QueryBuilder
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
                            .By(ctx => Tuple.Create(ctx.Using.Vowel, ctx.Using.NameLength))
                    )
                })
                .SelectExpression(ctx => ctx.Variables.Groups, shape => shape
                    .Explicitly((ctx, group) => new
                    {
                        group.Key,
                        Count = EdgeQL.Count(group.Elements),
                        MeanNameLength = EdgeQL.Mean(ctx.Aggregate(group.Elements, element => (long)element.Name!.Length))
                    })
                )
                .Compile(true);

            // grouping by grouping sets
            query = QueryBuilder
                .With(ctx => new
                {
                    People = ctx.SubQuerySingle(QueryBuilder.Select<Person>()),
                    Groups = ctx.SubQuerySingle(
                        QueryBuilder
                            .Group(ctx => ctx.Local<Person>("People"))
                            .Using(person => new
                            {
                                Vowel = Regex.IsMatch(person.Name!, "(?i)^[aeiou]"),
                                NameLength = person.Name!.Length
                            })
                            .By(ctx => EdgeQL.Cube(new { ctx.Using.Vowel, ctx.Using.NameLength }))
                    )
                })
                .SelectExpression(ctx => ctx.Variables.Groups, shape => shape
                    .Explicitly((ctx, group) => new
                    {
                        group.Key,
                        group.Grouping,
                        Count = EdgeQL.Count(group.Elements),
                        MeanNameLength = EdgeQL.Mean(ctx.Aggregate(group.Elements, element => (long)element.Name!.Length))//NameLength = 1//EdgeQL.Len(ctx.Ref(group.Elements).Name!)
                    })
                )
                .OrderBy(x => EdgeQL.ArrayAgg(x.Grouping))
                .Compile(true);
        }
    }
}
