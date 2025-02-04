.. _gel-dotnet-config:

=============
Configuration
=============

There are two different types of configuration that can be made: clientside 
config, and session state. Clientside configuration is strictly related 
on how Gel.Net operates; while session state is configuration for
a particular connection with Gel.

Configuring the client
======================

Clientside configuration happens at client instansiation and cannot be 
mutated.

The :dn:class:`Gel.GelClientPoolConfig` is used to configurate the 
:dn:class:`Gel.GelClient` instance by passing it as a parameter 
to the clients constructor:

.. tabs::

  .. code-tab:: csharp

    var config = new GelClientPoolConfig()
    {
        ConnectionTimeout = 5000u
    };

    var client = new GelClient(config);

  .. code-tab:: fsharp

    let config = GelClientPoolConfig(
      ConnectionTimeout = 5000u
    )

    let client = GelClient(config)

+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+
| Property              | Type                     | Description                                                                                       |
+=======================+==========================+===================================================================================================+
| DefaultPoolSize       | ``int``                  | The default client pool size.                                                                     |
+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+
| Logger                | ``ILogger``              | The logger used for logging messages from the driver.                                             |
+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+
| RetryMode             | ``ConnectionRetryMode``  | The retry mode for connecting new clients.                                                        |
+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+
| MaxConnectionRetries  | ``uint``                 | The maximum number of times to retry to connect.                                                  |
+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+
| ConnectionTimeout     | ``uint``                 | The number of miliseconds a client will wait for a connection to be established with the server.  |
+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+
| MessageTimeout        | ``uint``                 | The max amount of miliseconds a client will wait for an expected message.                         |
+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+
| ExplicitObjectIds     | ``bool``                 | Whether or not to always return object ids.                                                       |
+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+
| ImplicitLimit         | ``ulong``                | The implicit object limit for all queries. By default there is not limit.                         |
+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+
| SchemaNamingStrategy  | ``INamingStrategy``      | The default naming strategy used within the schema.                                               |
+-----------------------+--------------------------+---------------------------------------------------------------------------------------------------+

Configuring state
=================

All state configuration methods begin with ``With`` and return 
a new client instance with the applied changes. The client instance
returned from a state change shares the same underlying connection 
pool as the client it was derived from.

See :dn:method:`Gel.GelClient.WithConfig(Config)`, 
:dn:method:`Gel.GelClient.WithGlobals(IDictionary<string,object>)`,
:dn:method:`Gel.GelClient.WithModule(string)`, and
:dn:method:`Gel.GelClient.WithAliases(IDictionary<string,string>)`