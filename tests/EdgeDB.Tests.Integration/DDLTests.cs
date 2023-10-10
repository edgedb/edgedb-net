using EdgeDB.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration;

[TestClass]
public class DDLTests
{
    private readonly EdgeDBClient _client;
    private readonly EdgeDBClient _ddlClient;
    private readonly Func<CancellationToken> _getToken;


    public DDLTests()
    {
        _client = ClientProvider.EdgeDB;
        _ddlClient = _client.WithConfig(x => x.DDLPolicy = DDLPolicy.AlwaysAllow);
        _getToken = () => ClientProvider.GetTimeoutToken();
    }

    [TestMethod]
    public async Task TestDDLInvalidCapabilitiesAndSessionConfig() =>
        await Assert.ThrowsExceptionAsync<EdgeDBErrorException>(async () =>
        {
            await _client.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                token: _getToken());
        });

    [TestMethod]
    public async Task TestDDLInvalidCapabilitiesAndValidConfig() =>
        await Assert.ThrowsExceptionAsync<EdgeDBErrorException>(async () =>
        {
            await _ddlClient.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                token: _getToken());
        });

    [TestMethod]
    public async Task TestDDLValidConfigAndCapabilities()
    {
        try
        {
            await _ddlClient.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                capabilities: Capabilities.All, token: _getToken());

            var testResult = await _ddlClient.QueryRequiredSingleAsync<TestType>(
                "with t := (insert TestType { name := 'test' }) select t { name } limit 1", token: _getToken());

            Assert.IsNotNull(testResult);
            Assert.AreEqual("test", testResult.Name);
        }
        finally
        {
            // try to drop the type, don't throw if this fails
            try
            {
                await _ddlClient.ExecuteAsync("DROP TYPE TestType", capabilities: Capabilities.All, token: _getToken());
            }
            catch (EdgeDBErrorException) { }
        }
    }

    private class TestType
    {
        [EdgeDBProperty("name")] public string? Name { get; set; }
    }
}
