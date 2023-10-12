.. _edgedb-dotnet-intro:

==============================
.NET client library for EdgeDB
==============================

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

EdgeDB.Net is the official EdgeDB .NET client, compatable with C#, F# and
VB.NET.

.. note::

  EdgeDB version 2.0 and above is required to use EdgeDB.Net.

.. _edgedb-dotnet-installing:

Installing
----------

EdgeDB.Net is distributed between two package managers: NuGet and MyGet;
for stable and unstable respectively. To install the latest version, run the
following command in your terminal:

.. note:: 

  At this time, there is no base support for `EFCore
  <https://github.com/dotnet/efcore>`_.

.. tabs::

  .. code-tab:: bash
    :caption: NuGet
    
    $ dotnet add package EdgeDB.Net.Driver
  
  .. code-tab:: bash
    :caption: MyGet

    $ dotnet add package EdgeDB.Net.Driver --source https://www.myget.org/F/edgedb-net/api/v3/index.json

.. _edgedb-dotnet-basic-usage:

Quickstart
----------

To start, you will need to setup an EdgeDB project and have an instance
created. For more information regarding how to do this, we recommend going
through the `Quickstart guide <https://www.edgedb.com/docs/intro/quickstart>`_.

After you have an instance running, you may now create an ``EdgeDBClient``:

.. tabs::

  .. code-tab:: cs

    using EdgeDB.Net;
  
    var client = new EdgeDBClient();

  .. code-tab:: fsharp

    open EdgeDB
    
    let client = EdgeDBClient()

``EdgeDBClient`` will automatically attempt to resolve your project's instance.
In most circumstances, you won't need to specify any connection parameters.
However, if you do need to, you'll want to do that by using
``EdgeDBConnection.Parse()`` and passing the result into the client's instance.

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

  For more information on how EdgeDB types are mapped to .NET types,
  refer to the documentation on :ref:`datatypes <edgedb-dotnet-datatypes>`.

.. _edgedb-dotnet-types-cardinality:

Cardinality and return types
----------------------------

Cardinality is exposed as different methods in the ``EdgeDBClient``. This means
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
  on :ref:`custom types <edgedb-dotnet-custom-types>`.

Dependency Injection (DI)
-------------------------

EdgeDB.Net supports `Dependency Injection`_ design patterns, allowing you to 
easily integrate EdgeDB with your existing applications.

.. tabs::

  .. code-tab:: cs

    using EdgeDB.Net;
    using Microsoft.Extensions.DependencyInjection;
    
    ...

    services.AddEdgeDB();

  .. code-tab:: fsharp

    open EdgeDB.Net;
    open Microsoft.Extensions.DependencyInjection;
    
    ...

    services.AddEdgeDB();

You can specify both a ``EdgeDBConnection`` and a delegate for configuring 
the ``EdgeDBClientConfig``, the client will be added as a singleton to your 
service collection.

.. note:: 

  Currently, there is no way to create a factory for clients, your service collection 
  may only contain **one** ``EdgeDBClient``.

.. _Dependency Injection: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
