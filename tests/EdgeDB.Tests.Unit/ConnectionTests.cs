using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace EdgeDB.Tests.Unit;

[TestClass]
public class ConnectionTests
{
    #region Parse

    [TestMethod]
    public void HostAndUser() =>
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

    [TestMethod]
    public void AllEnviromentVariables() =>
        Expect(
            ParseConnection(envVars: new Dictionary<string, string>
            {
                {"EDGEDB_USER", "user"},
                {"EDGEDB_DATABASE", "testdb"},
                {"EDGEDB_PASSWORD", "passw"},
                {"EDGEDB_HOST", "host"},
                {"EDGEDB_PORT", "123"}
            }), new EdgeDBConnection
            {
                Hostname = "host",
                Port = 123,
                Username = "user",
                Password = "passw",
                Database = "testdb",
                TLSSecurity = TLSSecurityMode.Strict
            });

    [TestMethod]
    public void OptionsBeforeEnv() =>
        Expect(ParseConnection(configure: x =>
            {
                x.Hostname = "host2";
                x.Port = 456;
                x.Username = "user2";
                x.Password = "passw2";
                x.Database = "db2";
            },
            envVars: new Dictionary<string, string>
            {
                {"EDGEDB_USER", "user"},
                {"EDGEDB_DATABASE", "testdb"},
                {"EDGEDB_PASSWORD", "passw"},
                {"EDGEDB_HOST", "host"},
                {"EDGEDB_PORT", "123"}
            }), new EdgeDBConnection
        {
            Hostname = "host2",
            Port = 456,
            Username = "user2",
            Password = "passw2",
            Database = "db2",
            TLSSecurity = TLSSecurityMode.Strict
        });

    [TestMethod]
    public void DSNBeforeEnv() =>
        Expect(
            ParseConnection("edgedb://user3:123123@localhost:5555/abcdef",
                envVars: new Dictionary<string, string>
                {
                    {"EDGEDB_USER", "user"},
                    {"EDGEDB_DATABASE", "testdb"},
                    {"EDGEDB_PASSWORD", "passw"},
                    {"EDGEDB_HOST", "host"},
                    {"EDGEDB_PORT", "123"}
                }),
            new EdgeDBConnection
            {
                Hostname = "localhost",
                Port = 5555,
                Username = "user3",
                Password = "123123",
                Database = "abcdef",
                TLSSecurity = TLSSecurityMode.Strict
            });

    [TestMethod]
    public void DSNOnly() =>
        Expect(ParseConnection("edgedb://user3:123123@localhost:5555/abcdef"),
            new EdgeDBConnection
            {
                Hostname = "localhost",
                Port = 5555,
                Username = "user3",
                Password = "123123",
                Database = "abcdef",
                TLSSecurity = TLSSecurityMode.Strict
            });

    [TestMethod]
    public void DSNWithMultipleHosts() =>
        ExpectError<ConfigurationException>(ParseConnection("edgedb://user@host1,host2/db"),
            "DSN cannot contain more than one host");

    [TestMethod]
    public void DSNWIthMultipleHostsAndPorts() =>
        ExpectError<ConfigurationException>(ParseConnection("edgedb://user@host1:1111,host2:2222/db"),
            "DSN cannot contain more than one host");

    [TestMethod]
    public void EnviromentVariablesWithMultipleHostsAndPorts() =>
        ExpectError<ConfigurationException>(
            ParseConnection(envVars: new Dictionary<string, string>
            {
                {"EDGEDB_HOST", "host1:1111,host2:2222"}, {"EDGEDB_USER", "foo"}
            }), "Enviroment variable 'EDGEDB_HOST' cannot contain more than one host");

    [TestMethod]
    public void QueryParametersWithMultipleHostsAndPorts() =>
        ExpectError<ConfigurationException>(
            ParseConnection("edgedb:///db?host=host1:1111,host2:2222",
                envVars: new Dictionary<string, string> {{"EDGEDB_USER", "foo"}}),
            "DSN cannot contain more than one host");

    [TestMethod]
    public void MultipleCompoundOptions() =>
        ExpectError<ConfigurationException>(
            ParseConnection("edgedb:///db", x => x.Hostname = "host1",
                new Dictionary<string, string> {{"EDGEDB_USER", "foo"}}),
            "Cannot specify DSN and 'Hostname'; they are mutually exclusive");

    [TestMethod]
    public void DSNWithUnixSocket() =>
        ExpectError<ConfigurationException>(ParseConnection("edgedb:///dbname?host=/unix_sock/test&user=spam"),
            "Cannot use UNIX socket for 'Hostname'");

    [TestMethod]
    public void DSNRequiresEdgeDBSchema() =>
        ExpectError<ConfigurationException>(ParseConnection("pq:///dbname?host=/unix_sock/test&user=spam"),
            "DSN schema 'gel' expected but got 'pq'");

    [TestMethod]
    public void DSNQueryParameterWithUnixSocket() =>
        ExpectError<ConfigurationException>(ParseConnection("edgedb://user@?port=56226&host=%2Ftmp"),
            "Cannot use UNIX socket for 'Hostname'");

    [TestMethod]
    public void TestConnectionFormat()
    {
        var connection = EdgeDBConnection.FromDSN("edgedb://user3:123123@localhost:5555/abcdef");

        Assert.AreEqual("gel://user3:123123@localhost:5555/abcdef", connection.ToString());
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
        Assert.IsInstanceOfType(result.Exception, typeof(TError),
            $"Exception type {typeof(TError)} expected but got {result.Exception.GetType()}");
        Assert.AreEqual(message, result.Exception.Message);
    }

    private static Result ParseConnection(string? dsn = null, Action<EdgeDBConnection>? configure = null,
        Dictionary<string, string>? envVars = null)
    {
        try
        {
            // set envs
            if (envVars is not null)
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

        public static implicit operator Result(EdgeDBConnection c) => new() {Connection = c};
        public static implicit operator Result(Exception x) => new() {Exception = x};
    }

    #endregion

    #region Wait until available

    [TestMethod]
    [DataRow(" 1s ", 1)]
    [DataRow(" 1s", 1)]
    [DataRow("-0s", 0)]
    [DataRow("-1.0h", -3600)]
    [DataRow("-1.0hour", -3600)]
    [DataRow("-1.0hours", -3600)]
    [DataRow("-1.0m", -60)]
    [DataRow("-1.0minute", -60)]
    [DataRow("-1.0minutes", -60)]
    [DataRow("-1.0ms", -0.001)]
    [DataRow("-1.0s", -1)]
    [DataRow("-1.0second", -1)]
    [DataRow("-1.0seconds", -1)]
    [DataRow("-1.0us", -0.000001)]
    [DataRow("-1h", -3600)]
    [DataRow("-1hour", -3600)]
    [DataRow("-1hours", -3600)]
    [DataRow("-1m", -60)]
    [DataRow("-1minute", -60)]
    [DataRow("-1minutes", -60)]
    [DataRow("-1ms", -0.001)]
    [DataRow("-1s", -1)]
    [DataRow("-1second", -1)]
    [DataRow("-1seconds", -1)]
    [DataRow("-1us", -0.000001)]
    [DataRow("-2h 60m 3600s", 0)]
    [DataRow("-\t2\thour\t60\tminute\t3600\tsecond", 0)]
    [DataRow(".1h", 360)]
    [DataRow(".1hour", 360)]
    [DataRow(".1hours", 360)]
    [DataRow(".1m", 6)]
    [DataRow(".1minute", 6)]
    [DataRow(".1minutes", 6)]
    [DataRow(".1ms", 0.0001)]
    [DataRow(".1s", 0.1)]
    [DataRow(".1second", 0.1)]
    [DataRow(".1seconds", 0.1)]
    [DataRow(".1us", 0.0000001)]
    [DataRow("1   hour 60  minute -   7200   second", 0)]
    [DataRow("1   hours 60  minutes -   7200   seconds", 0)]
    [DataRow("1.0h", 3600)]
    [DataRow("1.0hour", 3600)]
    [DataRow("1.0hours", 3600)]
    [DataRow("1.0m", 60)]
    [DataRow("1.0minute", 60)]
    [DataRow("1.0minutes", 60)]
    [DataRow("1.0ms", 0.001)]
    [DataRow("1.0s", 1)]
    [DataRow("1.0second", 1)]
    [DataRow("1.0seconds", 1)]
    [DataRow("1.0us", 0.000001)]
    [DataRow("1h -120m 3600s", 0)]
    [DataRow("1h -120m3600s", 0)]
    [DataRow("1h 60m -7200s", 0)]
    [DataRow("1h", 3600)]
    [DataRow("1hour", 3600)]
    [DataRow("1hours -120minutes 3600seconds", 0)]
    [DataRow("1hours", 3600)]
    [DataRow("1m", 60)]
    [DataRow("1minute", 60)]
    [DataRow("1minutes", 60)]
    [DataRow("1ms", 0.001)]
    [DataRow("1s ", 1)]
    [DataRow("1s", 1)]
    [DataRow("1s\t", 1)]
    [DataRow("1second", 1)]
    [DataRow("1seconds", 1)]
    [DataRow("1us", 0.000001)]
    [DataRow("2  h  46  m  39  s", 9999)]
    [DataRow("2  hour  46  minute  39  second", 9999)]
    [DataRow("2  hours  46  minutes  39  seconds", 9999)]
    [DataRow("2.0  h  46.0  m  39.0  s", 9999)]
    [DataRow("2.0  hour  46.0  minute  39.0  second", 9999)]
    [DataRow("2.0  hours  46.0  minutes  39.0  seconds", 9999)]
    [DataRow("2.0h 46.0m 39.0s", 9999)]
    [DataRow("2.0h46.0m39.0s", 9999)]
    [DataRow("2.0hour 46.0minute 39.0second", 9999)]
    [DataRow("2.0hours 46.0minutes 39.0seconds", 9999)]
    [DataRow("2h 46m 39s", 9999)]
    [DataRow("2h46m39s", 9999)]
    [DataRow("2hour 46minute 39second", 9999)]
    [DataRow("2hours 46minutes 39seconds", 9999)]
    [DataRow("39.0\tsecond 2.0  hour  46.0  minute", 9999)]
    [DataRow("PT", 0)]
    [DataRow("PT-.1", -360)]
    [DataRow("PT-.1H", -360)]
    [DataRow("PT-.1M", -6)]
    [DataRow("PT-.1S", -0.1)]
    [DataRow("PT-0.000001S", -0.000001)]
    [DataRow("PT-0S", 0)]
    [DataRow("PT-1", -3600)]
    [DataRow("PT-1.", -3600)]
    [DataRow("PT-1.0", -3600)]
    [DataRow("PT-1.0H", -3600)]
    [DataRow("PT-1.0M", -60)]
    [DataRow("PT-1.0S", -1)]
    [DataRow("PT-1.H", -3600)]
    [DataRow("PT-1.M", -60)]
    [DataRow("PT-1.S", -1)]
    [DataRow("PT-1H", -3600)]
    [DataRow("PT-1M", -60)]
    [DataRow("PT-1S", -1)]
    [DataRow("PT.1", 360)]
    [DataRow("PT.1H", 360)]
    [DataRow("PT.1M", 6)]
    [DataRow("PT.1S", 0.1)]
    [DataRow("PT0.000001S", 0.000001)]
    [DataRow("PT0S", 0)]
    [DataRow("PT1", 3600)]
    [DataRow("PT1.", 3600)]
    [DataRow("PT1.0", 3600)]
    [DataRow("PT1.0H", 3600)]
    [DataRow("PT1.0M", 60)]
    [DataRow("PT1.0M", 60)]
    [DataRow("PT1.0S", 1)]
    [DataRow("PT1.H", 3600)]
    [DataRow("PT1.M", 60)]
    [DataRow("PT1.S", 1)]
    [DataRow("PT1H", 3600)]
    [DataRow("PT1M", 60)]
    [DataRow("PT1S", 1)]
    [DataRow("PT2.0H46.0M39.0S", 9999)]
    [DataRow("PT2H46M39S", 9999)]
    [DataRow("\t-\t2\thours\t60\tminutes\t3600\tseconds\t", 0)]
    [DataRow("\t1s", 1)]
    [DataRow("\t1s\t", 1)]
    public void TestValidWaitUntilAvailable(string input, double expectedSeconds)
    {
        int actualMilliseconds = EdgeDBConnection.ParseWaitUntilAvailable(input);
        Assert.IsTrue(Math.Abs(actualMilliseconds * 0.001 - expectedSeconds) < 0.0005);
    }

    [TestMethod]
    [DataRow(" ")]
    [DataRow(" PT1S")]
    [DataRow("")]
    [DataRow("-.1 s")]
    [DataRow("-.1s")]
    [DataRow("-.5 second")]
    [DataRow("-.5 seconds")]
    [DataRow("-.5second")]
    [DataRow("-.5seconds")]
    [DataRow("-.s")]
    [DataRow("-1.s")]
    [DataRow(".s")]
    [DataRow(".seconds")]
    [DataRow("1.s")]
    [DataRow("1h-120m3600s")]
    [DataRow("1hour-120minute3600second")]
    [DataRow("1hours-120minutes3600seconds")]
    [DataRow("1hours120minutes3600seconds")]
    [DataRow("2.0hour46.0minutes39.0seconds")]
    [DataRow("2.0hours46.0minutes39.0seconds")]
    [DataRow("20 hours with other stuff should not be valid")]
    [DataRow("20 minutes with other stuff should not be valid")]
    [DataRow("20 ms with other stuff should not be valid")]
    [DataRow("20 seconds with other stuff should not be valid")]
    [DataRow("20 us with other stuff should not be valid")]
    [DataRow("2hour46minute39second")]
    [DataRow("2hours46minutes39seconds")]
    [DataRow("3 hours is longer than 10 seconds")]
    [DataRow("P-.D")]
    [DataRow("P-D")]
    [DataRow("PD")]
    [DataRow("PT.S")]
    [DataRow("PT1S ")]
    [DataRow("\t")]
    [DataRow("not a duration")]
    [DataRow("s")]
    public void TestInvalidWaitUntilAvailable(string input)
    {
        void TryParse()
        {
            EdgeDBConnection.ParseWaitUntilAvailable(input);
        }
        Assert.ThrowsException<ConfigurationException>(TryParse);
    }

    #endregion
}
