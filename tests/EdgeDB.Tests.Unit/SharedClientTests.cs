using EdgeDB.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace EdgeDB.Tests.Unit;

[TestClass]
public class SharedClientTests
{
    [TestMethod]
    public void TestConnectParams()
    {
        StreamReader reader = new("shared-client-testcases/connection_testcases.json");
        List<TestCase>? testcases = JsonSerializer.Deserialize<List<TestCase>>(reader.ReadToEnd());
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
        public EdgeDBConnection? Connection { get; init; }
        public Exception? Exception { get; init; }

        public static implicit operator TestResult(EdgeDBConnection c) => new() {Connection = c};
        public static implicit operator TestResult(Exception x) => new() {Exception = x};
    }

    private static TestResult ParseConnection(TestCase testCase)
    {
        try
        {
            ISystemProvider mockSystem = new MockSystemProvider(testCase);

            EdgeDBConnection.Options config = new()
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
                        : TLSSecurityModeParser.Parse(testCase?.Options?.TlsSecurity)
                ),
                TLSServerName = testCase?.Options?.TlsServerName,
                WaitUntilAvailable = testCase?.Options?.WaitUntilAvailable,
                ServerSettings = testCase?.Options?.ServerSettings,
            };

            EdgeDBConnection connection = EdgeDBConnection._Create(config, mockSystem);

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
        EdgeDBConnection expected = new();
        if (expectedResult.Address is not null) expected.Hostname = expectedResult.Address[0];
        if (expectedResult.Address is not null) expected.Port = int.Parse(expectedResult.Address[1]);
        if (expectedResult.Database is not null)
        {
            expected.Branch = null;
            expected.Database = expectedResult.Database;
        }
        if (expectedResult.Branch is not null)
        {
            expected.Database = null;
            expected.Branch = expectedResult.Branch;
        }
        if (expectedResult.User is not null) expected.Username = expectedResult.User;
        if (expectedResult.Password is not null) expected.Password = expectedResult.Password;
        if (expectedResult.TlsCAData is not null) expected.TLSCertificateAuthority = expectedResult.TlsCAData;
        if (expectedResult.TlsServerName is not null) expected.TLSServerName = expectedResult.TlsServerName;
        if (expectedResult.TlsSecurity is not null) expected.TLSSecurity = expectedResult.TlsSecurity.Value;
        if (expectedResult.WaitUntilAvailable is not null)
        {
            if (EdgeDBConnection.TryGetFieldValue(
                EdgeDBConnection.ParseWaitUntilAvailable(expectedResult.WaitUntilAvailable),
                out int timeout))
            {
                expected.Timeout = timeout;
            }
        }
        if (expectedResult.ServerSettings is not null) expected.ServerSettings = expectedResult.ServerSettings;

        Assert.IsNull(result.Exception, $"\"{result.Exception?.Message}\"\n{result.Exception?.StackTrace}");
        Assert.IsNotNull(result.Connection);
        EdgeDBConnection actual = result.Connection;

        static string ResolveHostname(string hostname)
        {
            return hostname == "localhost" ? "127.0.0.1" : hostname;
        }
        Assert.AreEqual(ResolveHostname(expected.Hostname), ResolveHostname(actual.Hostname));
        Assert.AreEqual(expected.Port, actual.Port);
        Assert.AreEqual(expected.Database, actual.Database);
        Assert.AreEqual(expected.Username, actual.Username);
        Assert.AreEqual(expected.Password, actual.Password);
        Assert.AreEqual(expected.TLSCertificateAuthority, actual.TLSCertificateAuthority);
        Assert.AreEqual(expected.TLSServerName, actual.TLSServerName);
        Assert.AreEqual(expected.TLSSecurity, actual.TLSSecurity);
        CollectionAssert.AreEqual(expected.ServerSettings, actual.ServerSettings);
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
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("opts")]
        public OptionsData? Options { get; init; }
        
        [JsonPropertyName("env")]
        public Dictionary<string, string>? EnvVars { get; init; }

        [JsonPropertyName("platform")]
        public string? Platform { get; init; }

        [JsonPropertyName("fs")]
        public FileSystemData? FileSystem { get; init; }

        [JsonPropertyName("warnings")]
        public List<string>? Warnings { get; init; }

        [JsonPropertyName("result")]
        public ExpectedResult? Result { get; init; }

        [JsonPropertyName("error")]
        public ExpectedError? Error { get; init; }

        public class OptionsData
        {
            [JsonPropertyName("instance")]
            public string? Instance { get; init; }

            [JsonPropertyName("dsn")]
            public string? Dsn { get; init; }

            [JsonPropertyName("host")]
            public string? Host { get; init; }

            [JsonPropertyName("port")]
            [JsonConverter(typeof(AsStringConverter))]
            public string? Port { get; init; }

            [JsonPropertyName("database")]
            public string? Database { get; init; }

            [JsonPropertyName("branch")]
            public string? Branch { get; init; }

            [JsonPropertyName("user")]
            public string? User { get; init; }

            [JsonPropertyName("password")]
            public string? Password { get; init; }

            [JsonPropertyName("secretKey")]
            public string? SecretKey { get; init; }

            [JsonPropertyName("credentials")]
            public string? Credentials { get; init; }

            [JsonPropertyName("credentialsFile")]
            public string? CredentialsFile { get; init; }

            [JsonPropertyName("tlsCA")]
            public string? TlsCA { get; init; }

            [JsonPropertyName("tlsCAFile")]
            public string? TlsCAFile { get; init; }

            [JsonPropertyName("tlsSecurity")]
            public string? TlsSecurity { get; init; }

            [JsonPropertyName("tlsServerName")]
            public string? TlsServerName { get; init; }

            [JsonPropertyName("waitUntilAvailable")]
            public string? WaitUntilAvailable { get; init; }

            [JsonPropertyName("serverSettings")]
            public Dictionary<string, string>? ServerSettings { get; init; }
        }

        public class Credentials
        {
            [JsonPropertyName("host")]
            public string? Host { get; init; }

            [JsonPropertyName("port")]
            [JsonConverter(typeof(AsStringConverter))]
            public string? Port { get; init; }

            [JsonPropertyName("database")]
            public string? Database { get; init; }

            [JsonPropertyName("branch")]
            public string? Branch { get; init; }

            [JsonPropertyName("user")]
            public string? User { get; init; }

            [JsonPropertyName("password")]
            public string? Password { get; init; }

            [JsonPropertyName("tls_ca")]
            public string? TlsCA { get; init; }

            [JsonPropertyName("tls_security")]
            [JsonConverter(typeof(TLSSecurityModeConverter))]
            public TLSSecurityMode? TlsSecurity { get; init; }
        }

        public class FileSystemData
        {
            [JsonPropertyName("cwd")]
            public string? CurrentDir { get; init; }

            [JsonPropertyName("homedir")]
            public string? HomeDir { get; init; }

            [JsonPropertyName("files")]
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
            [JsonPropertyName("address")]
            [JsonConverter(typeof(AsListStringConverter))]
            public List<string> Address { get; init; } = new();

            [JsonPropertyName("database")]
            public string Database { get; init; } = string.Empty;

            [JsonPropertyName("branch")]
            public string Branch { get; init; } = string.Empty;

            [JsonPropertyName("user")]
            public string User { get; init; } = string.Empty;

            [JsonPropertyName("password")]
            public string? Password { get; init; }

            [JsonPropertyName("secretKey")]
            public string? SecretKey { get; init; }

            [JsonPropertyName("tlsCAData")]
            public string? TlsCAData { get; init; }

            [JsonPropertyName("tlsServerName")]
            public string? TlsServerName { get; init; }

            [JsonPropertyName("tlsSecurity")]
            [JsonConverter(typeof(TLSSecurityModeConverter))]
            public TLSSecurityMode? TlsSecurity { get; init; }

            [JsonPropertyName("waitUntilAvailable")]
            public string? WaitUntilAvailable { get; init; }

            [JsonPropertyName("serverSettings")]
            public Dictionary<string, string>? ServerSettings { get; init; }
        }

        public class ExpectedError
        {
            [JsonPropertyName("type")]
            public string Type { get; init; } = string.Empty;
        }

        private class AsStringConverter : JsonConverter<string>
        {
            // Always reads numbers as strings

            public override string? Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    if (reader.TryGetInt32(out int asInt))
                    {
                        return asInt.ToString();
                    }
                    else
                    {
                        return reader.GetDouble().ToString();
                    }
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    return reader.GetString();
                }
                else
                {
                    throw new JsonException("Expected Number or String.");
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                string value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        private class AsListStringConverter : JsonConverter<List<string>>
        {
            // Always reads numbers as strings

            public override List<string>? Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                List<string> result = new();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.Number)
                    {
                        if (reader.TryGetInt32(out int asInt))
                        {
                            result.Add(asInt.ToString());
                        }
                        else
                        {
                            result.Add(reader.GetDouble().ToString());
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.String)
                    {
                        string? text = reader.GetString();
                        if (text is null)
                        {
                            throw new JsonException();
                        }
                        result.Add(text);
                    }
                    else
                    {
                        throw new JsonException("Expected Number or String.");
                    }
                }

                return result;
            }

            public override void Write(
                Utf8JsonWriter writer,
                List<string> value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        private class TLSSecurityModeConverter : JsonConverter<TLSSecurityMode?>
        {
            // Always reads numbers as strings

            public override TLSSecurityMode? Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                string? text = reader.GetString();
                if (text is not null)
                {
                    return TLSSecurityModeParser.Parse(text, true);
                }
                else
                {
                    throw new JsonException("Expected String.");
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                TLSSecurityMode? value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        private class FileJsonConverter : JsonConverter<File>
        {
            public override File? Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    // string contents
                    return new()
                    {
                        Contents = reader.GetString(),
                    };
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // instance information
                    Dictionary<string, string> fields = new();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            break;
                        }

                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            throw new JsonException();
                        }

                        string? propertyName = reader.GetString();
                        if (propertyName is not null)
                        {
                            reader.Read();
                            if (reader.TokenType != JsonTokenType.String)
                            {
                                throw new JsonException($"Expected string for property \"{propertyName}\"");
                            }

                            string? value = reader.GetString();
                            if (value == null)
                            {
                                throw new JsonException();
                            }

                            fields[propertyName] = value;
                        }
                        else
                        {
                            throw new JsonException();
                        }
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

            public override void Write(
                Utf8JsonWriter writer,
                File value,
                JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }

    #endregion
}
