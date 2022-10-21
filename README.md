![EdgeDB.Net](https://raw.githubusercontent.com/quinchs/EdgeDB.Net/dev/branding/Banner.png)

<p align="center">
  <a href="https://www.nuget.org/packages/EdgeDB.Net.Driver/">
    <img src="https://img.shields.io/nuget/vpre/EdgeDB.Net.Driver.svg?maxAge=2592000?style=plastic" alt="NuGet">
  </a>
  <a href="https://github.com/edgedb/edgedb-net/actions/workflows/tests.yml">
    <img src="https://github.com/edgedb/edgedb-net/actions/workflows/tests.yml/badge.svg?branch=dev" alt="Tests">
  </a>
  <a href="https://discord.gg/tM4EpAaeSq">
    <img src="https://discord.com/api/guilds/841451783728529451/widget.png" alt="Discord">
  </a>
  
  <p align="center">
    EdgeDB.Net is the official .NET driver for the <a href="https://edgedb.com">EdgeDB</a> database.
  </p>
</p>

<h1 align="center">IMPORTANT read before using</h1>

EdgeDB.Net v1.0.0 is not yet released as its still in development, you can install the nightly version on our [myget](https://www.myget.org/feed/edgedb-net/package/nuget/EdgeDB.Net.Driver) feed. Please note bugs may be presents, if you encounter any issues please open a github issue.


## Installation

EdgeDB DotNet is distributed through the NuGet package manager; the most recommended way to install 
it is to use the dotnet command line tool or NuGet package manager in Visual Studio.

```bash
$ dotnet add package EdgeDB.Net.Driver
```

## Basic usage

> Full documentation is yet to be written.

### Create a client
A client is what allows your code to talk with EdgeDB. The `EdgeDBClient` class maintains a pool of connections and provides abstractions for executing queries with ease.
```cs
using EdgeDB;

var client = new EdgeDBClient();
```

### Configuring the client
The `EdgeDBClient` automatically determines how to connect to your edgedb database, using [EdgeDB Projects](https://www.edgedb.com/docs/intro/projects). If you want to specify custom connection arguments, you can use the static `EdgeDBConnection.Parse()` method.

```cs
using EdgeDB;

var connection = EdgeDBConnection.Parse("edgedb://user:password@localhost:5656/mydb");
var client = new EdgeDBClient(connection);
```

### Querying the database
```cs

var result = await client.QueryAsync<long>("select 2 + 2"); // 4
```

**Note**
The `QueryAsync` method always returns a `IReadOnlyCollection<T>`, regardless of actual query cardinality. If you want to explicitly define cardinality when querying, you can use the `QuerySingleAsync` or `QueryRequiredSingle` methods.


**Note**
EdgeDB.Net is a fully asynchronous driver, and as such, all IO operations are performed asynchronously.

## Examples
You can view our curated examples [here](https://github.com/quinchs/EdgeDB.Net/tree/dev/examples/EdgeDB.Examples.ExampleApp/Examples). We also have a demo asp.net project you can view [here](https://github.com/quinchs/EdgeDB.Net/tree/dev/examples/EdgeDB.Examples.ExampleTODOApi). You're more than welcome to contribute to the examples!

## Compiling
If you want to build the EdgeDB.Net project from source, you will need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download).
  
Once you have the SDK installed you can run the dotnet build command in the root directory of the project:

```bash
$ dotnet build
```

## Testing

You can run the test suite by using the dotnet test command like so:

```bash
$ dotnet test
```
