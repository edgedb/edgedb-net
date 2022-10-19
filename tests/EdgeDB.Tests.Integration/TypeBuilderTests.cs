using EdgeDB.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EdgeDB.Tests.Integration
{
    public class TypeBuilderTests : IClassFixture<ClientFixture>
    {
        private readonly EdgeDBClient _client;
        private readonly Func<CancellationToken> _getToken;

        public TypeBuilderTests(ClientFixture fixture)
        {
            _client = fixture.EdgeDB;
            _getToken = () => fixture.GetTimeoutToken();
        }

        private async Task EnsurePersonIsAddedAsync()
        {
            await _client.ExecuteAsync("insert Person { name := \"A random name\", email := \"test@example.com\"} unless conflict on .email", token: _getToken());
        }

        [Fact]
        public async Task BasicTypeDeserialize()
        {
            // insert person if they dont exist
            await EnsurePersonIsAddedAsync();

            var person = await _client.QueryRequiredSingleAsync<PersonClass>("select Person { name, email } filter .email = \"test@example.com\"", token: _getToken());

            Assert.Equal("A random name", person.Name);
            Assert.Equal("test@example.com", person.Email);
        }

        [Fact]
        public async Task LocalAbstractTypeDeserialize()
        {
            await EnsurePersonIsAddedAsync();

            var person = await _client.QueryRequiredSingleAsync<PersonImpl>("select Person { name, email } filter .email = \"test@example.com\"", token: _getToken());

            Assert.Equal("A random name", person.Name);
            Assert.Equal("test@example.com", person.Email);
        }

        [Fact]
        public async Task SchemaAbstractTypeDeserialize()
        {
            // insert some types
            await _client.ExecuteAsync("insert Thing { name := \"Thing1\", description := \"This is thing one!\" } unless conflict on .name", token: _getToken());
            await _client.ExecuteAsync("insert OtherThing { name := \"Thing2\", attribute := \"<readonly>\" } unless conflict on .name", token: _getToken());

            var abstractSelect = await _client.QueryAsync<AbstractThing>("select AbstractThing { name, [is Thing].description, [is OtherThing].attribute }", token: _getToken());

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

        [Fact]
        public async Task TestConstructorDeserializer()
        {
            await EnsurePersonIsAddedAsync();
            
            var person = await _client.QueryRequiredSingleAsync<PersonConstructorBuilder>("select Person { name, email } limit 1", token: _getToken());

            Assert.NotNull(person);
            Assert.IsType<PersonConstructorBuilder>(person);
            Assert.True(person.CustomDeserializerCalled);
        }

        [Fact]
        public async Task TestCustomMethodDeserializer()
        {
            await EnsurePersonIsAddedAsync();
            
            var person = await _client.QueryRequiredSingleAsync<PersonMethodBuilder>("select Person { name, email } limit 1", token: _getToken());

            Assert.NotNull(person);
            Assert.IsType<PersonMethodBuilder>(person);
            Assert.True(person.CustomDeserializerCalled);
        }

        [Fact]
        public async Task TestRecord()
        {
            await EnsurePersonIsAddedAsync();
            
            var person = await _client.QueryRequiredSingleAsync<PersonRecord>("select Person { name, email } limit 1", token: _getToken());

            Assert.NotNull(person);
            Assert.IsType<PersonRecord>(person);
        }
        
        [Fact]
        public async Task TestAddedFactoryBuilder()
        {
            await EnsurePersonIsAddedAsync();
            
            bool customDeserializerCalled = false;

            TypeDeserializerFactory customBuilder = (ref ObjectEnumerator enumerator) =>
            {
                var person = new PersonClass();

                while(enumerator.Next(out var name, out var value))
                {
                    switch (name)
                    {
                        case "name":
                            person.Name = (string?)value;
                            break;
                        case "email":
                            person.Email = (string?)value;
                            break;
                    }
                }

                customDeserializerCalled = true;

                return person;
            };

            TypeBuilder.AddOrUpdateTypeFactory<PersonClass>(customBuilder);

            Assert.True(TypeBuilder.TypeInfo.ContainsKey(typeof(PersonClass)));

            var typeInfo = TypeBuilder.TypeInfo[typeof(PersonClass)];

            Assert.Equal(customBuilder, typeInfo.Factory);

            var person = await _client.QueryAsync<PersonClass>("select Person { name, email }", token: _getToken());

            Assert.NotNull(person);

            Assert.True(customDeserializerCalled);
        }

        public class PersonClass
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
        }

        public class PersonMethodBuilder
        {
            public string? Name { get; set; }
            public string? Email { get; set; }

            public bool CustomDeserializerCalled { get; private set; }

            [EdgeDBDeserializer]
            public void Build(IDictionary<string, object?> data)
            {
                Name = (string?)data["name"];
                Email = (string?)data["email"];

                CustomDeserializerCalled = true;
            }
        }

        public class PersonConstructorBuilder
        {
            public string? Name { get; set; }
            public string? Email { get; set; }

            public bool CustomDeserializerCalled { get; }

            [EdgeDBDeserializer]
            public PersonConstructorBuilder(IDictionary<string, object?> data)
            {
                Name = (string?)data["name"];
                Email = (string?)data["email"];

                CustomDeserializerCalled = true;
            }
        }

        public record PersonRecord(string Name, string Email);
        
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
