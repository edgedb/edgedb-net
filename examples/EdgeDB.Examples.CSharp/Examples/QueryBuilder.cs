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
                var tests = await QueryBuilder
                    .Select<ObjectType>(shape =>
                    {
                        shape.IncludeMultiLink(x => x.Constraints);
                        shape.IncludeMultiLink(x => x.Properties, shape =>
                            shape.Computeds((ctx, prop) => new
                            {
                                Cardinality = (string)ctx.UnsafeLocal<object>("cardinality") == "One"
                                    ? ctx.UnsafeLocal<bool>("required")
                                        ? Cardinality.One
                                        : Cardinality.AtMostOne
                                    : ctx.UnsafeLocal<bool>("required")
                                        ? Cardinality.AtLeastOne
                                        : Cardinality.Many,
                                TargetId = ctx.UnsafeLocal<Guid>("target.id"),
                                IsLink = ctx.Raw<object?>("[IS schema::Link]") != null,
                                IsExclusive =
                                    ctx.Raw<bool>("exists (select .constraints filter .name = 'std::exclusive')"),
                                IsComputed = EdgeQL.Len(ctx.UnsafeLocal<object[]>("computed_fields")) != 0,
                                IsReadonly = ctx.UnsafeLocal<bool>("readonly"),
                                HasDefault =
                                    ctx.Raw<bool>(
                                        "EXISTS .default or (\"std::sequence\" in .target[IS schema::ScalarType].ancestors.name)")
                            })
                        );
                    })
                    .Filter((x, ctx) => !ctx.UnsafeLocal<bool>("builtin"))
                    .CompileAsync(client, true);

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
            var query = QueryBuilder.Select<Person>().Compile().Prettify();

            // Adding a filter, orderby, offset, and limit
            var queryTest = QueryBuilder
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
            ).Compile().Prettify();

            // selecting things that are not types
            query = QueryBuilder.SelectExpression(() =>
                EdgeQL.Count(QueryBuilder.Select<Person>())
            ).Compile().Prettify();

            // selecting 'free objects'
            query = QueryBuilder.SelectExpression(ctx => new
            {
                MyString = "This is a string",
                MyNumber = 42,
                SeveralNumbers = new long[] { 1, 2, 3 },
                People = ctx.SubQuery(QueryBuilder.Select<Person>())
            }).Compile().Prettify();

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
            ).Compile().Prettify();

            // With object variables
            query = QueryBuilder
                .With(new { Args = EdgeQL.AsJson(new { Name = "Example", Email = "example@example.com" }) })
                .SelectExpression(ctx => new
                {
                    PassedName = ctx.Variables.Args.Value.Name, PassedEmail = ctx.Variables.Args.Value.Email
                }).Compile().Prettify();

            // Inserting a new type
            var person = new Person { Email = "example@example.com", Name = "example" };

            query = QueryBuilder.Insert(person).Compile().Prettify();

            // Complex insert with links & dealing with conflicts
            query = (await QueryBuilder
                    .Insert(new Person { BestFriend = person, Name = "example2", Email = "example2@example.com" })
                    .UnlessConflict()
                    .ElseReturn()
                    .CompileAsync(client))
                .Prettify();

            // Manual conflicts
            query = QueryBuilder
                .Insert(person)
                .UnlessConflictOn(x => x.Email)
                .ElseReturn()
                .Compile()
                .Prettify();

            // Autogenerating unless conflict with introspection
            queryTest = (await QueryBuilder
                    .Insert(person)
                    .UnlessConflict()
                    .ElseReturn()
                    .CompileAsync(client, true));

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

            var tquery = await QueryBuilder.For(data,
                x => QueryBuilder.Insert(x)
            ).CompileAsync(client);

            // Else statements (upsert demo)
            query = (await QueryBuilder
                    .Insert(person)
                    .UnlessConflict()
                    .Else(q =>
                        q.Update(old => new Person { Name = old!.Name!.ToLower() })
                    )
                    .CompileAsync(client))
                .Prettify();

            // Updating a type
            query = QueryBuilder
                .Update<Person>(old => new Person { Name = "example new name" })
                .Filter(x => x.Email == "example@example.com")
                .Compile()
                .Prettify();

            // Deleting types
            query = QueryBuilder
                .Delete<Person>()
                .Filter(x => EdgeQL.ILike(x.Name, "e%"))
                .Compile()
                .Prettify();
        }
    }
}
