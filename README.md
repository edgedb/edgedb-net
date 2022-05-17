![EdgeDB.Net](./branding/Banner.png)

<p align="center">
  <a href="https://www.nuget.org/packages/EdgeDB.Net.Driver/">
    <img src="https://img.shields.io/nuget/vpre/EdgeDB.Net.Driver.svg?maxAge=2592000?style=plastic" alt="NuGet">
  </a>
  <a href="https://github.com/quinchs/EdgeDB.Net/actions/workflows/tests.yml">
    <img src="https://github.com/quinchs/EdgeDB.Net/actions/workflows/tests.yml/badge.svg?branch=master" alt="Tests">
  </a>
  <a href="https://discord.gg/tM4EpAaeSq">
    <img src="https://discord.com/api/guilds/841451783728529451/widget.png" alt="Discord">
  </a>
</p>


EdgeDB.Net is a community maintained .NET driver for the [EdgeDB](https://edgedb.com) database.

## Installation

EdgeDB DotNet is distributed through the NuGet package manager; the most recommended way to install 
it is to use the dotnet command line tool or NuGet package manager in Visual Studio.

```bash
$ dotnet add package EdgeDB.Net.Driver
```

## Examples
You can view our curated examples [here](https://github.com/quinchs/EdgeDB.Net/tree/master/examples/EdgeDB.ExampleApp/Examples). You're more than welcome to contribute to the examples!

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