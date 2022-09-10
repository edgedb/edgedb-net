using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Unit.Connection
{
    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void HostAndUser()
        {
            Expect(ParseConnection(configure: x =>
            {
                x.Username = "user";
                x.Hostname = "localhost";
            }), new EdgeDBConnection
            {
                Hostname = "localhost",
                Port = 5656,
                Username = "user",
                Database = "edgedb",
                TLSSecurity = TLSSecurityMode.Strict
            });
        }

        [TestMethod]
        public void AllEnviromentVariables()
        {
            Expect(ParseConnection(envVars: new Dictionary<string, string>()
            {
                {"EDGEDB_USER", "user" },
                {"EDGEDB_DATABASE", "testdb" },
                {"EDGEDB_PASSWORD", "passw" },
                {"EDGEDB_HOST", "host" },
                {"EDGEDB_PORT", "123" },
            }), new EdgeDBConnection
            {
                Hostname = "host",
                Port = 123,
                Username = "user",
                Password = "passw",
                Database = "testdb",
                TLSSecurity = TLSSecurityMode.Strict
            });
        }

        [TestMethod]
        public void OptionsBeforeEnv()
        {
            Expect(ParseConnection(configure: x =>
            {
                x.Hostname = "host2";
                x.Port = 456;
                x.Username = "user2";
                x.Password = "passw2";
                x.Database = "db2";
            }, envVars: new Dictionary<string, string>()
            {
                {"EDGEDB_USER", "user" },
                {"EDGEDB_DATABASE", "testdb" },
                {"EDGEDB_PASSWORD", "passw" },
                {"EDGEDB_HOST", "host" },
                {"EDGEDB_PORT", "123" },
            }), new EdgeDBConnection
            {
                Hostname = "host2",
                Port = 456,
                Username = "user2",
                Password = "passw2",
                Database = "db2",
                TLSSecurity = TLSSecurityMode.Strict
            });
        }

        [TestMethod]
        public void DSNBeforeEnv()
        {
            Expect(ParseConnection(dsn: "edgedb://user3:123123@localhost:5555/abcdef", envVars: new Dictionary<string, string>()
            {
                {"EDGEDB_USER", "user" },
                {"EDGEDB_DATABASE", "testdb" },
                {"EDGEDB_PASSWORD", "passw" },
                {"EDGEDB_HOST", "host" },
                {"EDGEDB_PORT", "123" },
            }), new EdgeDBConnection
            {
                Hostname = "localhost",
                Port = 5555,
                Username = "user3",
                Password = "123123",
                Database = "abcdef",
                TLSSecurity = TLSSecurityMode.Strict
            });
        }

        [TestMethod]
        public void DSNOnly()
        {
            Expect(ParseConnection(dsn: "edgedb://user3:123123@localhost:5555/abcdef"), new EdgeDBConnection
            {
                Hostname = "localhost",
                Port = 5555,
                Username = "user3",
                Password = "123123",
                Database = "abcdef",
                TLSSecurity = TLSSecurityMode.Strict
            });
        }

        [TestMethod]
        public void DSNWithMultipleHosts()
        {
            ExpectError<ConfigurationException>(ParseConnection(dsn: "edgedb://user@host1,host2/db"),
                "DSN cannot contain more than one host");
        }

        [TestMethod]
        public void DSNWIthMultipleHostsAndPorts()
        {
            ExpectError<ConfigurationException>(ParseConnection("edgedb://user@host1:1111,host2:2222/db"),
                "DSN cannot contain more than one host");
        }

        [TestMethod]
        public void EnviromentVariablesWithMultipleHostsAndPorts()
        {
            ExpectError<ConfigurationException>(ParseConnection(envVars: new Dictionary<string, string>()
            {
                {"EDGEDB_HOST", "host1:1111,host2:2222" },
                {"EDGEDB_USER", "foo" }
            }), "Enviroment variable 'EDGEDB_HOST' cannot contain more than one host");
        }

        [TestMethod]
        public void QueryParametersWithMultipleHostsAndPorts()
        {
            ExpectError<ConfigurationException>(ParseConnection(dsn: "edgedb:///db?host=host1:1111,host2:2222", envVars: new Dictionary<string, string>()
            {
                {"EDGEDB_USER", "foo" }
            }), "DSN cannot contain more than one host");
        }

        [TestMethod]
        public void MultipleCompoundOptions()
        {
            ExpectError<ConfigurationException>(ParseConnection(dsn: "edgedb:///db", configure: x => x.Hostname = "host1", envVars: new Dictionary<string, string>()
            {
                {"EDGEDB_USER", "foo" }
            }), "Cannot specify DSN and 'Hostname'; they are mutually exclusive");
        }

        [TestMethod]
        public void DSNWithUnixSocket()
        {
            ExpectError<ConfigurationException>(ParseConnection(dsn: "edgedb:///dbname?host=/unix_sock/test&user=spam"),
                "Cannot use UNIX socket for 'Hostname'");
        }

        [TestMethod]
        public void DSNRequiresEdgeDBSchema()
        {
            ExpectError<ConfigurationException>(ParseConnection(dsn: "pq:///dbname?host=/unix_sock/test&user=spam"),
                "DSN schema 'edgedb' expected but got 'pq'");
        }

        [TestMethod]
        public void DSNQueryParameterWithUnixSocket()
        {
            ExpectError<ConfigurationException>(ParseConnection(dsn: "edgedb://user@?port=56226&host=%2Ftmp"),
                "Cannot use UNIX socket for 'Hostname'");
        }

        private static void Expect(Result result, EdgeDBConnection expected)
        {
            Assert.IsNotNull(result.Connection);
            var actual = result.Connection;

            Assert.AreEqual(expected.Username, actual.Username);
            Assert.AreEqual(expected.Password, actual.Password);
            Assert.AreEqual(expected.Hostname, actual.Hostname);
            Assert.AreEqual(expected.Port, actual.Port);
            Assert.AreEqual(expected.Database, actual.Database);
            Assert.AreEqual(expected.TLSCertificateAuthority, actual.TLSCertificateAuthority);
            Assert.AreEqual(expected.TLSSecurity, actual.TLSSecurity);
        }

        private static void ExpectError<TError>(Result result, string message)
        {
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOfType(result.Exception, typeof(TError), $"Exception type {typeof(TError)} expected but got {result.Exception.GetType()}");
            Assert.AreEqual(message, result.Exception.Message);
        }

        private static Result ParseConnection(string? dsn = null, Action<EdgeDBConnection>? configure = null, Dictionary<string, string>? envVars = null)
        {
            try
            {
                // set envs
                if(envVars is not null)
                {
                    foreach (var env in envVars)
                    {
                        Environment.SetEnvironmentVariable(env.Key, env.Value);
                    }
                }

                return EdgeDBConnection.Parse(dsn: dsn, configure: configure, autoResolve: false);
            }
            catch (Exception x)
            {
                return x;
            }
            finally
            {
                // clear env variables
                if (envVars is not null)
                {
                    foreach (var env in envVars)
                    {
                        Environment.SetEnvironmentVariable(env.Key, null);
                    }
                }
            }
        }

        private class Result
        {
            public EdgeDBConnection? Connection { get; init; }
            public Exception? Exception { get; init; }

            public static implicit operator Result(EdgeDBConnection c) => new() { Connection = c };
            public static implicit operator Result(Exception x) => new() { Exception = x };
        }
    }
}
