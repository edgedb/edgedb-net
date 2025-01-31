using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration;

[TestClass]
public class HttpClientTests : ClientTests
{
    public HttpClientTests()
    {
        ClientPool = ClientProvider.HttpClientPool;
    }

    [TestMethod]
    public override Task TestPoolTransactions()
    {
        Assert.ThrowsExceptionAsync<EdgeDBException>(() => base.TestPoolTransactions());
        return Task.CompletedTask;
    }
}
