using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EdgeDB.Tests.Integration
{
    public class ErrorFormatTests : IClassFixture<ClientFixture>
    {
        private readonly EdgeDBClient _client;
        private readonly Func<CancellationToken> _getToken;

        public ErrorFormatTests(ClientFixture fixture)
        {
            _client = fixture.EdgeDB;
            _getToken = () => fixture.GetTimeoutToken();
        }

        [Fact]
        public async Task TestErrorFormat()
        {
            var exception = await Assert.ThrowsAsync<EdgeDBErrorException>(async () =>
            {
                await _client.QueryAsync<object>("select {\n    ver := sys::get_version(),\n    unknown := .abc,\n};", token: _getToken());
            });

            Assert.Equal("InvalidReferenceError: object type 'std::FreeObject' has no link or property 'abc'\n   |\n 3 |     unknown := .abc,\n   |                ^^^^\n", exception.ToString());
        }
    }
}
