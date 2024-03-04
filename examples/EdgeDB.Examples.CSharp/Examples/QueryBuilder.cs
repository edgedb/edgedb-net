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
            QueryBuilder
                .SelectExpression(x => EdgeQL.Count(QueryBuilder.Select<Person>()));

            // Selecting a type with autogen shape
            var query = QueryBuilder.Select<Person>().Build().Prettify();

            // Adding a filter, orderby, offset, and limit
            query = QueryBuilder
                .Select<Person>()
                .Filter(x => EdgeQL.ILike(x.Name, "e%"))
                .OrderByDesending(x => x.Name)
                .Offset(2)
                .Limit(10)
                .Build()
                .Prettify();

            // Specifying a shape
            query = QueryBuilder.Select<Person>(shape =>
                shape
                    .Explicitly(p => new { p.Name, p.Email, p.BestFriend })
            ).Build().Prettify();

            // selecting things that are not types
            query = QueryBuilder.SelectExpression(() =>
                EdgeQL.Count(QueryBuilder.Select<Person>())
            ).Build().Prettify();

            // selecting 'free objects'
            query = QueryBuilder.SelectExpression(ctx => new
            {
                MyString = "This is a string",
                MyNumber = 42,
                SeveralNumbers = new long[] { 1, 2, 3 },
                People = ctx.SubQuery(QueryBuilder.Select<Person>())
            }).Build().Prettify();

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
            ).Build().Prettify();

            // With object variables
            query = QueryBuilder
                .With(new { Args = EdgeQL.AsJson(new { Name = "Example", Email = "example@example.com" }) })
                .SelectExpression(ctx => new
                {
                    PassedName = ctx.Variables.Args.Value.Name, PassedEmail = ctx.Variables.Args.Value.Email
                }).Build().Prettify();

            // Inserting a new type
            var person = new Person { Email = "example@example.com", Name = "example" };

            query = QueryBuilder.Insert(person).Build().Prettify();

            // Complex insert with links & dealing with conflicts
            query = (await QueryBuilder
                    .Insert(new Person { BestFriend = person, Name = "example2", Email = "example2@example.com" })
                    .UnlessConflict()
                    .ElseReturn()
                    .BuildAsync(client))
                .Prettify();

            // Manual conflicts
            query = QueryBuilder
                .Insert(person)
                .UnlessConflictOn(x => x.Email)
                .ElseReturn()
                .Build()
                .Prettify();

            // Autogenerating unless conflict with introspection
            query = (await QueryBuilder
                    .Insert(person)
                    .UnlessConflict()
                    .ElseReturn()
                    .BuildAsync(client))
                .Prettify();

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
            ).BuildAsync(client);

            // Else statements (upsert demo)
            query = (await QueryBuilder
                    .Insert(person)
                    .UnlessConflict()
                    .Else(q =>
                        q.Update(old => new Person { Name = old!.Name!.ToLower() })
                    )
                    .BuildAsync(client))
                .Prettify();

            // Updating a type
            query = QueryBuilder
                .Update<Person>(old => new Person { Name = "example new name" })
                .Filter(x => x.Email == "example@example.com")
                .Build()
                .Prettify();

            // Deleting types
            query = QueryBuilder
                .Delete<Person>()
                .Filter(x => EdgeQL.ILike(x.Name, "e%"))
                .Build()
                .Prettify();
        }
    }
}
