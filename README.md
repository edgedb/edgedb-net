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

## Installation

EdgeDB.Net is distributed through the NuGet package manager.
We recommend using the `dotnet` command or NuGet package manager in Visual
Studio:

```bash
$ dotnet add package EdgeDB.Net.Driver
```

## Basic usage

### Creating a client

Clients are what allow your code to talk and interface with EdgeDB. The
[`EdgeDBClient`](https://www.edgedb.com/docs/clients/dotnet/api#EdgeDB.EdgeDBClient)
class contains a pool of connections and numerous abstractions for executing
queries with ease:

```cs
using EdgeDB;

var client = new EdgeDBClient();
```

### Client configuration

`EdgeDBClient` will automatically determine how to connect to your EdgeDB
instance by resolving [EdgeDB Projects](https://www.edgedb.com/docs/intro/projects).
For specifying custom connection arguments, considering checking out the
[`EdgeDBConnection`](https://www.edgedb.com/docs/clients/dotnet/connection_parameters#EdgeDBConnection)
class. Here's an example of using the [`.Parse()`](https://www.edgedb.com/docs/clients/dotnet/connection_parameters#EdgeDBConnection.Parse-string?-string?-Action_EdgeDBConnection_?-bool)
method:

```cs
using EdgeDB;

var connection = EdgeDBConnection.Parse("edgedb://user:password@localhost:5656/mydb");
var client = new EdgeDBClient(connection);
```

### Executing queries

**Note**: EdgeDB.Net is a fully asynchronous driver, and as such, all I/O
operations are performed asynchronously.


Queries are executed through the `EdgeDBClient` by using different helper
methods. Your choice of method is dependent on the kind of query you're making,
better known as [cardinality](https://www.edgedb.com/docs/clients/dotnet/index#cardinality-and-return-types).

Query helper methods will expect a generic `T` type which is the [.NET version of an EdgeDB type](https://www.edgedb.com/docs/clients/dotnet/datatypes#datatypes):

```cs
var result = await client.QueryAsync<long>("select 2 + 2"); // returns 4
```

## Contributing

We openly welcome and accept contributions to EdgeDB.Net! Before writing a
GitHub Issue or Pull Request, please see our [contribution requirements](CONTRIBUTING.md).

## Examples

This repository contains a list of [working examples](examples),
check them out to see EdgeDB.Net in action!


## Compiling

If you're building EdgeDB.Net from source, you will need to download the
[.NET 6 SDK](https://dotnet.microsoft.com/en-us/download).
  
Once you have the SDK installed, you can then run `dotnet build` in the root
directory of the project:

```bash
$ dotnet build
```

## Testing

You can run the test suite by using `dotnet test` like so:

```bash
$ dotnet test
```
