.. _edgedb-dotnet-intro:

====================
EdgeDB Dotnet Client
====================

.. toctree:: 
  :maxdepth: 3
  :hidden:


EdgeDB.Net is the official EdgeDB .NET client, compatable with C#, F#, and VB.NET. 

.. note:: 
  EdgeDB version >= 2.0 is required to use EdgeDB.Net.

Installing
----------

EdgeDB.Net is distributed using NuGet. To install the latest version, run the following command in the Package Manager Console:

.. tabs::

  .. codetab:: bash#Stable (NuGet)

    dotnet add package EdgeDB.Net.Driver --version 0.3.4

  .. codetab:: bash#Nightlies (MyGet)

    dotnet add package EdgeDB.Net.Driver --source https://www.myget.org/F/edgedb-net/api/v3/index.json

This package is the base driver, allowing you to connect and execute queries against EdgeDB. There is currently no EFCore support.

Basic Usage
-----------
First, you will need to setup an EdgeDB project and create an instance. You can read more about that in the `Quickstart Guide<https://www.edgedb.com/docs/intro/quickstart>`__.


