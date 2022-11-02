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

            public Person[]? Friends { get; set; }
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            try
            {
                await QueryBuilderDemo(client);
                await QueryableCollectionDemo(client);
            }
            catch(Exception x)
            {
            }
        }

        private static async Task QueryBuilderDemo(EdgeDBClient client)
        {
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
            query = QueryBuilder.Select((ctx) => new Person
            {
                Email = ctx.Include<string>(),
                Name = ctx.Include<string>(),
                BestFriend = ctx.IncludeLink(() => new Person
                {
                    Email = ctx.Include<string>(),
                })
            }).Build().Prettify();

            // Adding computed properties in our shape
            // Note: we need to use a new instance of query builder to provide the
            // 'LinkPerson' type as a generic, since its being used for local context
            // in the anon type.
            query = new QueryBuilder<Person>().Select((ctx) => new
            {
                Name = ctx.Include<string>(),
                Email = ctx.Include<string>(),
                HasBestfriend = ctx.Self.BestFriend != null
            }).Build().Prettify();

            // selecting things that are not types
            query = QueryBuilder.Select(() => 
                EdgeQL.Count(
                    QueryBuilder.Select<Person>()
                )
            ).Build().Prettify();

            // selecting 'free objects'
            query = QueryBuilder.Select(ctx => new
            {
                MyString = "This is a string",
                MyNumber = 42,
                SeveralNumbers = new long[] { 1, 2, 3 },
                People = ctx.SubQuery(QueryBuilder.Select<Person>())
            }).Build().Prettify();

            // Backlinks
            query = new QueryBuilder<Person>().Select(ctx => new
            {
                Name = ctx.Include<string>(),
                Email = ctx.Include<string>(),
                Friends = ctx.IncludeLink(() => new Person
                {
                    Name = ctx.Include<string>(),
                    Email = ctx.Include<string>(),
                }),
                // The 'ReferencedFriends' will be equal to '.<best_friends[is MultiLinkPerson] { name, email }'
                // The '[is x]' statement is only inserted when a property selector is used with the generic,
                // you can pass in a string instead of an expression to select out a 'EdgeDBObject' type.
                ReferencedFriends = ctx.BackLink(x => x.Friends, () => new Person
                {
                    Name = ctx.Include<string>(),
                    Email = ctx.Include<string>(),
                })
            }).Build().Prettify();

            // With object variables
            query = QueryBuilder.With(new
            {
                Args = EdgeQL.AsJson(new Person
                {
                    Name = "Example",
                    Email = "example@example.com"
                })
            }).Select(ctx => new
            {
                PassedName = ctx.Variables.Args.Value.Name,
                PassedEmail = ctx.Variables.Args.Value.Email
            }).Build().Pretty;

            // Inserting a new type
            var person = new Person
            {
                Email = "example@example.com",
                Name = "example"
            };
            
            query = QueryBuilder.Insert(person).Build().Prettify();

            // Complex insert with links & dealing with conflicts
            query = (await QueryBuilder
                .Insert(new Person
                {
                    BestFriend = person,
                    Name = "example2",
                    Email = "example2@example.com"
                })
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
                new Person
                {
                    Email = "test1@mail.com",
                    Name = "test1",
                },
                new Person
                {
                    Email = "test2@mail.com",
                    Name = "test2",
                    BestFriend = new Person
                    {
                        Email = "test3@mail.com",
                        Name = "test3",
                    }
                }
            };

            var tquery = (await QueryBuilder.For(data,
                    x => QueryBuilder.Insert(x)
                ).BuildAsync(client));

            // Else statements (upsert demo)
            query = (await QueryBuilder
                .Insert(person)
                .UnlessConflict()
                .Else(q =>
                    q.Update(old => new Person
                    {
                        Name = old!.Name!.ToLower()
                    })
                )
                .BuildAsync(client))
                .Prettify();

            // Updating a type
            query = QueryBuilder
                .Update<Person>(old => new Person
                {
                    Name = "example new name"
                })
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

        private static async Task QueryableCollectionDemo(EdgeDBClient client)
        {
            // Get a 'collection' object, this class wraps the query
            // builder and provides simple CRUD methods.
            var collection = client.GetCollection<Person>();

            // Get or add a value
            var person = await collection.GetOrAddAsync(new Person
            {
                Email = "example@example.com",
                Name = "example"
            });

            // we can change properties locally and then call UpdateAsync to update the type in the database.
            person.Name = "example new name";

            await collection.UpdateAsync(person);

            // or we can provide an update function
            person = await collection.UpdateAsync(person, old => new Person
            {
                Name = "example"
            });

            // we can select types based on a filter
            var people = await collection.WhereAsync(x => EdgeQL.ILike(x.Name, "e%"));

            // we can add or update a type (Broken https://github.com/edgedb/edgedb/issues/4577)
            //var otherPerson = await collection.AddOrUpdateAsync(new Person
            //{
            //    Name = "example2",
            //    Email = "example2@example.com",
            //    BestFriend = person
            //});

            // we can delete types
            var toBeDeleted = await collection.GetOrAddAsync(new Person
            {
                Email = "example3@example.com",
                Name = "example3"
            });

            // the result of this delete functions is whether or not it was deleted.
            var success = await collection.DeleteAsync(toBeDeleted);

            // we can also delete types based on a filter
            var count = await collection.DeleteWhereAsync(x => EdgeQL.ILike(x.Name, "e%"));
        }
    }
}
