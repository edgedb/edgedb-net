.. _edgedb-dotnet-connection-parameters:

=====================
Connection Parameters
=====================

The ``EdgeDBClient`` constructor can accept an ``EdgeDBConnection`` class which
specifies how the client should connect to EdgeDB. The main way to to construct
a ``EdgeDBConnection`` is to use the static helper methods:

.. dn:class:: EdgeDBConnection
    :no_link:
    
    .. note::

        The ``TLSCertData`` property is obselete following version 1.0 and
        higher.

    Represents a client-facing connection to EdgeDB.

    :property string Username:
        The username used to connect to the database.
        Defaults to ``"edgedb"``.

    :property string? Password:
        The password to connect to the database.

    :property string Hostname:
        The hostname of the EdgeDB instance.
        Defaults to ``"127.0.0.1"``.

    :property int Port:
        The port of the EdgeDB instance to connect to.
        Defaults to ``5656``.

    :property string? Database:
        The database name to use when connecting.
        Defaults to ``"edgedb"``.

    :property TLSSecurityMode TLSSecurity:
        The TLS security level.
        Defaults to ``TLSSecurityMode.Strict``.

    .. dn:method:: FromDSN(string dsn): EdgeDBConnection

        Creates a :dn:class:`EdgeDBConnection` from an `EdgeDB DSN`_.

        :param string dsn: 
            The DSN to create the connection from.

        :returns:
           A :dn:class:`EdgeDBConnection` representing the DSN.

        :throws ArgumentException:
            A query parameter has already been defined in the DSN.

        :throws FormatException:
            Port was not in the correct format of int.

        :throws FileNotFoundException:
            A file parameter wasn't found.

        :throws KeyNotFoundException:
            An environment variable couldn't be found.
    
    .. dn:method:: FromProjectFile(string path): EdgeDBConnection

        Creates a :dn:class:`EdgeDBConnection` from a ``.toml`` project file.

        :param string path:
            The path to the ``.toml`` project file.
        
        :returns:
            A :dn:class:`EdgeDBConnection` representing the project defined in
            the ``.toml`` file.

        :throws FileNotFoundException:
            The supplied file path, credentials path, or instance-name file
            doesn't exist.

        :throws DirectoryNotFoundException:
            The project directory doesn't exist for the supplied toml file.

    .. dn:method:: FromInstanceName(string name): EdgeDBConnection

        Creates a :dn:class:`EdgeDBConnection` from an instance name.

        :param string name:
            The name of the instance.

        :returns:
            A :dn:class:`EdgeDBConnection` containing connection details for
            the specific instance.

        :throws FileNotFoundException:
            The instances config file couldn't be found.

    .. dn:method:: ResolveEdgeDBTOML(): EdgeDBConnection

        Resolves a connection by traversing the current working directory and
        its parents
        to find an ``edgedb.toml`` file.

        :returns:
            A resolved :dn:class:`EdgeDBConnection`.
            
        :throws FileNotFoundException:
            No ``edgedb.toml`` file could be found.

    .. dn:method:: Parse(string? instance = null, \
            string? dsn = null, \
            Action<EdgeDBConnection>? configure = null, \
            bool autoResolve = true \
        ): EdgeDBConnection

        Parses the provided arguments to build a :dn:class:`EdgeDBConnection`;
        parse logic follows the `Priority Levels`_ of arguments.

        :param string? instance:
            The instance name to connect to.

        :param string? dsn:
            The DSN string to use to connect.

        :param Action<EdgeDBConnection>? configure:
            A configuration delegate.

        :param bool autoResolve:
            Whether or not to autoresolve a connection using
            :dn:method:`EdgeDBConnection.ResolveEdgeDBTOML`.

        :returns:
            A :dn:class:`EdgeDBConnection` that can be used to connect to a
            EdgeDB instance.

        :throws ConfigurationException:
            An error occured while parsing or configuring the
            :dn:class:`EdgeDBConnection`.

        :throws FileNotFoundException:
            A configuration file could not be found.


.. _Priority Levels: https://www.edgedb.com/docs/reference/connection#ref-reference-connection-priority
.. _EdgeDB DSN: https://www.edgedb.com/docs/reference/dsn

