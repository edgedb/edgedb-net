using Gel.Abstractions;
using Gel.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Gel.Tests.Unit;

[TestClass]
public class SharedClientTests
{
    [TestMethod]
    public void TestConnectParams()
    {
        StreamReader reader = new("shared-client-testcases/connection_testcases.json");
        List<TestCase>? testcases = JsonConvert.DeserializeObject<List<TestCase>>(reader.ReadToEnd());
        if (testcases is null)
        {
            throw new JsonException("Failed to read 'connection_testcases.json.\n"
                + "Is the 'shared-client-testcases' submodule initialised? "
                + "Try running 'git submodule update --init'.");
        }

        foreach ((int textIndex, TestCase testCase) in testcases.Select((x, i) => (i, x)))
        {
            if (testCase.FileSystem is not null
                && (
                    !(testCase.Platform is null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ||
                    !(testCase.Platform == "windows" && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ||
                    !(testCase.Platform == "macos" && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    ))
            {
                // skipping unsupported platform test
                continue;
            }

            if ((testCase.Result is null) == (testCase.Error is null))
            {
                throw new Exception("invalid test case: either \"result\" or \"error\" key has to be specified");
            }

            TestResult result = ParseConnection(testCase);

            if (testCase.Result is not null)
            {
                AssertSameConnection(result, testCase.Result);
            }
            else if (testCase.Error is not null)
            {
                AssertSameException(result, testCase.Error);
            }
        }
    }

    private class TestResult
    {
        public GelConnection? Connection { get; init; }
        public Exception? Exception { get; init; }

        public static implicit operator TestResult(GelConnection c) => new() {Connection = c};
        public static implicit operator TestResult(Exception x) => new() {Exception = x};
    }

    private static TestResult ParseConnection(TestCase testCase)
    {
        try
        {
            ISystemProvider mockSystem = new MockSystemProvider(testCase);

            GelConnection.Options config = new()
            {
                Instance = testCase?.Options?.Instance,
                Dsn = testCase?.Options?.Dsn,
                Host = testCase?.Options?.Host,
                Port = (
                    testCase?.Options?.Port is null
                        ? null
                        : int.TryParse(testCase?.Options?.Port, out var parsedPort)
                            ? parsedPort
                            : throw new ConfigurationException(
                                $"Invalid port: {testCase?.Options?.Port}, not an integer")
                ),
                Database = testCase?.Options?.Database,
                Branch = testCase?.Options?.Branch,
                User = testCase?.Options?.User,
                Password = testCase?.Options?.Password,
                SecretKey = testCase?.Options?.SecretKey,
                Credentials = testCase?.Options?.Credentials,
                CredentialsFile = testCase?.Options?.CredentialsFile,
                TLSCertificateAuthority = testCase?.Options?.TlsCA,
                TLSCertificateAuthorityFile = testCase?.Options?.TlsCAFile,
                TLSSecurity = (
                    testCase?.Options?.TlsSecurity is null
                        ? null
                        : TLSSecurityModeParser.Parse(testCase.Options.TlsSecurity)
                ),
                TLSServerName = testCase?.Options?.TlsServerName,
                WaitUntilAvailable = testCase?.Options?.WaitUntilAvailable,
                ServerSettings = testCase?.Options?.ServerSettings,
            };

            GelConnection connection = GelConnection._Create(config, mockSystem);

            return connection;
        }
        catch (Exception x)
        {
            return x;
        }
    }

    #region Test Assertions

    private static void AssertSameConnection(TestResult result, TestCase.ExpectedResult expectedResult)
    {
        string expectedHostname = expectedResult.Address is not null
            ? expectedResult.Address[0]
            : "localhost";
        int expectedPort = expectedResult.Address is not null
            ? int.Parse(expectedResult.Address[1])
            : 5656;
        string expectedDatabase = expectedResult.Database;
        string expectedBranch = expectedResult.Branch;
        string expectedUsername = expectedResult.User;
        string expectedPassword = expectedResult.Password ?? "";
        string? expectedSecretKey = expectedResult.SecretKey;
        string? expectedTLSCertificateAuthority = expectedResult.TlsCAData;
        TLSSecurityMode expectedTLSSecurity = expectedResult.TlsSecurity ?? TLSSecurityMode.Strict;
        string? expectedTLSServerName = expectedResult.TlsServerName;
        int expectedWaitUntilAvailable = 
            expectedResult.WaitUntilAvailable is not null
            && ConfigUtils.TryGetFieldValue(
                ConfigUtils.ParseWaitUntilAvailable(expectedResult.WaitUntilAvailable),
                out int timeout)
            ? timeout
            : 30000;

        Assert.IsNull(result.Exception, $"\"{result.Exception?.Message}\"\n{result.Exception?.StackTrace}");
        Assert.IsNotNull(result.Connection);
        GelConnection actual = result.Connection;

        Assert.AreEqual(expectedHostname, actual.Hostname);
        Assert.AreEqual(expectedPort, actual.Port);
        Assert.AreEqual(expectedDatabase, actual.Database);
        Assert.AreEqual(expectedBranch, actual.Branch);
        Assert.AreEqual(expectedUsername, actual.Username);
        Assert.AreEqual(expectedPassword, actual.Password);
        Assert.AreEqual(expectedSecretKey, actual.SecretKey);
        Assert.AreEqual(expectedTLSCertificateAuthority, actual.TLSCertificateAuthority);
        Assert.AreEqual(expectedTLSSecurity, actual.TLSSecurity);
        Assert.AreEqual(expectedTLSServerName, actual.TLSServerName);
        Assert.AreEqual(expectedWaitUntilAvailable, actual.WaitUntilAvailable);
        CollectionAssert.AreEqual(expectedResult.ServerSettings, actual.ServerSettings);
    }

    private static void AssertSameException(TestResult result, TestCase.ExpectedError expectedError)
    {
        (Type expectedType, Regex expectedRegex) = _errorMapping[expectedError.Type];

        Assert.IsNull(result.Connection);
        Assert.IsNotNull(result.Exception);
        Exception actual = result.Exception;
        
        Assert.IsInstanceOfType(
            actual,
            expectedType,
            $"Exception type {expectedType} expected but got {result.Exception.GetType()}\n"
            + $"{result.Exception.Message}\n"
            + $"{result.Exception?.StackTrace}");
        Assert.IsTrue(
            expectedRegex.Match(actual.Message).Success,
            $"Exception message \"{actual.Message}\" does not match pattern \"{expectedRegex}\"\n"
            + $"{result.Exception?.StackTrace}");
    }

    private static readonly Dictionary<string, (Type, Regex)> _errorMapping = new()
    {
        {
            "credentials_file_not_found",
            (
                typeof(ConfigurationException),
                new("cannot read credentials", RegexOptions.Compiled)
            )
        },
        {
            "project_not_initialised",
            (
                typeof(ConfigurationException),
                new("Found `\\w+.toml` but the project is not initialized", RegexOptions.Compiled)
            )
        },
        {
            "no_options_or_toml",
            (
                typeof(ConfigurationException),
                new("No `gel.toml` found and no connection options specified", RegexOptions.Compiled)
            )
        },
        {
            "invalid_credentials_file",
            (
                typeof(ConfigurationException),
                new("Invalid CredentialsFile", RegexOptions.Compiled)
            )
        },
        {
            "invalid_dsn_or_instance_name",
            (
                typeof(ConfigurationException),
                new("Invalid (?:DSN|instance name)", RegexOptions.Compiled)
            )
        },
        {
            "invalid_instance_name",
            (
                typeof(ConfigurationException),
                new("invalid instance name", RegexOptions.Compiled)
            )
        },
        {
            "invalid_dsn",
            (
                typeof(ConfigurationException),
                new("Invalid DSN", RegexOptions.Compiled)
            )
        },
        {
            "unix_socket_unsupported",
            (
                typeof(ConfigurationException),
                new("unix socket paths not supported", RegexOptions.Compiled)
            )
        },
        {
            "invalid_host",
            (
                typeof(ConfigurationException),
                new("Invalid host", RegexOptions.Compiled)
            )
        },
        {
            "invalid_port",
            (
                typeof(ConfigurationException),
                new("Invalid port", RegexOptions.Compiled)
            )
        },
        {
            "invalid_user",
            (
                typeof(ConfigurationException),
                new("Invalid user", RegexOptions.Compiled)
            )
        },
        {
            "invalid_database",
            (
                typeof(ConfigurationException),
                new("Invalid database", RegexOptions.Compiled)
            )
        },
        {
            "multiple_compound_env",
            (
                typeof(ConfigurationException),
                new("Cannot have more than one of the following connection environment variables", RegexOptions.Compiled)
            )
        },
        {
            "multiple_compound_opts",
            (
                typeof(ConfigurationException),
                new("Connection options cannot have more than one of the following values", RegexOptions.Compiled)
            )
        },
        {
            "exclusive_options",
            (
                typeof(ConfigurationException),
                new("are mutually exclusive", RegexOptions.Compiled)
            )
        },
        {
            "env_not_found",
            (
                typeof(ConfigurationException),
                new("environment variable \".*\" doesn\'t exist", RegexOptions.Compiled)
            )
        },
        {
            "file_not_found",
            (
                typeof(ConfigurationException),
                new("could not find file", RegexOptions.Compiled)
            )
        },
        {
            "invalid_tls_security",
            (
                typeof(ConfigurationException),
                new(
                    "Invalid TLS Security|\\w+ must be strict when \\w+ is strict",
                    RegexOptions.Compiled)
            )
        },
        {
            "invalid_secret_key",
            (
                typeof(ConfigurationException),
                new("Invalid secret key", RegexOptions.Compiled)
            )
        },
        {
            "secret_key_not_found",
            (
                typeof(ConfigurationException),
                new("Cannot connect to cloud instances without secret key", RegexOptions.Compiled)
            )
        },
        {
            "docker_tcp_port",
            (
                typeof(ConfigurationException),
                new("\\w+_PORT in \"tcp://host:port\" format, so will be ignored", RegexOptions.Compiled)
            )
        },
        {
            "gel_and_edgedb",
            (
                typeof(ConfigurationException),
                new("Both GEL_\\w+ and EDGEDB_\\w+ are set; EDGEDB_\\w+ will be ignored", RegexOptions.Compiled)
            )
        },
    };

    #endregion

    #region MockSystemProvider

    class MockSystemProvider : BaseDefaultSystemProvider
    {
        private readonly string? _homeDir;
        private readonly string? _currentDir;
        private readonly Dictionary<string, string> _envVars;
        private Dictionary<string, string> _files;

        public List<string> Warnings { get; } = new();

        public MockSystemProvider(TestCase testCase)
        {
            _homeDir = testCase.FileSystem?.HomeDir;
            _currentDir = testCase.FileSystem?.CurrentDir;
            _envVars = testCase.EnvVars ?? new();
            _files = CacheFiles(testCase?.FileSystem?.Files);
        }

        private Dictionary<string, string> CacheFiles(Dictionary<string, TestCase.File>? files)
        {
            return files?.SelectMany(
                x => {
                    string path = x.Key;
                    TestCase.File file = x.Value;

                    if (file.Contents is not null)
                    {
                        return new List<(string, string)>(){(path, file.Contents)};
                    }
                    else
                    {
                        if (file.Fields is null)
                        {
                            throw new Exception("File must be either string or json object of fields");
                        }
                        if (!file.Fields.ContainsKey("project-path"))
                        {
                            throw new Exception("File as object must have \"project-path\" field");
                        }

                        List<(string,string)> subfiles = new();

                        string dir = path.Replace("${HASH}", ProjectPathHash(file.Fields["project-path"]));

                        foreach (KeyValuePair<string, string> field in file.Fields)
                        {
                            subfiles.Add((CombinePaths(new string[]{ dir, field.Key }), field.Value));
                        }

                        return subfiles;
                    }
                })
                .ToDictionary(x => x.Item1, x => x.Item2)
                ?? new();
        }

        private string ProjectPathHash(string path)
        {
            if (IsOSPlatform(OSPlatform.Windows) && !path.StartsWith("\\\\"))
            {
                path = "\\\\?\\" + path;
            }

            return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(path)));
        }

        public override string GetHomeDir() => _homeDir ?? base.GetHomeDir();

        public override string GetCurrentDirectory() => _currentDir ?? base.GetCurrentDirectory();

        public override string? GetEnvVariable(string name)
            => _envVars.TryGetValue(name, out var val)
                ? val
                : null;

        public override bool FileExists(string path) => _files.ContainsKey(path);

        public override string FileReadAllText(string path) => _files[path];

        public override void WriteWarning(string message)
            => Warnings.Add(message);
    }

    #endregion

    #region TestCase

    class TestCase
    {
        [JsonProperty("name")]
        public string Name { get; init; } = string.Empty;

        [JsonProperty("opts")]
        public OptionsData? Options { get; init; }
        
        [JsonProperty("env")]
        public Dictionary<string, string>? EnvVars { get; init; }

        [JsonProperty("platform")]
        public string? Platform { get; init; }

        [JsonProperty("fs")]
        public FileSystemData? FileSystem { get; init; }

        [JsonProperty("warnings")]
        public List<string>? Warnings { get; init; }

        [JsonProperty("result")]
        public ExpectedResult? Result { get; init; }

        [JsonProperty("error")]
        public ExpectedError? Error { get; init; }

        public class OptionsData
        {
            [JsonProperty("instance")]
            public string? Instance { get; init; }

            [JsonProperty("dsn")]
            public string? Dsn { get; init; }

            [JsonProperty("host")]
            public string? Host { get; init; }

            [JsonProperty("port")]
            [JsonConverter(typeof(AsStringConverter))]
            public string? Port { get; init; }

            [JsonProperty("database")]
            public string? Database { get; init; }

            [JsonProperty("branch")]
            public string? Branch { get; init; }

            [JsonProperty("user")]
            public string? User { get; init; }

            [JsonProperty("password")]
            public string? Password { get; init; }

            [JsonProperty("secretKey")]
            public string? SecretKey { get; init; }

            [JsonProperty("credentials")]
            public string? Credentials { get; init; }

            [JsonProperty("credentialsFile")]
            public string? CredentialsFile { get; init; }

            [JsonProperty("tlsCA")]
            public string? TlsCA { get; init; }

            [JsonProperty("tlsCAFile")]
            public string? TlsCAFile { get; init; }

            [JsonProperty("tlsSecurity")]
            public string? TlsSecurity { get; init; }

            [JsonProperty("tlsServerName")]
            public string? TlsServerName { get; init; }

            [JsonProperty("waitUntilAvailable")]
            public string? WaitUntilAvailable { get; init; }

            [JsonProperty("serverSettings")]
            public Dictionary<string, string>? ServerSettings { get; init; }
        }

        public class Credentials
        {
            [JsonProperty("host")]
            public string? Host { get; init; }

            [JsonProperty("port")]
            [JsonConverter(typeof(AsStringConverter))]
            public string? Port { get; init; }

            [JsonProperty("database")]
            public string? Database { get; init; }

            [JsonProperty("branch")]
            public string? Branch { get; init; }

            [JsonProperty("user")]
            public string? User { get; init; }

            [JsonProperty("password")]
            public string? Password { get; init; }

            [JsonProperty("tls_ca")]
            public string? TlsCA { get; init; }

            [JsonProperty("tls_security")]
            [JsonConverter(typeof(TLSSecurityModeParser))]
            public TLSSecurityMode? TlsSecurity { get; init; }
        }

        public class FileSystemData
        {
            [JsonProperty("cwd")]
            public string? CurrentDir { get; init; }

            [JsonProperty("homedir")]
            public string? HomeDir { get; init; }

            [JsonProperty("files")]
            public Dictionary<string, File>? Files { get; init; }
        }

        [JsonConverter(typeof(FileJsonConverter))]
        public class File
        {
            // Has either string contents or has explicitly defined instance information

            // string contents
            public string? Contents { get; init; }

            // instance information
            public Dictionary<string, string>? Fields { get; init; }
        }

        public class ExpectedResult
        {
            [JsonProperty("address")]
            [JsonConverter(typeof(AsListStringConverter))]
            public List<string> Address { get; init; } = new();

            [JsonProperty("database")]
            public string Database { get; init; } = string.Empty;

            [JsonProperty("branch")]
            public string Branch { get; init; } = string.Empty;

            [JsonProperty("user")]
            public string User { get; init; } = string.Empty;

            [JsonProperty("password")]
            public string? Password { get; init; }

            [JsonProperty("secretKey")]
            public string? SecretKey { get; init; }

            [JsonProperty("tlsCAData")]
            public string? TlsCAData { get; init; }

            [JsonProperty("tlsSecurity")]
            [JsonConverter(typeof(TLSSecurityModeParser))]
            public TLSSecurityMode? TlsSecurity { get; init; }

            [JsonProperty("tlsServerName")]
            public string? TlsServerName { get; init; }

            [JsonProperty("waitUntilAvailable")]
            public string? WaitUntilAvailable { get; init; }

            [JsonProperty("serverSettings")]
            public Dictionary<string, string>? ServerSettings { get; init; }
        }

        public class ExpectedError
        {
            [JsonProperty("type")]
            public string Type { get; init; } = string.Empty;
        }

        private class AsListStringConverter : JsonConverter<List<string>>
        {
            // Always reads numbers as strings

            public override List<string>? ReadJson(
                JsonReader reader,
                Type objectType,
                List<string>? existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                List<string> result = new();

                // skip JsonToken.StartArray
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndArray)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonToken.Integer)
                    {
                        result.Add(reader.Value!.ToString()!);
                    }
                    else if (reader.TokenType == JsonToken.Float)
                    {
                        result.Add(reader.Value!.ToString()!);
                    }
                    else if (reader.TokenType == JsonToken.String)
                    {
                        result.Add((string)reader.Value!);
                    }
                    else
                    {
                        throw new JsonException(
                            $"Invalid {reader.TokenType} token: \"{reader.Value}\", expected Number or String.");
                    }
                }

                return result;
            }

            public override void WriteJson(
                JsonWriter writer, List<string>? value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        private class FileJsonConverter : JsonConverter<File>
        {
            public override File? ReadJson(
                JsonReader reader,
                Type objectType,
                File? existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.String)
                {
                    // string contents
                    return new()
                    {
                        Contents = (string)reader.Value!,
                    };
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    // instance information
                    Dictionary<string, string> fields = new();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.EndObject)
                        {
                            break;
                        }

                        if (reader.TokenType != JsonToken.PropertyName)
                        {
                            throw new JsonException(
                                $"Invalid {reader.TokenType} token: \"{reader.Value}\", expected PropertyName.");
                        }

                        string propertyName = (string)reader.Value!;

                        reader.Read();
                        if (reader.TokenType != JsonToken.String)
                        {
                            throw new JsonException(
                                $"Invalid {reader.TokenType} token: \"{reader.Value}\", expected String.");
                        }

                        string value = (string)reader.Value!;

                        fields[propertyName] = value;
                    }

                    return new()
                    {
                        Fields = fields,
                    };
                }
                else
                {
                    throw new JsonException("Could not read File object.");
                }
            }

            public override void WriteJson(
                JsonWriter writer, File? value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }

    #endregion
}
