.. _edgedb-dotnet-api:

=================
API Documentation
=================

**Namespaces**

- :dn:namespace:`EdgeDB`
- :dn:namespace:`EdgeDB.TypeConverters`
- :dn:namespace:`EdgeDB.State`
- :dn:namespace:`EdgeDB.DataTypes`

.. dn:namespace:: EdgeDB

    .. dn:enum:: Capabilities

        Represents a bitfield of capabilities used when executing queries. 


    .. dn:enum:: Cardinality

        A enum containing the cardinality specification of a command. 


    .. dn:enum:: ConnectionRetryMode

        An enum representing the retry mode when connecting new clients. 


    .. dn:class:: EdgeDBClient

        Represents a client pool used to interact with EdgeDB. 


        :property int ConnectedClients:
            Gets the total number of clients within the client pool that are connected. 


        :property int AvailableClients:
            Gets the number of available (idle) clients within the client pool. 

            .. note::

                This property can equal ``EdgeDB.EdgeDBClient.ConnectedClients`` if the client type doesn't have restrictions on idling. 


        :property Config Config:
            The :dn:class:`EdgeDB.State.Config` containing session-level configuration. 


        :property string Module:
            The default module for this client. 


        :property IReadOnlyDictionary<string, string> Aliases:
            The module aliases for this client. 


        :property IReadOnlyDictionary<string, object> Globals:
            The globals for this client. 


        :property IReadOnlyDictionary<string, object> ServerConfig:
            Gets the EdgeDB server config. 

            .. note::

                The returned dictionary can be empty if the client pool hasn't connected any clients or the clients don't support getting a server config. 


        .. dn:method:: EdgeDBClient(): EdgeDBClient

            Creates a new instance of a EdgeDB client pool allowing you to execute commands. 

            .. note::

                This constructor uses the default config and will attempt to find your EdgeDB project toml file in the current working directory. If no file is found this method will throw a ``System.IO.FileNotFoundException``. 

        .. dn:method:: EdgeDBClient(EdgeDBClientPoolConfig clientPoolConfig): EdgeDBClient

            Creates a new instance of a EdgeDB client pool allowing you to execute commands. 

            .. note::

                This constructor will attempt to find your EdgeDB project toml file in the current working directory. If no file is found this method will throw a ``System.IO.FileNotFoundException``. 

            :param EdgeDBClientPoolConfig clientPoolConfig:
                The config for this client pool.

        .. dn:method:: EdgeDBClient(EdgeDBConnection connection): EdgeDBClient

            Creates a new instance of a EdgeDB client pool allowing you to execute commands. 

            :param EdgeDBConnection connection:
                The connection parameters used to create new clients.

        .. dn:method:: EdgeDBClient(EdgeDBConnection connection, EdgeDBClientPoolConfig clientPoolConfig): EdgeDBClient

            Creates a new instance of a EdgeDB client pool allowing you to execute commands. 

            :param EdgeDBConnection connection:
                The connection parameters used to create new clients.

            :param EdgeDBClientPoolConfig clientPoolConfig:
                The config for this client pool.

        .. dn:method::  ExecuteAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task

            Executes a given query without reading the returning result. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous execute operation. 

        .. dn:method::  QueryAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<IReadOnlyCollection<TResult>>

            Executes a given query and returns the result as a collection. 

            .. note::

                Cardinality isn't enforced nor takes effect on the return result, the client will always construct a collection out of the data. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QuerySingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result or ``null``. 

            .. note::

                This method enforces ``EdgeDB.Cardinality.AtMostOne``, if your query returns more than one result a :dn:class:`EdgeDB.EdgeDBException` will be thrown. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QueryRequiredSingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result. 

            .. note::

                This method enforces ``EdgeDB.Cardinality.One``, if your query returns zero or more than one result a :dn:class:`EdgeDB.EdgeDBException` will be thrown. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QueryJsonAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<Json>

            Executes a given query and returns the result as a single json string. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Optional collection of arguments within the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The tasks result is the json result of the query. 

            :throws EdgeDB.ResultCardinalityMismatchException:
                The query returned more than 1 datapoint.

        .. dn:method::  QueryJsonElementsAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<IReadOnlyCollection<Json>>

            Executes a given query and returns the result as a read-only collection of json objects. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Optional collection of arguments within the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The tasks result is the json result of the query. 

        .. dn:method::  EnsureConnectedAsync(CancellationToken token): ValueTask

            Ensures that a connection is established to the EdgeDB server; and that the client pool is configured to the servers recommended pool size. 

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A ``System.Threading.Tasks.ValueTask`` representing the asynchronous connection operation. 

        .. dn:method::  WithConfig(Action<ConfigProperties> configDelegate): EdgeDBClient

            Creates a new client with the specified ``EdgeDB.EdgeDBClient.Config``. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one. 

            :param Action<ConfigProperties> configDelegate:
                A delegate used to modify the config.

            :returns:

                A new client with the specified config. 

        .. dn:method::  WithConfig(Config config): EdgeDBClient

            Creates a new client with the specified ``EdgeDB.EdgeDBClient.Config``. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one. 

            :param Config config:
                The config for the new client.

            :returns:

                A new client with the specified config. 

        .. dn:method::  WithGlobals(IDictionary<string,object> globals): EdgeDBClient

            Creates a new client with the specified `Globals <https://www.edgedb.com/docs/datamodel/globals#globals>`_. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one.
                The newly created client doesn't copy any of the parents globals, this method is settative to the ``EdgeDB.EdgeDBClient.Globals`` property. 

            :param IDictionary<string, object> globals:
                The globals for the newly create client.

            :returns:

                A new client with the specified globals. 

        .. dn:method::  WithModule(string module): EdgeDBClient

            Creates a new client with the specified ``EdgeDB.EdgeDBClient.Module``. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one. 

            :param string module:
                The module for the new client.

            :returns:

                A new client with the specified module. 

        .. dn:method::  WithAliases(IDictionary<string,string> aliases): EdgeDBClient

            Creates a new client with the specified ``EdgeDB.EdgeDBClient.Aliases``. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one.
                The newly created client doesn't copy any of the parents aliases, this method is settative to the ``EdgeDB.EdgeDBClient.Aliases`` property. 

            :param IDictionary<string, string> aliases:
                The module aliases for the new client.

            :returns:

                A new client with the specified module aliases. 

    .. dn:class:: EdgeDBClientExtensions

        A class containing extension methods for edgedb clients. 


        .. dn:method::  TransactionAsync(this ITransactibleClient client, Func<Transaction,Task> func): Task

            Creates a transaction and executes a callback with the transaction object. 

            :param ITransactibleClient client:
                The TCP client to preform the transaction with.

            :param Func<Transaction, Task> func:
                The callback to pass the transaction into.

            :returns:

                A task that proxies the passed in callbacks awaiter.

        .. dn:method::  TransactionAsync<TResult>(this ITransactibleClient client, Func<Transaction,Task<TResult>> func): Task<TResult>

            Creates a transaction and executes a callback with the transaction object. 

            :param ITransactibleClient client:
                The TCP client to preform the transaction with.

            :param Func<Transaction, Task<TResult>> func:
                The callback to pass the transaction into.

            :param TResult:
                The return result of the task.

            :returns:

                A task that proxies the passed in callbacks awaiter.

        .. dn:method::  TransactionAsync(this ITransactibleClient client, TransactionSettings settings, Func<Transaction,Task> func): Task

            Creates a transaction and executes a callback with the transaction object. 

            :param ITransactibleClient client:
                The TCP client to preform the transaction with.

            :param TransactionSettings settings:
                The transactions settings.

            :param Func<Transaction, Task> func:
                The callback to pass the transaction into.

            :returns:

                A task that proxies the passed in callbacks awaiter.

        .. dn:method::  TransactionAsync<TResult>(this ITransactibleClient client, TransactionSettings settings, Func<Transaction,Task<TResult>> func): Task<TResult>

            Creates a transaction and executes a callback with the transaction object. 

            :param ITransactibleClient client:
                The TCP client to preform the transaction with.

            :param TransactionSettings settings:
                The transactions settings.

            :param Func<Transaction, Task<TResult>> func:
                The callback to pass the transaction into.

            :param TResult:
                The return result of the task.

            :returns:

                A task that proxies the passed in callbacks awaiter.

        .. dn:method::  DumpDatabaseAsync(this EdgeDBClient pool, CancellationToken token): Task<Stream>

            Dumps the current database to a stream. 

            :param EdgeDBClient pool:
                The client to preform the dump with.

            :param CancellationToken token:
                A token to cancel the operation with.

            :returns:

                A stream containing the entire dumped database.

            :throws EdgeDB.EdgeDBErrorException:
                The server sent an error message during the dumping process.

            :throws EdgeDB.EdgeDBException:
                The server sent a mismatched packet.

        .. dn:method::  RestoreDatabaseAsync(this EdgeDBClient pool, Stream stream, CancellationToken token): Task<string>

            Restores the database based on a database dump stream. 

            :param EdgeDBClient pool:
                The TCP client to preform the restore with.

            :param Stream stream:
                The stream containing the database dump.

            :param CancellationToken token:
                A token to cancel the operation with.

            :returns:

                The status result of the restore.

            :throws EdgeDB.EdgeDBException:
                The server sent an invalid packet or the restore operation couldn't proceed due to the database not being empty. 

            :throws EdgeDB.EdgeDBErrorException:
                The server sent an error during the restore operation.

    .. dn:class:: EdgeDBClientPoolConfig

        Represents a config for a :dn:class:`EdgeDB.EdgeDBClient`, extending :dn:class:`EdgeDB.EdgeDBConfig`. 


        :property int DefaultPoolSize:
            Gets or sets the default client pool size. 


    .. dn:class:: EdgeDBConfig

        Represents the configuration options for a :dn:class:`EdgeDB.EdgeDBClient` or ``T:EdgeDB.EdgeDBTcpClient``


        :property ILogger Logger:
            Gets or sets the logger used for logging messages from the driver. 


        :property ConnectionRetryMode RetryMode:
            Gets or sets the retry mode for connecting new clients. 


        :property uint MaxConnectionRetries:
            Gets or sets the maximum number of times to retry to connect. 


        :property uint ConnectionTimeout:
            Gets or sets the number of miliseconds a client will wait for a connection to be established with the server. 


        :property uint MessageTimeout:
            Gets or sets the max amount of miliseconds a client will wait for an expected message. 


        :property bool ExplicitObjectIds:
            Gets or sets whether or not to always return object ids. 

            .. note::

                If set to ``true`` returned objects will not have an implicit id property i.e. query shapes will have to explicitly list id properties. 


        :property ulong ImplicitLimit:
            Gets or sets the implicit object limit for all queries. By default there is not limit. 


        :property INamingStrategy SchemaNamingStrategy:
            Gets or sets the default naming strategy used within the schema. 

            .. note::

                By default, the naming convention will not modify property names. 


    .. dn:class:: EdgeDBConnection

        Represents a class containing information on how to connect to a edgedb instance. 


        :property string Username:
            Gets or sets the username used to connect to the database. 

            .. note::

                This property defaults to edgedb 


        :property string Password:
            Gets or sets the password to connect to the database. 


        :property string Hostname:
            Gets or sets the hostname of the edgedb instance to connect to. 

            .. note::

                This property defaults to 127.0.0.1. 


        :property int Port:
            Gets or sets the port of the edgedb instance to connect to. 

            .. note::

                This property defaults to 5656 


        :property string Database:
            Gets or sets the database name to use when connecting. 

            .. note::

                This property defaults to edgedb 


        :property string TLSCertData:
            Gets or sets the TLS certificate data used to very the certificate when authenticating. 

            .. note::

                This value is a legacy value pre 1.0 and should not be set explicity, use ``EdgeDB.EdgeDBConnection.TLSCertificateAuthority`` instead. 


        :property string TLSCertificateAuthority:
            Gets or sets the TLS Certificate Authority. 


        :property TLSSecurityMode TLSSecurity:
            Gets or sets the TLS security level. 

            .. note::

                The default value is ``EdgeDB.TLSSecurityMode.Strict``. 


        .. dn:method::  FromDSN(string dsn): EdgeDBConnection

            Creates an :dn:class:`EdgeDB.EdgeDBConnection` from a `valid DSN <https://www.edgedb.com/docs/reference/dsn#dsn-specification>`_. 

            :param string dsn:
                The DSN to create the connection from.

            :returns:

                A :dn:class:`EdgeDB.EdgeDBConnection` representing the DSN.

            :throws System.ArgumentException:
                A query parameter has already been defined in the DSN.

            :throws System.FormatException:
                Port was not in the correct format of int.

            :throws System.IO.FileNotFoundException:
                A file parameter wasn't found.

            :throws System.Collections.Generic.KeyNotFoundException:
                An environment variable couldn't be found.

        .. dn:method::  FromProjectFile(string path): EdgeDBConnection

            Creates a new EdgeDBConnection from a .toml project file. 

            :param string path:
                The path to the .toml project file.

            :returns:

                A :dn:class:`EdgeDB.EdgeDBConnection` representing the project defined in the .toml file.

            :throws System.IO.FileNotFoundException:
                The supplied file path, credentials path, or instance-name file doesn't exist.

            :throws System.IO.DirectoryNotFoundException:
                The project directory doesn't exist for the supplied toml file.

        .. dn:method::  FromInstanceName(string name): EdgeDBConnection

            Creates a new :dn:class:`EdgeDB.EdgeDBConnection` from an instance name. 

            :param string name:
                The name of the instance.

            :returns:

                A :dn:class:`EdgeDB.EdgeDBConnection` containing connection details for the specific instance.

            :throws System.IO.FileNotFoundException:
                The instances config file couldn't be found.

        .. dn:method::  ResolveEdgeDBTOML(): EdgeDBConnection

            Resolves a connection by traversing the current working directory and its parents to find an 'edgedb.toml' file. 

            :returns:

                A resolved :dn:class:`EdgeDB.EdgeDBConnection`.

            :throws System.IO.FileNotFoundException:
                No 'edgedb.toml' file could be found.

        .. dn:method::  Parse(string instance, string dsn, Action<EdgeDBConnection> configure, bool autoResolve): EdgeDBConnection

            Parses the provided arguments to build an :dn:class:`EdgeDB.EdgeDBConnection` class; Parse logic follows the `Priority levels <https://www.edgedb.com/docs/reference/connection#ref-reference-connection-priority>`_ of arguments. 

            :param string instance:
                The instance name to connect to.

            :param string dsn:
                The DSN string to use to connect.

            :param Action<EdgeDBConnection> configure:
                A configuration delegate.

            :param bool autoResolve:
                Whether or not to autoresolve a connection using :dn:method:`EdgeDB.EdgeDBConnection.ResolveEdgeDBTOML`.

            :returns:

                A :dn:class:`EdgeDB.EdgeDBConnection` class that can be used to connect to a EdgeDB instance. 

            :throws EdgeDB.ConfigurationException:
                An error occured while parsing or configuring the :dn:class:`EdgeDB.EdgeDBConnection`. 

            :throws System.IO.FileNotFoundException:
                A configuration file could not be found.

        .. dn:method::  ToString(): string

    .. dn:class:: EdgeDBDeserializerAttribute

        Marks the current method as the method to use to deserialize the current type. 


    .. dn:class:: EdgeDBHostingExtensions

        A class containing extension methods for DI. 


        .. dn:method::  AddEdgeDB(this IServiceCollection collection, EdgeDBConnection connection, Action<EdgeDBClientPoolConfig> clientConfig): IServiceCollection

            Adds a :dn:class:`EdgeDB.EdgeDBClient` singleton to a ``Microsoft.Extensions.DependencyInjection.IServiceCollection``. 

            :param IServiceCollection collection:
                The source collection to add a :dn:class:`EdgeDB.EdgeDBClient` to.

            :param EdgeDBConnection connection:
                An optional connection arguments for the client.

            :param Action<EdgeDBClientPoolConfig> clientConfig:
                An optional configuration delegate for configuring the :dn:class:`EdgeDB.EdgeDBClient`. 

            :returns:

                The source ``Microsoft.Extensions.DependencyInjection.IServiceCollection`` with :dn:class:`EdgeDB.EdgeDBClient` added as a singleton. 

    .. dn:class:: EdgeDBIgnoreAttribute

        Marks the current target to be ignored when deserializing or building queries. 


    .. dn:class:: EdgeDBPropertyAttribute

        Marks the current field or property as a valid target for serializing/deserializing. 


        .. dn:method:: EdgeDBPropertyAttribute(string propertyName): EdgeDBPropertyAttribute

            Marks this member to be used when serializing/deserializing. 

            :param string propertyName:
                The name of the member in the edgedb schema.

    .. dn:class:: EdgeDBTypeAttribute

        Marks this class or struct as a valid type to use when serializing/deserializing. 


        .. dn:method:: EdgeDBTypeAttribute(string name): EdgeDBTypeAttribute

            Marks this as a valid target to use when serializing/deserializing. 

            :param string name:
                The name of the type in the edgedb schema.

        .. dn:method:: EdgeDBTypeAttribute(): EdgeDBTypeAttribute

            Marks this as a valid target to use when serializing/deserializing. 

    .. dn:class:: EdgeDBTypeConverterAttribute

        Marks the current property to be deserialized/serialized with a specific :dn:class:`EdgeDB.TypeConverters.EdgeDBTypeConverter<TSource, TTarget>`. 


        .. dn:method:: EdgeDBTypeConverterAttribute(Type converterType): EdgeDBTypeConverterAttribute

            Initializes the :dn:class:`EdgeDB.EdgeDBTypeConverterAttribute` with the specified :dn:class:`EdgeDB.TypeConverters.EdgeDBTypeConverter<TSource, TTarget>`. 

            :param Type converterType:
                The type of the converter.

            :throws System.ArgumentException:
                is not a valid ``EdgeDB.DocGenerator.docMemberSummaryParamref``. :dn:class:`EdgeDB.TypeConverters.EdgeDBTypeConverter<TSource, TTarget>`

    .. dn:enum:: ErrorSeverity

        An enum representing the error severity of a ``T:EdgeDB.Binary.Packets.ErrorResponse``. 


    .. dn:struct:: ExecuteResult

        Represents a generic execution result of a command. 


        :property bool IsSuccess:

        :property Exception Exception:

        :property string ExecutedQuery:

    .. dn:class:: Group<TKey, TElement>

        Represents a group result returned from the ``GROUP`` expression. 

        :param TKey:
            The type of the key used to group the elements.

        :param TElement:
            The type of the elements.


        :property TKey Key:
            Gets the key used to group the set of ``EdgeDB.Group`2.Elements``. 


        :property IReadOnlyCollection<string> Grouping:
            Gets the name of the property that was grouped by. 


        :property IReadOnlyCollection<TElement> Elements:
            Gets a collection of elements that have the same key as ``EdgeDB.Group`2.Key``. 


        .. dn:method:: Group<TKey,TElement>(TKey key, IEnumerable<string> groupedBy, IEnumerable<TElement> elements): Group<TKey,TElement>

            Constructs a new grouping. 

            :param TKey key:
                The key that each element share.

            :param IEnumerable<string> groupedBy:
                The property used to group the elements.

            :param IEnumerable<TElement> elements:
                The collection of elements that have the specified key.

        .. dn:method::  GetEnumerator(): IEnumerator<TElement>

    .. dn:interface:: IEdgeDBQueryable

        Represents a object that can be used to query a EdgeDB instance. 


        .. dn:method::  ExecuteAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task

            Executes a given query without reading the returning result. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous execute operation. 

        .. dn:method::  QueryAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<IReadOnlyCollection<object>>

            Executes a given query and returns the result as a collection. 

            .. note::

                Cardinality isn't enforced nor takes effect on the return result, the client will always construct a collection out of the data. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QueryAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<IReadOnlyCollection<TResult>>

            Executes a given query and returns the result as a collection. 

            .. note::

                Cardinality isn't enforced nor takes effect on the return result, the client will always construct a collection out of the data. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :param TResult:
                The type of the return result of the query.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QuerySingleAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<object>

            Executes a given query and returns a single result or ``null``. 

            .. note::

                This method enforces ``EdgeDB.Cardinality.AtMostOne``, if your query returns more than one result a :dn:class:`EdgeDB.EdgeDBException` will be thrown. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QuerySingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result or ``null``. 

            .. note::

                This method enforces ``EdgeDB.Cardinality.AtMostOne``, if your query returns more than one result a :dn:class:`EdgeDB.EdgeDBException` will be thrown. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :param TResult:
                The return type of the query.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QueryRequiredSingleAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<object>

            Executes a given query and returns a single result. 

            .. note::

                This method enforces ``EdgeDB.Cardinality.One``, if your query returns zero or more than one result a :dn:class:`EdgeDB.EdgeDBException` will be thrown. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QueryRequiredSingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result. 

            .. note::

                This method enforces ``EdgeDB.Cardinality.One``, if your query returns zero or more than one result a :dn:class:`EdgeDB.EdgeDBException` will be thrown. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :param TResult:
                The return type of the query.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QueryJsonAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<Json>

            Executes a given query and returns the result as a single json string. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Optional collection of arguments within the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The tasks result is the json result of the query. 

            :throws EdgeDB.ResultCardinalityMismatchException:
                The query returned more than 1 datapoint.

        .. dn:method::  QueryJsonElementsAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<IReadOnlyCollection<Json>>

            Executes a given query and returns the result as a read-only collection of json objects. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Optional collection of arguments within the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The tasks result is the json result of the query. 

    .. dn:interface:: IExecuteError

        Represents a generic execution error. 


        :property string Message:
            Gets the error message. 


        :property ServerErrorCodes ErrorCode:
            Gets the error code. 


    .. dn:interface:: IExecuteResult

        An interface representing a generic execution result. 


        :property bool IsSuccess:
            Gets whether or not the command executed successfully. 


        :property IExecuteError ExecutionError:
            Gets the error (if any) that the command received. 


        :property Exception Exception:
            Gets the exception (if any) that the command threw when executing. 


        :property string ExecutedQuery:
            Gets the executed query string. 


    .. dn:interface:: INamingStrategy

        Represents an abstract naming strategy used to convert property names within a dotnet type to a name within a schema file. 


        :property INamingStrategy DefaultNamingStrategy:
            Gets the default naming strategy. This strategy does not modify property names. 


        :property INamingStrategy AttributeNamingStrategy:
            Gets the attribute-based naming strategy. 


        :property INamingStrategy CamelCaseNamingStrategy:
            Gets the 'camelCase' naming strategy. 


        :property INamingStrategy PascalNamingStrategy:
            Gets the 'PascalCase' naming strategy. 


        :property INamingStrategy SnakeCaseNamingStrategy:
            Gets the 'snake-case' naming strategy. 

            .. note::

                This is the default naming strategy for the :dn:class:`EdgeDB.TypeBuilder`. 


        .. dn:method::  Convert(PropertyInfo property): string

            Converts the ``EdgeDB.DocGenerator.docMemberSummaryParamref``'s name to the desired naming scheme. 

            :param PropertyInfo property:
                The property info of which to convert its name.

            :returns:

                The name defined in the schema.

        .. dn:method::  Convert(string name): string

            Converts the name to the desired naming scheme. 

            :param string name:
                The property name of which to convert its name.

            :returns:

                The name defined in the schema.

    .. dn:enum:: IOFormat

        An enum representing the format of a commands result. 


    .. dn:enum:: Isolation

        An enum representing the transaction mode within a :dn:class:`EdgeDB.Transaction`. 


    .. dn:interface:: ITransactibleClient

        Represents a client that supports transactions. 


        :property TransactionState TransactionState:
            Gets the transaction state of the client. 


        .. dn:method::  StartTransactionAsync(Isolation isolation, bool readOnly, bool deferrable, CancellationToken token): Task

            Starts a transaction. 

            :param Isolation isolation:
                The isolation mode of the transaction.

            :param bool readOnly:
                Whether or not the transaction is in read-only mode.

            :param bool deferrable:
                Whether or not the trasaction is deferrable.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A Task that represents the asynchronous operation of starting a transaction. 

        .. dn:method::  CommitAsync(CancellationToken token): Task

            Commits the transaction to the database. 

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A Task that represents the asynchronous operation of commiting a transaction. 

        .. dn:method::  RollbackAsync(CancellationToken token): Task

            Rolls back all commands preformed within the transaction. 

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A Task that represents the asynchronous operation of rolling back a transaction. 

    .. dn:struct:: ObjectEnumerator

        Represents an enumerator for creating objects. 


        .. dn:method::  ToDynamic(): object

            Converts this :dn:struct:`EdgeDB.ObjectEnumerator` to a ``dynamic`` object. 

            :returns:

                A ``dynamic`` object.

        .. dn:method::  Flatten(): IDictionary<string,object>

            Flattens this :dn:struct:`EdgeDB.ObjectEnumerator` into a dictionary with keys being property names. 

            :returns:

                A ``System.Collections.Generic.Dictionary`2`` representing the objects properties.

        .. dn:method::  Next(ref String& name, ref Object& value): bool

            Reads the next property within this enumerator. 

            :param String& name:
                The name of the property.

            :param Object& value:
                The value of the property.

            :returns:

                if a property was read successfully; otherwise ``true``. ``false``

    .. dn:class:: Optional

        Represents an optional value. 


        .. dn:method::  Create<T>(): Optional<T>

            Creates an unspecified optional value. 

            :param T:
                The inner type of the optional.

            :returns:

                A :dn:struct:`EdgeDB.Optional<T>` with no value specified.

        .. dn:method::  Create<T>(T value): Optional<T>

            Creates an optional value. 

            :param T value:
                The value of the :dn:struct:`EdgeDB.Optional<T>`.

            :param T:
                The inner type of the optional.

        .. dn:method::  ToNullable<T>(this Optional<T> val): T?

            Converts the :dn:struct:`EdgeDB.Optional<T>` to a ``System.Nullable`1``. 

            :param Optional<T> val:
                The optional to convert.

            :param T:
                The inner type of the optional.

            :returns:

                A nullable version of the optional.

    .. dn:struct:: Optional<T>

        Represents an optional value type. 

        :param T:
            The type of the optional value.


        :property Optional<T> Unspecified:
            Gets the unspecified value for ``T``. 


        :property T Value:
            Gets the value for this parameter. 

            :throws System.InvalidOperationException:
                This property has no value set.


        :property bool IsSpecified:
            Returns true if this value has been specified. 


        .. dn:method:: Optional<T>(T value): Optional<T>

            Creates a new Parameter with the provided value. 

        .. dn:method::  GetValueOrDefault(): T

            Gets the value or ``default``{``T``}. 

            :returns:

                The value or ``default``{``T``}.

        .. dn:method::  GetValueOrDefault(T defaultValue): T

            Gets the value or the provided ``EdgeDB.DocGenerator.docMemberSummaryParamref``. 

            :param T defaultValue:
                The default value of ``T`` to return if the current :dn:class:`EdgeDB.Optional` does not have a value. 

            :returns:

                The ``EdgeDB.Optional`1.Value``; or ``EdgeDB.DocGenerator.docMemberSummaryParamref``.

        .. dn:method::  Equals(object other): bool

        .. dn:method::  GetHashCode(): int

        .. dn:method::  ToString(): string

    .. dn:enum:: ServerErrorCodes

        Represents the different error codes sent by the server defined 


    .. dn:enum:: TLSSecurityMode

        Represents the TLS security mode the client will follow. 


    .. dn:class:: Transaction

        Represents a transaction within EdgeDB. 


        :property TransactionState State:
            Gets the transaction state of this transaction. 


        .. dn:method::  ExecuteAsync(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task

            Executes a given query without reading the returning result. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous execute operation. 

        .. dn:method::  QueryAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<IReadOnlyCollection<TResult>>

            Executes a given query and returns the result as a collection. 

            .. note::

                Cardinality isn't enforced nor takes effect on the return result, the client will always construct a collection out of the data. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QuerySingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result or ``null``. 

            .. note::

                This method enforces ``EdgeDB.Cardinality.AtMostOne``, if your query returns more than one result a :dn:class:`EdgeDB.EdgeDBException` will be thrown. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QueryRequiredSingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result. 

            .. note::

                This method enforces ``EdgeDB.Cardinality.One``, if your query returns zero or more than one result a :dn:class:`EdgeDB.EdgeDBException` will be thrown. 

            :param string query:
                The query to execute.

            :param IDictionary<string, object> args:
                Any arguments that are part of the query.

            :param Nullable<Capabilities> capabilities:
                The allowed capabilities for the query.

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

    .. dn:enum:: TransactionState

        Represents the transaction state of the client. 


    .. dn:class:: TypeBuilder

        Represents the class used to build types from edgedb query results. 


        :property INamingStrategy SchemaNamingStrategy:
            Gets or sets the naming strategy used for deserialization of edgeql property names to dotnet property names. 

            .. note::

                All dotnet types passed to the type builder will have their properties converted to the edgeql version using this naming strategy, the naming convention of the dotnet type will be preserved. 

            .. note::

                If the naming strategy doesn't find a match, the ``EdgeDB.TypeBuilder.AttributeNamingStrategy`` will be used. 


        .. dn:method::  AddOrUpdateTypeBuilder<TType>(Action<TType,IDictionary<string,object>> builder): void

            Adds or updates a custom type builder. 

            :param Action<TType, IDictionary<string, object>> builder:
                The builder for ``TType``.

            :param TType:
                The type of which the builder will build.

            :returns:

                The type info for ``TType``.

        .. dn:method::  AddOrUpdateTypeConverter<TConverter>(): void

            Adds or updates a custom :dn:class:`EdgeDB.TypeConverters.EdgeDBTypeConverter<TSource, TTarget>`

            :param TConverter:
                The type converter to add.

        .. dn:method::  AddOrUpdateTypeFactory<TType>(TypeDeserializerFactory factory): void

            Adds or updates a custom type factory. 

            :param TypeDeserializerFactory factory:
                The factory for ``TType``.

            :param TType:
                The type of which the factory will build.

            :returns:

                The type info for ``TType``.

        .. dn:method::  TryRemoveTypeFactory<TType>(ref TypeDeserializerFactory& factory): bool

            Attempts to remove a type factory. 

            :param TType:
                The type of which to remove the factory.

            :returns:

                if the type factory was removed; otherwise ``true``. ``false``

    .. dn:class:: TypeDeserializerFactory

        A method that will create a object from a :dn:struct:`EdgeDB.ObjectEnumerator`. 

        The enumerator containing the property values.

        :returns:

            An instance of an object that represents the data read from the :dn:struct:`EdgeDB.ObjectEnumerator`. 


.. dn:namespace:: EdgeDB.Binary

.. dn:namespace:: EdgeDB.Binary.Packets

.. dn:namespace:: EdgeDB.TypeConverters

    .. dn:class:: EdgeDBTypeConverter<TSource, TTarget>

        Represents a generic client-side type converter. 

        :param TSource:
            The client-side type which the converter is responsible for converting.

        :param TTarget:
            The database-side type which the converter is responsible for converting to.


        .. dn:method::  ConvertFrom(TTarget value): TSource

            Converts the given ``TTarget`` to a ``TSource``. 

            :param TTarget value:
                The value to convert to a ``TSource``.

            :returns:

                An instance of ``TSource``; or ``default``. 

        .. dn:method::  ConvertTo(TSource value): TTarget

            Converts the given ``TSource`` to a ``TTarget``. 

            :param TSource value:
                The value to convert to a ``TTarget``.

            :returns:

                An instance of ``TTarget``; or ``default``.

        .. dn:method::  CanConvert(Type from, Type to): bool

            Checks if the type builder can convert one type to another. 

            :param Type from:
                The source type.

            :param Type to:
                The target type.

            :returns:

                if the source type can be converted to the target type; otherwise ``true``. ``false``

    .. dn:interface:: IEdgeDBTypeConverter

        Represents a custom type converter capable of converting one type to another. 


        :property Type Source:
            Gets the source type of the converter. 


        :property Type Target:
            Gets the target type of the converter. 


        .. dn:method::  ConvertFrom(object value): object

            Converts the given target value to the source value. 

            :param object value:
                The value to convert.

        .. dn:method::  ConvertTo(object value): object

            Converts the given source value to a the target value. 

            :param object value:
                The value to convert.

        .. dn:method::  CanConvert(Type from, Type to): bool

            Checks if the type builder can convert one type to another. 

            :param Type from:
                The source type.

            :param Type to:
                The target type.

            :returns:

                if the source type can be converted to the target type; otherwise ``true``. ``false``

.. dn:namespace:: EdgeDB.State

    .. dn:class:: Config

        Represents a session-level config. 


        :property Nullable<TimeSpan> IdleTransationTimeout:
            Gets the idle transation timeout duration. 


        :property Nullable<TimeSpan> QueryExecutionTimeout:
            Gets the query execution timeout duration. 


        :property Nullable<bool> AllowDMLInFunctions:
            Gets whether or not to allow data maniplulations in edgeql functions. 


        :property Nullable<DDLPolicy> DDLPolicy:
            Gets the data definition policy for this client. 


        :property Nullable<bool> ApplyAccessPolicies:
            Gets whether or not to apply the access policy. 


        :property Config Default:
            Gets the default config. 


    .. dn:class:: ConfigProperties

        Represents properties used to modify a :dn:class:`EdgeDB.State.Config`. 


        :property Optional<TimeSpan> IdleTransationTimeout:
            Gets or sets the idle transation timeout duration. 


        :property Optional<TimeSpan> QueryExecutionTimeout:
            Gets or sets the query execution timeout duration. 


        :property Optional<bool> AllowDMLInFunctions:
            Gets or sets whether or not to allow data maniplulations in edgeql functions. 


        :property Optional<DDLPolicy> DDLPolicy:
            Gets or sets the data definition policy for this client. 


        :property Optional<bool> ApplyAccessPolicies:
            Gets or sets whether or not to apply the access policy. 


    .. dn:enum:: DDLPolicy

        Represents a DDL policy. 


.. dn:namespace:: EdgeDB.DataTypes

    .. dn:struct:: Json

        Represents a standard json value. 


        .. dn:method:: Json(string value): Json

            Creates a new json type with a provided value. 

            :param string value:
                The raw json value of this json object.

        .. dn:method::  Deserialize<T>(JsonSerializer serializer): T

            Deserializes ``EdgeDB.DataTypes.Json.Value`` into a dotnet type using Newtonsoft.Json. 

            .. note::

                If ``EdgeDB.DataTypes.Json.Value`` is null, the ``default`` value of ``T`` will be returned. 

            :param JsonSerializer serializer:
                The optional custom serializer to use to deserialize ``EdgeDB.DataTypes.Json.Value``. 

            :param T:
                The type to deserialize as.

            :returns:

                The deserialized form of ``EdgeDB.DataTypes.Json.Value``; or ``default``. 

    .. dn:struct:: Memory

        Represents the memory type in EdgeDB. 


        :property long TotalBytes:
            Gets the total amount of bytes for this memory object. 


        :property long TotalMegabytes:
            Gets the total amount of megabytes for this memory object. 


    .. dn:struct:: Range<T>

        Represents the `Range <https://www.edgedb.com/docs/stdlib/range>`_ type in EdgeDB. 

        :param T:
            The inner type of the range.


        :property Nullable<T> Lower:
            Gets the lower bound of the range. 


        :property Nullable<T> Upper:
            Gets the upper bound of the range. 


        :property bool IncludeLower:
            Gets whether or not the lower bound is included. 


        :property bool IncludeUpper:
            Gets whether or not the upper bound is included. 


        :property bool IsEmpty:
            Gets whether or not the range is empty. 


        .. dn:method:: Range<T>(T? lower, T? upper, bool includeLower, bool includeUpper): Range<T>

            Constructs a new range type. 

            :param Nullable<T> lower:
                The lower bound of the range.

            :param Nullable<T> upper:
                The upper bound of the range.

            :param bool includeLower:
                Whether or not to include the lower bound.

            :param bool includeUpper:
                Whether or not to include the upper bound.

        .. dn:method::  Empty(): Range<T>

            Gets an empty range. 

            :returns:

                An empty range.

    .. dn:struct:: TransientTuple

        Represents an abstract tuple which is used for deserializing edgedb tuples to dotnet tuples. 


        :property IReadOnlyCollection<Type> Types:
            Gets the types within this tuple, following the arity order of the tuple. 


        :property IReadOnlyCollection<object> Values:
            Gets the values within this tuple, following the arity order of the tuple. 


        :property Object& Item:
            Gets the value within the tuple at the specified index. 

            .. note::

                The value returned is by-ref and is read-only. 

            The index of the element to return.

            :returns:

                The value at the specified index. 


        :property int Length:
            The length of the tuple. 


        .. dn:method::  ToValueTuple(): ITuple

            Converts this tuple to a ``System.ValueTuple`` with the specific arity. 

            :returns:

                A ``System.ValueTuple`` boxed as a ``System.Runtime.CompilerServices.ITuple``.

        .. dn:method::  ToReferenceTuple(): ITuple

            Converts this tuple to a ``System.Tuple`` with the specific arity. 

            :returns:

                A ``System.Tuple`` boxed as a ``System.Runtime.CompilerServices.ITuple``.

