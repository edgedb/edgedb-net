.. _edgedb-dotnet-installing:

Installing
==========

EdgeDB DotNet is distributed through the `NuGet`_ package manager; the most recommended way to install 
it is to use the dotnet command line tool or NuGet package manager in Visual Studio.

**Dotnet cli**

.. code-block:: bash
  
  $ dotnet add package EdgeDB.Net.Driver


.. warning:: 
  
  EdgeDB.Net targets .net5 and .net6. Other .NET frameworks are not supported currently.


Building from source
--------------------

If you want to build the EdgeDB.Net driver from source, you will need the `.NET 6 SDK`_.
  
Once you have the SDK installed you can run the dotnet build command in the root directory of the project:

.. code:: bash

  $ dotnet build


Running tests
-------------

You can run the test suite by using the dotnet test command like so:

.. code:: bash

  $ dotnet test



.. _NuGet: https://www.nuget.org/packages/EdgeDB.Driver.DotNet/
.. _.NET 6 SDK: https://dotnet.microsoft.com/en-us/download