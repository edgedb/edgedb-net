.. _edgedb-dotnet-intro:

====================
EdgeDB Dotnet Client
====================

.. toctree:: 
   :maxdepth: 3
   :hidden:


Welcome to the Dotnet EdgeDB client! This is a community built client for EdgeDB.

.. _edgedb-dotnet-installation:

**Installation**

You can install the driver and all additional packages using NuGet:
 
.. code-block:: bash

   $ dotnet add package EdgeDB.Net

Or you can add each component seperatly

.. code-block:: bash

   $ dotnet add package EdgeDB.Net.Driver
   $ dotnet add package EdgeDB.Net.QueryBuilder

The Driver
==========
    
The Driver is the core package that allows you to connect, read and write to the database, and execute querries.
The driver only allows you to execute querries as strings. 

.. code-block:: c#

   using EdgeDB;

   var edgedb = new EdgeDBClient();
   var query = "select \"Hello World!\"";

   var result = await edgedb.QueryAsync(query);
   Console.WriteLine(result); // "Hello World!"


The QueryBuilder
================

The query builder allows you to build querries that resemble linq expressions, It also provides typing when building querries.


**Note**: The Query builder is an addon package for the driver, it does not replace the driver.

.. code-block:: c#

   using EdgeDB;

   var edgedb = new EdgeDBClient();
   var query = QueryBuilder.Select(() => "Hello World!");

   var result = await edgedb.QueryAsync(query.Build());
   Console.WriteLine(result); // "Hello World!";

