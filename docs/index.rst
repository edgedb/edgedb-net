.. _edgedb-dotnet-intro:

====================
EdgeDB Dotnet Client
====================

.. toctree:: 
  :maxdepth: 3
  :hidden:

  customtypes
  datatypes

EdgeDB.Net is the official EdgeDB .NET client, compatable with C#, F# and
VB.NET.

.. attention::

  EdgeDB version 2.0 and above are required to use EdgeDB.Net.

.. _edgedb-dotnet-installing:

Installing
----------

EdgeDB.Net is distributed among two package managers, NuGet and MyGet; for
stable and unstable respectively. To install the latest version, run the
following command into your terminal:

.. tabs::

  .. code-tab:: bash#NuGet
    
    $ dotnet add package EdgeDB.Net.Driver
  
  .. code-tab:: bash#MyGet

    $ dotnet add package EdgeDB.Net.Driver --source https://www.myget.org/F/edgedb-net/api/v3/index.json

``EdgeDB.NET.Driver`` is the base driver for connections and query execution
in EdgeDB. At this time, there is no support for EFCore usage.

.. _edgedb-dotnet-basic-usage:

Basic Usage
-----------

First, you will need to setup an EdgeDB project and create an instance. You can
read more about that in the
`Quickstart guide <https://www.edgedb.com/docs/intro/quickstart>`_.

After you have an instance running, you can now create an ``EdgeDBClient``:

.. tabs::

  .. code-tab:: cs#CSharp

    using EdgeDB.Net;
  
    var client = new EdgeDBClient();

  .. code-tab:: fs#FSharp

    open EdgeDB.Net;
    
    let client = new EdgeDBClient();

An instance of ``EdgeDBClient`` will attempt to automatically resolve our
project's instance. In most circumstance, you won't need to specify any
connection argument parameters. However, if you do need to, you'll want to do
so by using the ``EdgeDBConnection.Parse()`` method and pass the result into
the client constructor given above.

Now with that done, you may now start performing queries:

.. tabs::

  .. code-tab:: cs#CSharp

    var result = await client.QuerySingleAsync<string>("SELECT \"Hello, World!\"");

    Console.WriteLine(result);
    
  .. code-tab:: fs#FSharp
  
    let! result = client.QuerySingleAsync<string>("SELECT \"Hello, World!\"")
    
    print result

.. note:: 

  For more information on how EdgeDB types are mapped to .NET types,
  refer to the document on :ref:`datatypes <edgedb-dotnet-datatypes>`.

.. _edgedb-dotnet-types-cardinality:

Return Types and Cardinality
----------------------------

The .NET driver treats cardinality exposure explicitly, meaning you'll need
to specify the cardinality you want in your query. In order to do this, the
driver provides three methods that change the result type:

+-------------+---------------------------------+-----------------------------+
| Cardinality | Method                          | Result                      |
+=============+=================================+=============================+
| Many        | ``QueryAsync<T>``               | ``IReadOnlyCollection<T?>`` |
+-------------+---------------------------------+-----------------------------+
| At Most One | ``QuerySingleAsync<T>``         | ``T?``                      |
+-------------+---------------------------------+-----------------------------+
| One         | ``QueryRequiredSingleAsync<T>`` | ``T``                       |
+-------------+---------------------------------+-----------------------------+

Each ``Query*`` method takes in a generic ``T`` representing the resulting type
of each query. To represent objects, you can use classes and structs to
reflect the names and values within each result. Classes can be used for a
better representation of query results.

.. note::

  For more information on how to use classes, refer to the documentation
  on :ref:`custom types <edgedb-dotnet-custom-types>`.

.. tabs:: 

  .. code-tab:: cs#CSharp

    public class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    var result = await client.QueryAsync<Person>("SELECT Person { Name, Age }");

  .. code-tab:: fs#FSharp

    type Person = {
      Name: string;
      Age: int;
    }

    let! result = client.QueryAsync<Person>("SELECT Person { Name, Age }")
