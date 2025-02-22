![Gel.Net](https://raw.githubusercontent.com/geldata/gel-net/dev/branding/Banner.png)

<p align="center">
  <a href="https://www.nuget.org/packages/Gel.Net.Driver/">
    <img src="https://img.shields.io/nuget/dt/Gel.Net.Driver" alt="Downloads">
  </a>
  <a href="https://www.nuget.org/packages/Gel.Net.Driver/">
    <img src="https://img.shields.io/nuget/vpre/Gel.Net.Driver.svg?maxAge=2592000?style=plastic" alt="NuGet">
  </a>
  <a href="https://github.com/geldata/gel-net/actions/workflows/tests.yml">
    <img src="https://github.com/geldata/gel-net/actions/workflows/tests.yml/badge.svg?branch=dev" alt="Tests">
  </a>
  <a href="https://discord.gg/tM4EpAaeSq">
    <img src="https://discord.com/api/guilds/841451783728529451/widget.png" alt="Discord">
  </a>
  
  <p align="center">
    Gel.Net is the official .NET driver for the <a href="https://geldata.com">Gel</a> database.
  </p>
</p>

## Documentation

Documentation for the dotnet driver can be found [here](https://www.geldata.com/docs/clients/dotnet).

## Installation

Gel.Net is distributed through the NuGet package manager.
We recommend using the `dotnet` command or NuGet package manager in Visual
Studio:

```bash
$ dotnet add package Gel.Net.Driver
```

## Basic usage

### Creating a client

Clients are what allow your code to talk and interface with Gel. The
[`GelClientPool`](https://www.geldata.com/docs/clients/dotnet/api#Gel.GelClientPool)
class contains a pool of connections and numerous abstractions for executing
queries with ease:

```cs
using Gel;

var client = new GelClientPool();
```

### Client configuration

`GelClientPool` will automatically determine how to connect to your Gel
instance by resolving [Gel Projects](https://www.geldata.com/docs/intro/projects).
For specifying custom connection arguments, considering checking out the
[`GelConnection`](https://www.geldata.com/docs/clients/dotnet/connection_parameters#GelConnection)
class. Here's an example of using the [`.Create()`](https://www.geldata.com/docs/clients/dotnet/connection_parameters#GelConnection.Create-Options?)
method:

```cs
using Gel;

var connection = GelConnection.Create(
  new GelConnection.Options(){dsn="gel://user:password@localhost:5656/mybranch"}
);
var client = new GelClientPool(connection);
```

### Executing queries

**Note**: Gel.Net is a fully asynchronous driver, and as such, all I/O
operations are performed asynchronously.


Queries are executed through the `GelClientPool` by using different helper
methods. Your choice of method is dependent on the kind of query you're making,
better known as [cardinality](https://www.geldata.com/docs/clients/dotnet/index#cardinality-and-return-types).

Query helper methods will expect a generic `T` type which is the [.NET version of an Gel type](https://www.edgedb.com/docs/clients/dotnet/datatypes#datatypes):

```cs
var result = await client.QueryAsync<long>("select 2 + 2"); // returns 4
```

## Contributing

We openly welcome and accept contributions to Gel.Net! Before writing a
GitHub Issue or Pull Request, please see our [contribution requirements](CONTRIBUTING.md).

## Examples

This repository contains a list of [working examples](examples),
check them out to see Gel.Net in action!


## Compiling

If you're building Gel.Net from source, you will need to download the
[.NET 8 SDK](https://dotnet.microsoft.com/en-us/download).
  
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
