.. _edgedb-dotnet-connection-parameters:

=====================
Connection Parameters
=====================

The ``EdgeDBClient`` constructor can accept an ``EdgeDBConnection`` class which
specifies how the client should connect to EdgeDB. The main way to to construct
a ``EdgeDBConnection`` is to use the static helper methods:


**FromDSN**

The ``EdgeDBConnection.FromDSN(string dsn)`` method will convert a 
`EdgeDB DSN`_ into a ``EdgeDBConnection`` instance.

.. _EdgeDB DSN: https://www.edgedb.com/docs/reference/dsn

**FromProjectFile**

The ``EdgeDBConnection.FromProjectFile(string path)`` will resolve an instance from an ``edgedb.toml`` file.

**FromInstanceName**

