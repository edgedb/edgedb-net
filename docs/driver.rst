.. _edgedb-dotnet-examples:

Driver
======

The Driver is the core connection point between your code and the database, its responsible for serializing, 
deserializing, error handling, and pooling.

Creating a Client
-----------------

A client represents a connection to a database. The client itself is a connection pool managing connections to the 
database.

Creating a client:
.. code-block:: cs

  using EdgeDB;

  var client = new EdgeDBClient();


