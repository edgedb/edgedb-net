.. _edgedb-dotnet-configuring:

Configuring the client
======================

You can configure your edgedb client (:ref:`or client pool<edgedb-dotnet-pooling>`) 
with the ``EdgeDBConfig`` class like so:

.. code:: c#

  var config = new EdgeDBConfig()
  {
      ConnectionTimeout = 5000,
      MaxConnectionRetries = 5,
      Logger = yourCoolLogger,
      MessageTimeout = 5000,
      RetryMode = ConnectionRetryMode.AlwaysRetry
  }

  var client = new EdgeDBClient(config);

.. list-table:: 
  :widths: 25 25 50
  :header-rows: 1

  * - Property
    - Type
    - Description
  * - ConnectionTimeout
    - ``uint``
    - The amount of miliseconds to wait for a connection to be established.
  * - MaxConnectionRetries
    - ``uint``
    - The maximum number of attemts to make to reconnect if a connection is lost.
  * - Logger
    - ``ILogger``
    - The logger to use for logging.
  * - MessageTimeout
    - ``uint``
    - The amount of miliseconds to wait for a message to be received. 
  * - RetryMode
    -  ``ConnectionRetryMode``
    - The retry mode for when a connection can't be established 