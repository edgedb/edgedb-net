using EdgeDB.State;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EdgeDB.Tests.Integration
{
    public class DDLTests : IClassFixture<ClientFixture>
    {
        private readonly EdgeDBClient _client;
        private readonly EdgeDBClient _ddlClient;
        private readonly Func<CancellationToken> _getToken;


        public DDLTests(ClientFixture fixture)
        {
            _client = fixture.EdgeDB;
            _ddlClient = _client.WithConfig(x => x.DDLPolicy = DDLPolicy.AlwaysAllow);
            _getToken = () => fixture.GetTimeoutToken();
        }

        [Fact]
        public async Task TestDDLInvalidCapabilitiesAndSessionConfig()
        {
            await Assert.ThrowsAsync<EdgeDBErrorException>(async () =>
            {
                await _client.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }", token: _getToken());
            });
        }

        [Fact]
        public async Task TestDDLInvalidCapabilitiesAndValidConfig()
        {
            await Assert.ThrowsAsync<EdgeDBErrorException>(async () =>
            {
                await _ddlClient.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }", token: _getToken());
            });
        }

        [Fact]
        public async Task TestDDLValidConfigAndCapabilities()
        {
            try
            {
                await _ddlClient.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }", capabilities: Capabilities.All, token: _getToken());

                var testResult = await _ddlClient.QueryRequiredSingleAsync<TestType>("with t := (insert TestType { name := 'test' }) select t { name } limit 1", token: _getToken());

                Assert.NotNull(testResult);
                Assert.Equal("test", testResult.Name);
            }
            finally
            {
                // try to drop the type, don't throw if this fails
                try
                {
                    await _ddlClient.ExecuteAsync("DROP TYPE TestType", capabilities: Capabilities.All, token: _getToken());
                }
                catch(EdgeDBErrorException) { }
            }
        }

        private class TestType
        {
            [EdgeDBProperty("name")]
            public string? Name { get; set; }
        }
    }
}

