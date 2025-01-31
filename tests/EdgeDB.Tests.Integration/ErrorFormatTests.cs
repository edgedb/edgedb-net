using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration;

[TestClass]
public class ErrorFormatTests
{
    private readonly GelClientPool _clientPool;
    private readonly Func<CancellationToken> _getToken;

    public ErrorFormatTests()
    {
        _clientPool = ClientProvider.ClientPool;
        _getToken = () => ClientProvider.GetTimeoutToken();
    }

    [TestMethod]
    public async Task TestErrorFormat()
    {
        var exception = await Assert.ThrowsExceptionAsync<ServerErrorException>(async () =>
        {
            await _clientPool.QueryAsync<object>("select {\n    ver := sys::get_version(),\n    unknown := .abc,\n};",
                token: _getToken());
        });

        Assert.AreEqual(
            "InvalidReferenceError: object type 'std::FreeObject' has no link or property 'abc'\n   |\n 3 |     unknown := .abc,\n   |                ^^^^\n",
            exception.ToString());
    }
}
