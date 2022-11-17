using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration
{
    [TestClass]
    public class ErrorFormatTests
    {
        private readonly EdgeDBClient _client;
        private readonly Func<CancellationToken> _getToken;

        public ErrorFormatTests()
        {
            _client = ClientProvider.EdgeDB;
            _getToken = () => ClientProvider.GetTimeoutToken();
        }

        [TestMethod]
        public async Task TestErrorFormat()
        {
            var exception = await Assert.ThrowsExceptionAsync<EdgeDBErrorException>(async () =>
            {
                await _client.QueryAsync<object>("select {\n    ver := sys::get_version(),\n    unknown := .abc,\n};", token: _getToken());
            });

            Assert.AreEqual($"InvalidReferenceError: object type 'std::FreeObject' has no link or property 'abc'\n   |\n 3 |     unknown := .abc,\n   |                ^^^^\n", exception.ToString());
        }
    }
}
