using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Gel.Tests.Integration;

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
        Assert.ThrowsExceptionAsync<GelException>(() => base.TestPoolTransactions());
        return Task.CompletedTask;
    }
}
