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

        public class LinkPerson
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public LinkPerson? BestFriend { get; set; }
        }

        public class MultiLinkPerson
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public MultiLinkPerson[]? BestFriends { get; set; }
        }

        public class ArrayPerson
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
        
            public IEnumerable<string>? Roles { get; set; }
        }

        public class PropertyConstraintPerson
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
        }

        public class ConstraintPerson
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            await QueryBuilderDemo(client);
            await QueryableCollectionDemo(client);
        }

        private static async Task QueryBuilderDemo(EdgeDBClient client)
        {
            // Selecting a type with autogen shape
            var query = QueryBuilder.Select<LinkPerson>().Build().Prettify();

            // Adding a filter, orderby, offset, and limit
            query = QueryBuilder
                .Select<LinkPerson>()
                .Filter(x => EdgeQL.ILike(x.Name, "e%"))
                .OrderByDesending(x => x.Name)
                .Offset(2)
                .Limit(10)
                .Build()
                .Prettify();

            // Specifying a shape
            query = QueryBuilder.Select((ctx) => new LinkPerson
            {
                Email = ctx.Include<string>(),
                Name = ctx.Include<string>(),
                BestFriend = ctx.IncludeLink(() => new LinkPerson
                {
                    Email = ctx.Include<string>(),
                })
            }).Build().Prettify();

            // Adding computed properties in our shape
            // Note: we need to use a new instance of query builder to provide the
            // 'LinkPerson' type as a generic, since its being used for local context
            // in the anon type.
            query = new QueryBuilder<LinkPerson>().Select((ctx) => new
            {
                Name = ctx.Include<string>(),
                Email = ctx.Include<string>(),
                HasBestfriend = ctx.Local("BestFriend") != null
            }).Build().Prettify();

            // selecting things that are not types
            query = QueryBuilder.Select(() => 
                EdgeQL.Count(
                    QueryBuilder.Select<LinkPerson>()
                )
            ).Build().Prettify();

            // selecting 'free objects'
            query = QueryBuilder.Select(ctx => new
            {
                MyString = "This is a string",
                MyNumber = 42,
                SeveralNumbers = new long[] { 1, 2, 3 },
                People = ctx.SubQuery(QueryBuilder.Select<MultiLinkPerson>())
            }).Build().Prettify();

            // Backlinks
            query = new QueryBuilder<MultiLinkPerson>().Select(ctx => new
            {
                Name = ctx.Include<string>(),
                Email = ctx.Include<string>(),
                BestFriends = ctx.IncludeLink(() => new MultiLinkPerson
                {
                    Name = ctx.Include<string>(),
                    Email = ctx.Include<string>(),
                }),
                // The 'ReferencedFriends' will be equal to '.<best_friends[is MultiLinkPerson] { name, email }'
                // The '[is x]' statement is only inserted when a property selector is used with the generic,
                // you can pass in a string instead of an expression to select out a 'EdgeDBObject' type.
                ReferencedFriends = ctx.BackLink(x => x.BestFriends, () => new MultiLinkPerson
                {
                    Name = ctx.Include<string>(),
                    Email = ctx.Include<string>(),
                })
            }).Build().Prettify();

            // With object variables
            query = QueryBuilder.With(new
            {
                Args = EdgeQL.AsJson(new ConstraintPerson
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
            var person = new LinkPerson
            {
                Email = "example@example.com",
                Name = "example"
            };
            
            query = QueryBuilder.Insert(person).Build().Prettify();

            // Complex insert with links & dealing with conflicts
            query = (await QueryBuilder
                .Insert(new LinkPerson
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
            var data = new LinkPerson[]
            {
                new LinkPerson
                {
                    Email = "test1@mail.com",
                    Name = "test1",
                },
                new LinkPerson
                {
                    Email = "test2@mail.com",
                    Name = "test2",
                    BestFriend = new LinkPerson
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
                    q.Update(old => new LinkPerson
                    {
                        Name = old!.Name!.ToLower()
                    })
                )
                .BuildAsync(client))
                .Prettify();

            // Updating a type
            query = QueryBuilder
                .Update<LinkPerson>(old => new LinkPerson
                {
                    Name = "example new name"
                })
                .Filter(x => x.Email == "example@example.com")
                .Build()
                .Prettify();

            // Deleting types
            query = QueryBuilder
                .Delete<LinkPerson>()
                .Filter(x => EdgeQL.ILike(x.Name, "e%"))
                .Build()
                .Prettify();
        }

        private static async Task QueryableCollectionDemo(EdgeDBClient client)
        {
            // Get a 'collection' object, this class wraps the query
            // builder and provides simple CRUD methods.
            var collection = client.GetCollection<LinkPerson>();

            // Get or add a value
            var person = await collection.GetOrAddAsync(new LinkPerson
            {
                Email = "example@example.com",
                Name = "example"
            });

            // we can change properties locally and then call UpdateAsync to update the type in the database.
            person.Name = "example new name";

            await collection.UpdateAsync(person);

            // or we can provide an update function
            person = await collection.UpdateAsync(person, old => new LinkPerson
            {
                Name = "example"
            });

            // we can select types based on a filter
            var people = await collection.WhereAsync(x => EdgeQL.ILike(x.Name, "e%"));

            // we can add or update a type
            var otherPerson = await collection.AddOrUpdateAsync(new LinkPerson
            {
                Name = "example2",
                Email = "example2@example.com",
                BestFriend = person
            });

            // we can delete types
            var toBeDeleted = await collection.GetOrAddAsync(new LinkPerson
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
