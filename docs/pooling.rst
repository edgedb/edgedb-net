.. _edgedb-dotnet-pooling:

Client pooling
==============

TThe default ``EdgeDBClient`` handles client management for you so theres no need 
to worry about 2 clients trying to execute a query at the same time.

you can configure the pools default size and client factory with the ``EdgeDBClientPoolConfig`` class:

.. code:: c#

  var config = new EdgeDBClientPoolConfig()
  {
      ClientFactory = (clientId) => { ... },
      ClientType = EdgeDBClientType.Tcp,
      DefaultPoolSize = 100,
  };

  var client = new EdgeDBClient(config);

.. note:: All the :ref:`configurable options<edgedb-dotnet-configuring>` can be added to the ``EdgeDBClientPoolConfig`` as well.

.. list-table:: Properties
  :widths: 20 30 50
  :header-rows: 1

  * - Property
    - Type
    - Description
  * - ClientFactory
    - ``Func<ulong, EdgeDBConnection, EdgeDBConfig, ValueTask<BaseEdgeDBClient>>`` 
    - Gets or sets the client factory to use when adding new clients to the client pool.
  * - ClientType
    - ``EdgeDBClientType``
    - Gets or sets the client type the pool will use. In order to use a custom client factory, this property must be set to ``EdgeDBClientType.Custom``.
  * - DefaultPoolSize
    - ``int``
    - Gets or sets the default pool size. The internal pool size may change based off of the servers recommended pool size.

Executing commands in a pool
----------------------------
By default, when you call a method like ``ExecuteAsync`` or ``QueryAsync``, the pool will first get or 
create a client within the pool, hold that client and execute your command, and then release it back to the pool.
You don't need to manage the pool yourself, the pool will manage itself for you.


Pulling clients out of the pool
-------------------------------

If your code requires a single client instead of a pooled one, you can pull clients out of the client pool with the ``GetOrCreateClientAsync()`` function:

.. code:: c#

  var client = new EdgeDBClient();

  await using (var singleClient = await client.GetOrCreateClientAsync())
  {
      // ...
  }

.. note:: If the client pool is full and all clients within it are in use, the ``GetOrCreateClientAsync`` function will block until a client becomes available.

