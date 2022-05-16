.. _edgedb-dotnet-connecting:

Connecting to a EdgeDB Instance
===============================

The dotnet driver supports both DSN-style connection strings and project files. Most of the 
time you will not need to worry about specifying any connection paramters as the client will 
auto resolve them based on project linking.

.. code:: c#

  // the driver will look for a 'edgedb.toml file'
  var client = new EdgeDBClient();


**Specifying custom connection parameters**

.. code:: c#

  var connection = new EdgeDBConnection()
  {
      Database = "edgedb",
      Hostname = "localhost",
      Password = "password",
      Port = 1234,
      Username = "username",
      TLSSecurity = TLSSecurityMode.Strict,
      TLSCertificateAuthority = "..."
  };

  var client = new EdgeDBClient(connection);

**Using a DSN**

.. code:: c#

  var connection = EdgeDBConnection.FromDSN("edgedb://username:password@localhost:1234/edgedb");

  var client = new EdgeDBClient(connection);

**Using an instance name**

.. code:: c#

  var connection = EdgeDBConnection.FromInstanceName("edgedb");

  var client = new EdgeDBClient(connection);

  