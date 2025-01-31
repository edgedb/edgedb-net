using EdgeDB.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration;

[TestClass]
public class DMLTests
{
    private readonly GelClientPool _clientPool;
    private readonly GelClientPool _ddlClientPool;
    private readonly Func<CancellationToken> _getToken;


    public DMLTests()
    {
        _clientPool = ClientProvider.ClientPool;
        _ddlClientPool = _clientPool.WithConfig(x => x.DDLPolicy = DDLPolicy.AlwaysAllow);
        _getToken = () => ClientProvider.GetTimeoutToken();
    }

    [TestMethod]
    public async Task TestInsert()
    {
        try
        {
            await _ddlClientPool.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                capabilities: Capabilities.All, token: _getToken());

            var testResult = await _ddlClientPool.QueryRequiredSingleAsync<TestType>(
                "with t := (insert TestType { name := 'test' }) select t { name } limit 1", token: _getToken());

            Assert.IsNotNull(testResult);
            Assert.AreEqual("test", testResult.Name);
        }
        finally
        {
            // try to drop the type, don't throw if this fails
            try
            {
                await _ddlClientPool.ExecuteAsync("DROP TYPE TestType", capabilities: Capabilities.All, token: _getToken());
            }
            catch (ServerErrorException) { }
        }
    }

    [TestMethod]
    public async Task TestDelete()
    {
        try
        {
            await _ddlClientPool.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                capabilities: Capabilities.All, token: _getToken());

            await _ddlClientPool.ExecuteAsync("delete TestType", token: _getToken());
        }
        finally
        {
            // try to drop the type, don't throw if this fails
            try
            {
                await _ddlClientPool.ExecuteAsync("DROP TYPE TestType", capabilities: Capabilities.All, token: _getToken());
            }
            catch (ServerErrorException) { }
        }
    }

    [TestMethod]
    public async Task TestInsertTransaction()
    {
        try
        {
            await _ddlClientPool.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                capabilities: Capabilities.All, token: _getToken());

            await _ddlClientPool.TransactionAsync(async transaction =>
            {
                var testResult = await _ddlClientPool.QueryRequiredSingleAsync<TestType>(
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
                await _ddlClientPool.ExecuteAsync("DROP TYPE TestType", capabilities: Capabilities.All, token: _getToken());
            }
            catch (ServerErrorException) { }
        }
    }

    [TestMethod]
    public async Task TestDeleteTransaction()
    {
        try
        {
            await _ddlClientPool.ExecuteAsync("CREATE TYPE TestType { CREATE REQUIRED PROPERTY name -> str; }",
                capabilities: Capabilities.All, token: _getToken());

            await _ddlClientPool.TransactionAsync(async transaction =>
            {
                await transaction.ExecuteAsync("delete TestType", token: _getToken());
            });
        }
        finally
        {
            // try to drop the type, don't throw if this fails
            try
            {
                await _ddlClientPool.ExecuteAsync("DROP TYPE TestType", capabilities: Capabilities.All, token: _getToken());
            }
            catch (ServerErrorException) { }
        }
    }

    private class TestType
    {
        [GelProperty("name")] public string? Name { get; set; }
    }
}
