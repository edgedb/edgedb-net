.. _gel-dotnet-intro:

===========================
.NET client library for Gel
===========================

.. toctree:: 
  :maxdepth: 3
  :hidden:

  connection_parameters
  config
  customtypes
  datatypes
  exceptions
  api
  transactions

Gel.Net is the official Gel .NET client, compatable with C#, F# and
VB.NET.

.. note::

  Gel version 2.0 and above is required to use Gel.Net.

.. _gel-dotnet-installing:

Installing
----------

Gel.Net is distributed between two package managers: NuGet and MyGet;
for stable and unstable respectively. To install the latest version, run the
following command in your terminal:

.. note:: 

  At this time, there is no base support for `EFCore
  <https://github.com/dotnet/efcore>`_.

.. tabs::

  .. code-tab:: bash
    :caption: NuGet
    
    $ dotnet add package Gel.Net.Driver
  
  .. code-tab:: bash
    :caption: MyGet

    $ dotnet add package Gel.Net.Driver --source https://www.myget.org/F/gel-net/api/v3/index.json

.. _gel-dotnet-basic-usage:

Quickstart
----------

To start, you will need to setup an Gel project and have an instance
created. For more information regarding how to do this, we recommend going
through the `Quickstart guide <https://www.geldata.com/docs/intro/quickstart>`_.

After you have an instance running, you may now create an ``GelClientPool``:

.. tabs::

  .. code-tab:: cs

    using Gel.Net;
  
    var client = new GelClientPool();

  .. code-tab:: fsharp

    open Gel
    
    let client = GelClientPool()

``GelClientPool`` will automatically attempt to resolve your project's instance.
In most circumstances, you won't need to specify any connection parameters.
However, if you do need to, you'll want to do that by using
``GelConnection.Parse()`` and passing the result into the client's instance.

Executing queries
^^^^^^^^^^^^^^^^^

Executing a query is simple in the .NET driver. Let's make and execute a query
with the ``QuerySingleAsync<T>`` method and printing its result:

.. tabs::

  .. code-tab:: cs

    var result = await client.QuerySingleAsync<string>("SELECT \"Hello, World!\"");

    Console.WriteLine(result);
    
  .. code-tab:: fsharp
  
    let result = 
      client.QuerySingleAsync<string>("SELECT \"Hello, World!\"")
      |> Async.AwaitTask
      |> Async.RunSynchronously
    
    printfn $"{result}"

.. note:: 

  For more information on how Gel types are mapped to .NET types,
  refer to the documentation on :ref:`datatypes <gel-dotnet-datatypes>`.

.. _gel-dotnet-types-cardinality:

Cardinality and return types
----------------------------

Cardinality is exposed as different methods in the ``GelClientPool``. This means
you will need to specify which cardinality you want in your query by using
what's given in the table below:

+-------------+---------------------------------+-----------------------------+
| Cardinality | Method                          | Result                      |
+=============+=================================+=============================+
| Many        | ``QueryAsync<T>``               | ``IReadOnlyCollection<T?>`` |
+-------------+---------------------------------+-----------------------------+
| At Most One | ``QuerySingleAsync<T>``         | ``T?``                      |
+-------------+---------------------------------+-----------------------------+
| One         | ``QueryRequiredSingleAsync<T>`` | ``T``                       |
+-------------+---------------------------------+-----------------------------+

Each query method shown takes in ``T`` representing the return type.

For object representation, you can either use classes or structs to reflect the
names and values within each result.

.. tabs:: 

  .. code-tab:: cs

    public class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    var result = await client.QueryAsync<Person>("SELECT Person { Name, Age }");

  .. code-tab:: fsharp

    type Person = { Name: string; Age: int }

    let result = // Person list
      client.QueryAsync<Person>("SELECT Person { Name, Age }")
      |> Async.AwaitTask
      |> Async.RunSynchronously
      |> List.ofSeq

.. note::

  For more information on how to use classes, refer to the documentation
  on :ref:`custom types <gel-dotnet-custom-types>`.

Dependency Injection (DI)
-------------------------

Gel.Net supports `Dependency Injection`_ design patterns, allowing you to 
easily integrate Gel with your existing applications.

.. tabs::

  .. code-tab:: cs

    using Gel.Net;
    using Microsoft.Extensions.DependencyInjection;
    
    ...

    services.AddClientPool();

  .. code-tab:: fsharp

    open Gel.Net;
    open Microsoft.Extensions.DependencyInjection;
    
    ...

    services.AddClientPool();

You can specify both a ``GelConnection`` and a delegate for configuring 
the ``GelClientPoolConfig``, the client will be added as a singleton to your 
service collection.

.. note:: 

  Currently, there is no way to create a factory for clients, your service collection 
  may only contain **one** ``GelClientPool``.

.. _Dependency Injection: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
