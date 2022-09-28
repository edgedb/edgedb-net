using EdgeDB.Abstractions;
using EdgeDB.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Unit
{
    [TestClass]
    public class ProjectPathHashingTest
    {
        [TestMethod]
        public void TestLinuxDefault()
            => TestProjectHashing(
                OSPlatform.Linux,
                "/home/edgedb",
                "/home/edgedb/test",
                null,
                "/home/edgedb/.config/edgedb/projects/test-cf3c86df8fc33fbb73a47671ac5762eda8219158");

        [TestMethod]
        public void TestLinuxXDGConfig()
            => TestProjectHashing(
                OSPlatform.Linux,
                "/home/edgedb",
                "/home/edgedb/test",
                "/some/other/path",
                "/some/other/path/edgedb/projects/test-cf3c86df8fc33fbb73a47671ac5762eda8219158");

        [TestMethod]
        public void TestMacOS()
            => TestProjectHashing(
                OSPlatform.OSX,
                "/Users/edgedb",
                "/Users/edgedb/test",
                null,
                "/Users/edgedb/Library/Application Support/edgedb/projects/test-bea4557cb17f02c484b4e7f8ea93fc168ecd28d4");

        [TestMethod]
        public void TestWindows()
            => TestProjectHashing(
                OSPlatform.Windows,
                "C:\\Users\\edgedb",
                "C:\\Users\\edgedb\\test",
                null,
                "C:\\Users\\edgedb\\AppData\\Local\\EdgeDB\\config\\projects\\test-461d1a84b3307c343f37187e3722391ce4f8f64c");

        private void TestProjectHashing(OSPlatform platform, string homeDir, string projectDir, string? xdgconf, string expectedResult)
        {
            var mockSystem = new MockSystemProvider(platform, homeDir, new Dictionary<string, string>()
            {
                { "XDG_CONFIG_HOME", xdgconf! }
            },  projectDir, xdgconf ?? "/", expectedResult);

            if (xdgconf is not null)
                Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", xdgconf);
            
            var result = ConfigUtils.GetInstanceProjectDirectory(projectDir, mockSystem);

            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", null);

            Assert.AreEqual(expectedResult, result);
        }

        private class MockSystemProvider : ISystemProvider
        {
            public char DirectorySeparatorChar
                => _platform.Equals(OSPlatform.Windows) ? '\\' : '/';
            
            private readonly string[] _dirs;
            private readonly string _home;
            private readonly OSPlatform _platform;
            private readonly Dictionary<string, string> _env;

            public MockSystemProvider(OSPlatform platform, string home, Dictionary<string, string> env, params string[] dirs)
            {
                _dirs = dirs;
                _home = home;
                _env = env;
                _platform = platform;
            }


            public string CombinePaths(params string[] paths)
                => string.Join(DirectorySeparatorChar, paths);
            public bool DirectoryExists(string dir)
                => _dirs.Any(x => x == dir || x.StartsWith(dir));
            public string GetFullPath(string path)
                => path;
            public string GetHomeDir()
                => _home;
            public bool IsOSPlatform(OSPlatform platform)
                => platform.Equals(_platform);

            public bool IsRooted(string path)
                => IsOSPlatform(OSPlatform.Windows)
                    ? Path.IsPathRooted(path)
                    : path.StartsWith("/");

            public string? GetEnvVariable(string name)
                => _env.TryGetValue(name, out var val)
                    ? val
                    : null;
        }
    }
}
