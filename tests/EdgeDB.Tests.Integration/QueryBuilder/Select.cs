using EdgeDB.Tests.Integration.QueryBuilder.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdgeDB.Tests.Integration.QueryBuilder;

[TestClass]
public class Select
{
    [TestMethod]
    public void TestSelectType()
    {
        var query = EdgeDB.QueryBuilder.Select<Person>().Compile();

        new TestParser(query)
            .Expect("select tests::Person { name, email, age }")
            .EOF();
    }

    [TestMethod]
    public void TestSelectShapes()
    {
        var query = EdgeDB.QueryBuilder.Select<Person>(shape => shape.Explicitly(p => new {p.Name})).Compile();

        new TestParser(query)
            .Expect("select tests::Person { name }")
            .EOF();

        query = EdgeDB.QueryBuilder.Select<Person>(shape =>
            shape.IncludeMultiLink(x => x.Friends)
        ).Compile();

        new TestParser(query)
            .Expect("select tests::Person { name, email, age, friends: { name, email, age } }")
            .EOF();

        query = EdgeDB.QueryBuilder.Select<Person>(shape =>
            shape
                .IncludeMultiLink(x => x.Friends)
                .Include(x => x.BestFriend)
        ).Compile();

        new TestParser(query)
            .Expect(
                "select tests::Person { name, email, age, friends: { name, email, age }, best_friend: { name, email, age } }")
            .EOF();

        query = EdgeDB.QueryBuilder.Select<Person>(shape =>
            shape.Computeds(p => new {FriendCount = EdgeQL.Count(p.Friends)})
        ).Compile();

        new TestParser(query)
            .Expect("select tests::Person { name, email, age, FriendCount := std::count(.friends) }")
            .EOF();
    }
}
