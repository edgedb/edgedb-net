using EdgeDB.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration;

[TestClass]
public class DMLTests
{
    private readonly EdgeDBClient _client;
    private readonly EdgeDBClient _ddlClient;
    private readonly Func<CancellationToken> _getToken;


    public DMLTests()
    {
        _client = ClientProvider.EdgeDB;
        _ddlClient = _client.WithConfig(x => x.DDLPolicy = DDLPolicy.AlwaysAllow);
        _getToken = () => ClientProvider.GetTimeoutToken();
    }

    [TestMethod]
    public async Task TestInsert()
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

    [TestMethod]
    public async Task TestDelete()
    {
        try
        {
            await _ddlClient.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                capabilities: Capabilities.All, token: _getToken());

            await _ddlClient.ExecuteAsync("delete TestType", token: _getToken());
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

    [TestMethod]
    public async Task TestInsertTransaction()
    {
        try
        {
            await _ddlClient.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                capabilities: Capabilities.All, token: _getToken());

            await _ddlClient.TransactionAsync(async transaction =>
            {
                var testResult = await _ddlClient.QueryRequiredSingleAsync<TestType>(
                    "with t := (insert TestType { name := 'test' }) select t { name } limit 1", token: _getToken());

                Assert.IsNotNull(testResult);
                Assert.AreEqual("test", testResult.Name);
            });
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

    [TestMethod]
    public async Task TestDeleteTransaction()
    {
        try
        {
            await _ddlClient.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                capabilities: Capabilities.All, token: _getToken());

            await _ddlClient.TransactionAsync(async transaction =>
            {
                await transaction.ExecuteAsync("delete TestType", token: _getToken());
            });
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
