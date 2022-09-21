using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EdgeDB.Tests.Integration
{
    public class DeserializationTests : IClassFixture<ClientFixture>
    {
        private readonly EdgeDBClient _edgedb;
        private readonly ITestOutputHelper _output;

        public DeserializationTests(ClientFixture clientFixture, ITestOutputHelper output)
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

        

        public class Person 
        {
            public string? Name { get; init; }

            public string? Email { get; init; }
        }

        
    }
}
