using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EdgeDB.Tests.Integration
{
    public class ClientTestsCustomTypes : IClassFixture<ClientFixture>
    {
        private readonly EdgeDBClient _edgedb;
        private readonly ITestOutputHelper _output;

        public ClientTestsCustomTypes(ClientFixture clientFixture, ITestOutputHelper output)
        {
            _edgedb = clientFixture.EdgeDB;
            _output = output;
        }

        [Fact]
        public async Task TypeDeserialize()
        {
            // insert person if they dont exist
            await _edgedb.ExecuteAsync("insert Person { name := \"A random name\", email := \"test@example.com\"} unless conflict on .email");

            var person = await _edgedb.QueryRequiredSingleAsync<Person>("select Person { name, email } filter .email = \"test@example.com\"");

            Assert.Equal("A random name", person.Name);
            Assert.Equal("test@example.com", person.Email);
        }

        [Fact]
        public async Task LocalAbstractTypeDeserialize()
        {
            // insert person if they dont exist
            await _edgedb.ExecuteAsync("insert Person { name := \"A random name\", email := \"test@example.com\"} unless conflict on .email");

            var person = await _edgedb.QueryRequiredSingleAsync<PersonImpl>("select Person { name, email } filter .email = \"test@example.com\"");

            Assert.Equal("A random name", person.Name);
            Assert.Equal("test@example.com", person.Email);
        }

        [Fact]
        public async Task SchemaAbstractTypeDeserialize()
        {
            // insert some types
            await _edgedb.ExecuteAsync("insert Thing { name := \"Thing1\", description := \"This is thing one!\" } unless conflict on .name");
            await _edgedb.ExecuteAsync("insert OtherThing { name := \"Thing2\", attribute := \"<readonly>\" } unless conflict on .name");

            var abstractSelect = await _edgedb.QueryAsync<AbstractThing>("select AbstractThing { name, [is Thing].description, [is OtherThing].attribute }");

            foreach (var result in abstractSelect)
            {
                if (result is Thing thing)
                {
                    Assert.Equal("Thing1", thing.Name);
                    Assert.Equal("This is thing one!", thing.Description);
                }
                else if (result is OtherThing otherThing)
                {
                    Assert.Equal("Thing2", otherThing.Name);
                    Assert.Equal("<readonly>", otherThing.Attribute);
                }
                else
                    throw new Exception("Unexpected type");
            }
        }

        public class Person 
        {
            public string? Name { get; init; }

            public string? Email { get; init; }
        }

        public class PersonImpl : AbstractPerson
        {
            public string? Name { get; init; }
        }

        public abstract class AbstractPerson
        {
            public string? Email { get; init; }
        }

        public abstract class AbstractThing
        {
            public string? Name { get; init; }
        }

        public class Thing : AbstractThing
        {
            public string? Description { get; init; }
        }

        public class OtherThing : AbstractThing
        {
            public string? Attribute { get; init; }
        }
    }
}
