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
    public class HttpClientTests : ClientTests
    {
        public HttpClientTests()
        {
            EdgeDB = ClientProvider.HttpEdgeDB;
        }

        [TestMethod]
        public override Task TestPoolTransactions()
        {
            Assert.ThrowsExceptionAsync<EdgeDBException>(() => base.TestPoolTransactions());
            return Task.CompletedTask;
        }
    }
}
