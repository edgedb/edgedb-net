.. _gel-dotnet-api:

=================
API Documentation
=================

**Namespaces**

- :dn:namespace:`Gel`
- :dn:namespace:`Gel.TypeConverters`
- :dn:namespace:`Gel.Models.DataTypes`
- :dn:namespace:`Gel.State`
- :dn:namespace:`Gel.DataTypes`

.. dn:namespace:: Gel

    .. dn:enum:: Capabilities

        Represents a bitfield of capabilities used when executing queries. 


    .. dn:enum:: Cardinality

        A enum containing the cardinality specification of a command. 


    .. dn:enum:: ConnectionRetryMode

        An enum representing the retry mode when connecting new clients. 


    .. dn:enum:: ErrorSeverity

        An enum representing the error severity of a ``Gel.Binary.Protocol.Common.IProtocolError``. 


    .. dn:struct:: ExecuteResult

        Represents a generic execution result of a command. 


        :property bool IsSuccess:

        :property Exception Exception:

        :property string ExecutedQuery:

    .. dn:class:: GelClientConfig

        Represents the configuration options for a :dn:class:`Gel.GelClientPool` or ``T:Gel.GelTcpClient``


        :property ILogger Logger:
            Gets or sets the logger used for logging messages from the driver. 


        :property ConnectionRetryMode RetryMode:
            Gets or sets the retry mode for connecting new clients. 


        :property uint MaxConnectionRetries:
            Gets or sets the maximum number of times to retry to connect. 


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


        :property bool PreferSystemTemporalTypes:
            Gets or sets whether or not to prefer using .NETs system temporal types when deserializing Gel's temporal types using non-concrete query result definitions. 

            .. note::

                This setting does not override property-defined types, for example: :dn:struct:`Gel.DataTypes.DateTime` would be deserialized as ``dynamic`` regardless of this option. Where this option does apply is when using ``object`` or  as the generic in one of the Query* methods, e.g.: 


        :property bool PreferValueTupleType:
            Gets or sets whether or not to prefer ``System.ValueTuple`` when deserializing the ``std::tuple`` type opposed to :dn:struct:`Gel.DataTypes.TransientTuple` when using non-concrete query result definitions. 


        :property bool ImplicitTypeIds:
            Gets or sets whether or not to include type ids in results. 


    .. dn:class:: GelClientExtensions

        A class containing extension methods for gel clients. 


        .. dn:method::  QueryAsync(this IGelQueryable client, string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<IReadOnlyCollection<object>>

            Executes a given query and returns the result as a collection. 

            .. note::

                Cardinality isn't enforced nor takes effect on the return result, the client will always construct a collection out of the data. 

            :param IGelQueryable client:
                The client to execute the query on.

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

        .. dn:method::  QuerySingleAsync(this IGelQueryable client, string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<object>

            Executes a given query and returns a single result or ``null``. 

            .. note::

                This method enforces ``Gel.Cardinality.AtMostOne``, if your query returns more than one result a :dn:class:`Gel.GelException` will be thrown. 

            :param IGelQueryable client:
                The client to execute the query on.

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

        .. dn:method::  QueryRequiredSingleAsync(this IGelQueryable client, string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<object>

            Executes a given query and returns a single result. 

            .. note::

                This method enforces ``Gel.Cardinality.One``, if your query returns zero or more than one result a :dn:class:`Gel.GelException` will be thrown. 

            :param IGelQueryable client:
                The client to execute the query on.

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

        .. dn:method::  ExecuteAsync<T>(this IGelQueryable client, string query, T args, Capabilities? capabilities, CancellationToken token): Task

            .. note::

                The ``Gel.DocGenerator.docMemberSummaryParamref`` parameter *must* be an `anonymous type <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types>`_ . 

            :param T:
                The dynamic type of the arguments for this query.

        .. dn:method::  QueryAsync<T>(this IGelQueryable client, string query, object args, Capabilities? capabilities, CancellationToken token): Task<IReadOnlyCollection<T>>

            .. note::

                The ``Gel.DocGenerator.docMemberSummaryParamref`` parameter *must* be an `anonymous type <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types>`_ . 

            :param T:
                The type of the return result of the query.

        .. dn:method::  QueryAsync<T>(this IGelQueryable client, string query, T args, Capabilities? capabilities, CancellationToken token): Task<IReadOnlyCollection<object>>

            .. note::

                The ``Gel.DocGenerator.docMemberSummaryParamref`` parameter *must* be an `anonymous type <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types>`_ . 

            :param T:
                The type of the return result of the query.

        .. dn:method::  QuerySingleAsync<T>(this IGelQueryable client, string query, object args, Capabilities? capabilities, CancellationToken token): Task<T>

            .. note::

                The ``Gel.DocGenerator.docMemberSummaryParamref`` parameter *must* be an `anonymous type <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types>`_ . 

            :param T:
                The type of the return result of the query.

        .. dn:method::  QuerySingleAsync<T>(this IGelQueryable client, string query, T args, Capabilities? capabilities, CancellationToken token): Task<object>

            .. note::

                The ``Gel.DocGenerator.docMemberSummaryParamref`` parameter *must* be an `anonymous type <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types>`_ . 

            :param T:
                The type of the return result of the query.

        .. dn:method::  QueryRequiredSingleAsync<T>(this IGelQueryable client, string query, object args, Capabilities? capabilities, CancellationToken token): Task<T>

            .. note::

                The ``Gel.DocGenerator.docMemberSummaryParamref`` parameter *must* be an `anonymous type <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types>`_ . 

            :param T:
                The type of the return result of the query.

        .. dn:method::  QueryRequiredSingleAsync<T>(this IGelQueryable client, string query, T args, Capabilities? capabilities, CancellationToken token): Task<object>

            .. note::

                The ``Gel.DocGenerator.docMemberSummaryParamref`` parameter *must* be an `anonymous type <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/anonymous-types>`_ . 

            :param T:
                The type of the return result of the query.

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

        .. dn:method::  DumpDatabaseAsync(this GelClientPool clientPool, ProtocolVersion dumprestoreVersion, CancellationToken token): Task<Stream>

            Dumps the current database to a stream. 

            :param GelClientPool clientPool:
                The client to preform the dump with.

            :param CancellationToken token:
                A token to cancel the operation with.

            :returns:

                A memory stream containing the entire dumped database.

            :throws Gel.ServerErrorException:
                The server sent an error message during the dumping process.

            :throws Gel.GelException:
                The server sent a mismatched packet.

        .. dn:method::  DumpDatabaseAsync(this GelClientPool clientPool, Stream stream, ProtocolVersion dumprestoreVersion, CancellationToken token): Task

            Dumps the database to a stream. 

            :param GelClientPool clientPool:
                The client to preform the dump with.

            :param Stream stream:
                The stream to write the dump to.

            :param ProtocolVersion dumprestoreVersion:
                The version of the dump format to use.

            :param CancellationToken token:
                A token to cancel the operation with.

            :returns:

                A memory stream containing the entire dumped database.

            :throws Gel.ServerErrorException:
                The server sent an error message during the dumping process.

            :throws Gel.GelException:
                The server sent a mismatched packet.

            :throws System.ArgumentException:
                The provided stream cannot be written to.

        .. dn:method::  RestoreDatabaseAsync(this GelClientPool clientPool, Stream stream, ProtocolVersion dumprestoreVersion, CancellationToken token): Task<string>

            Restores the database based on a database dump stream. 

            :param GelClientPool clientPool:
                The client to preform the restore with.

            :param Stream stream:
                The stream containing the database dump.

            :param ProtocolVersion dumprestoreVersion:
                The version of the dump format to use.

            :param CancellationToken token:
                A token to cancel the operation with.

            :returns:

                The status result of the restore.

            :throws Gel.GelException:
                The server sent an invalid packet or the restore operation couldn't proceed due to the database not being empty. 

            :throws Gel.ServerErrorException:
                The server sent an error during the restore operation.

    .. dn:class:: GelClientPool

        Represents a client pool used to interact with Gel. 


        :property int ConnectedClients:
            Gets the total number of clients within the client pool that are connected. 


        :property int AvailableClients:
            Gets the number of available (idle) clients within the client pool. 

            .. note::

                This property can equal ``Gel.GelClientPool.ConnectedClients`` if the client type doesn't have restrictions on idling. 


        :property Config Config:
            The :dn:class:`Gel.State.Config` containing session-level configuration. 


        :property string Module:
            The default module for this client. 


        :property IReadOnlyDictionary<string, string> Aliases:
            The module aliases for this client. 


        :property IReadOnlyDictionary<string, object> Globals:
            The globals for this client. 


        :property IReadOnlyDictionary<string, object> ServerConfig:
            Gets the Gel server config. 

            .. note::

                The returned dictionary can be empty if the client pool hasn't connected any clients or the clients don't support getting a server config. 


        .. dn:method:: GelClientPool(): GelClientPool

            Creates a new instance of a Gel client pool allowing you to execute commands. 

            .. note::

                This constructor uses the default config and will attempt to find your Gel project toml file in the current working directory. If no file is found this method will throw a :dn:class:`Gel.ConfigurationException`. 

        .. dn:method:: GelClientPool(GelClientPoolConfig clientPoolConfig): GelClientPool

            Creates a new instance of a Gel client pool allowing you to execute commands. 

            .. note::

                This constructor will attempt to find your Gel project toml file in the current working directory. If no file is found this method will throw a :dn:class:`Gel.ConfigurationException`. 

            :param GelClientPoolConfig clientPoolConfig:
                The config for this client pool.

        .. dn:method:: GelClientPool(GelConnection connection): GelClientPool

            Creates a new instance of a Gel client pool allowing you to execute commands. 

            :param GelConnection connection:
                The connection parameters used to create new clients.

        .. dn:method:: GelClientPool(GelConnection connection, GelClientPoolConfig clientPoolConfig): GelClientPool

            Creates a new instance of a Gel client pool allowing you to execute commands. 

            :param GelConnection connection:
                The connection parameters used to create new clients.

            :param GelClientPoolConfig clientPoolConfig:
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

            :param TResult:
                The type of the return result of the query.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QuerySingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result or ``null``. 

            .. note::

                This method enforces ``Gel.Cardinality.AtMostOne``, if your query returns more than one result a :dn:class:`Gel.GelException` will be thrown. 

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

        .. dn:method::  QueryRequiredSingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result. 

            .. note::

                This method enforces ``Gel.Cardinality.One``, if your query returns zero or more than one result a :dn:class:`Gel.GelException` will be thrown. 

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

            :throws Gel.ResultCardinalityMismatchException:
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

            Ensures that a connection is established to the Gel server; and that the client pool is configured to the servers recommended pool size. 

            :param CancellationToken token:
                A cancellation token used to cancel the asynchronous operation.

            :returns:

                A ``System.Threading.Tasks.ValueTask`` representing the asynchronous connection operation. 

        .. dn:method::  WithConfig(Action<ConfigProperties> configDelegate): GelClientPool

            Creates a new client with the specified ``Gel.GelClientPool.Config``. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one. 

            :param Action<ConfigProperties> configDelegate:
                A delegate used to modify the config.

            :returns:

                A new client with the specified config. 

        .. dn:method::  WithConfig(Config config): GelClientPool

            Creates a new client with the specified ``Gel.GelClientPool.Config``. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one. 

            :param Config config:
                The config for the new client.

            :returns:

                A new client with the specified config. 

        .. dn:method::  WithGlobals(IDictionary<string,object> globals): GelClientPool

            Creates a new client with the specified `Globals <https://www.geldata.com/docs/datamodel/globals#globals>`_. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one.
                The newly created client doesn't copy any of the parents globals, this method is settative to the ``Gel.GelClientPool.Globals`` property. 

            :param IDictionary<string, object> globals:
                The globals for the newly create client.

            :returns:

                A new client with the specified globals. 

        .. dn:method::  WithModule(string module): GelClientPool

            Creates a new client with the specified ``Gel.GelClientPool.Module``. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one. 

            :param string module:
                The module for the new client.

            :returns:

                A new client with the specified module. 

        .. dn:method::  WithAliases(IDictionary<string,string> aliases): GelClientPool

            Creates a new client with the specified ``Gel.GelClientPool.Aliases``. 

            .. note::

                The created client is a 'sub' client of this one, the child client shares the same client pool as this one.
                The newly created client doesn't copy any of the parents aliases, this method is settative to the ``Gel.GelClientPool.Aliases`` property. 

            :param IDictionary<string, string> aliases:
                The module aliases for the new client.

            :returns:

                A new client with the specified module aliases. 

    .. dn:class:: GelClientPoolConfig

        Represents a config for a :dn:class:`Gel.GelClientPool`, extending :dn:class:`Gel.GelClientConfig`. 


        :property int DefaultPoolSize:
            Gets or sets the default client pool size. 


        :property GelClientType ClientType:
            Gets or sets the client type the pool will use. 


    .. dn:enum:: GelClientType

        Represents different client types used in a :dn:class:`Gel.GelClientPool`. 


    .. dn:class:: GelConnection

        Represents a class containing information on how to connect to a gel instance. 


        :property string Hostname:
            Gets the hostname of the gel instance to connect to. 

            .. note::

                This property defaults to localhost. 


        :property int Port:
            Gets the port of the gel instance to connect to. 

            .. note::

                This property defaults to 5656 


        :property string Database:
            Gets the database name to use when connecting. 

            .. note::

                This property defaults to ``"edgedb"``. It is mutually exclusive with ``Gel.GelConnection.Branch``. 


        :property string Branch:
            Gets the branch name to use when connecting. 

            .. note::

                This property defaults to ``"__default__"``. It is mutually exclusive with ``Gel.GelConnection.Database``


        :property string Username:
            Gets the username used to connect to the database. 

            .. note::

                This property defaults to ``"edgedb"`` 


        :property string Password:
            Gets the password to connect to the database. 


        :property string SecretKey:
            Gets the secret key used to authenticate with cloud instances. 


        :property string TLSCertificateAuthority:
            Gets the TLS Certificate Authority. 


        :property TLSSecurityMode TLSSecurity:
            Gets the TLS security level. 

            .. note::

                The default value is ``Gel.TLSSecurityMode.Strict``. 


        :property string TLSServerName:
            Gets the TLS server name to be used. 

            .. note::

                Overrides the value provided by Hostname. 


        :property int WaitUntilAvailable:
            Gets the number of miliseconds a client will wait for a connection to be established with the server. 


        :property Dictionary<string, string> ServerSettings:
            Additional settings for the server connection. 

            .. note::

                This currently has no effect. 


        .. dn:method::  ToString(): string

        .. dn:method::  Create(Options options): GelConnection

            Parses the ``gel.toml``, optional ``T:Gel.GelConnection.Options``, and environment variables
            to build an :dn:class:`Gel.GelConnection`.

            This function will first search for the first valid primary args (which can set host/port)
            in the following order:
            - ``T:Gel.GelConnection.Options``
            - Environment variables
            - ``gel.toml`` file

            It will then apply any secondary args from the environment variables and options.

            If any primary ``T:Gel.GelConnection.Options`` are present, then all environment variables
            are ignored.

            See the `documentation <https://www.geldata.com/docs/reference/connection>`_ for more information. 

            :param Options options:
                Options used to build the :dn:class:`Gel.GelConnection`.

            :returns:

                A :dn:class:`Gel.GelConnection` class that can be used to connect to a Gel instance. 

            :throws Gel.ConfigurationException:
                An error occured while parsing or configuring the :dn:class:`Gel.GelConnection`. 

    .. dn:class:: GelDeserializerAttribute

        Marks the current method as the method to use to deserialize the current type. 


    .. dn:class:: GelHostingExtensions

        A class containing extension methods for DI. 


        .. dn:method::  AddClientPool(this IServiceCollection collection, GelConnection connection, Action<GelClientPoolConfig> clientPoolConfig): IServiceCollection

            Adds a :dn:class:`Gel.GelClientPool` singleton to a ``Microsoft.Extensions.DependencyInjection.IServiceCollection``. 

            :param IServiceCollection collection:
                The source collection to add a :dn:class:`Gel.GelClientPool` to.

            :param GelConnection connection:
                An optional connection arguments for the client.

            :param Action<GelClientPoolConfig> clientPoolConfig:
                An optional configuration delegate for configuring the :dn:class:`Gel.GelClientPool`. 

            :returns:

                The source ``Microsoft.Extensions.DependencyInjection.IServiceCollection`` with :dn:class:`Gel.GelClientPool` added as a singleton. 

    .. dn:class:: GelIgnoreAttribute

        Marks the current target to be ignored when deserializing or building queries. 


    .. dn:class:: GelPropertyAttribute

        Marks the current field or property as a valid target for serializing/deserializing. 


        .. dn:method:: GelPropertyAttribute(string propertyName): GelPropertyAttribute

            Marks this member to be used when serializing/deserializing. 

            :param string propertyName:
                The name of the member in the gel schema.

    .. dn:class:: GelTypeAttribute

        Marks this class or struct as a valid type to use when serializing/deserializing. 


        .. dn:method:: GelTypeAttribute(string name): GelTypeAttribute

            Marks this as a valid target to use when serializing/deserializing. 

            :param string name:
                The name of the type in the gel schema.

        .. dn:method:: GelTypeAttribute(): GelTypeAttribute

            Marks this as a valid target to use when serializing/deserializing. 

    .. dn:class:: GelTypeConverterAttribute

        Marks the current property to be deserialized/serialized with a specific :dn:class:`Gel.TypeConverters.GelTypeConverter<TSource, TTarget>`. 


        .. dn:method:: GelTypeConverterAttribute(Type converterType): GelTypeConverterAttribute

            Initializes the :dn:class:`Gel.GelTypeConverterAttribute` with the specified :dn:class:`Gel.TypeConverters.GelTypeConverter<TSource, TTarget>`. 

            :param Type converterType:
                The type of the converter.

            :throws System.ArgumentException:
                is not a valid ``Gel.DocGenerator.docMemberSummaryParamref``. :dn:class:`Gel.TypeConverters.GelTypeConverter<TSource, TTarget>`

    .. dn:class:: Group<TKey, TElement>

        Represents a group result returned from the ``GROUP`` expression. 

        :param TKey:
            The type of the key used to group the elements.

        :param TElement:
            The type of the elements.


        :property IReadOnlyCollection<string> Grouping:
            Gets the name of the property that was grouped by. 


        :property IReadOnlyCollection<TElement> Elements:
            Gets a collection of elements that have the same key as ``Gel.Group`2.Key``. 


        :property TKey Key:
            Gets the key used to group the set of ``Gel.Group`2.Elements``. 


        .. dn:method:: Group<TKey,TElement>(TKey key, IEnumerable<string> groupedBy, IEnumerable<TElement> elements): Group<TKey,TElement>

            Constructs a new grouping. 

            :param TKey key:
                The key that each element share.

            :param IEnumerable<string> groupedBy:
                The property used to group the elements.

            :param IEnumerable<TElement> elements:
                The collection of elements that have the specified key.

        .. dn:method::  GetEnumerator(): IEnumerator<TElement>

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


    .. dn:interface:: IGelQueryable

        Represents a object that can be used to query a Gel instance. 


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

            :param TResult:
                The type of the return result of the query.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QuerySingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result or ``null``. 

            .. note::

                This method enforces ``Gel.Cardinality.AtMostOne``, if your query returns more than one result a :dn:class:`Gel.GelException` will be thrown. 

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

        .. dn:method::  QueryRequiredSingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result. 

            .. note::

                This method enforces ``Gel.Cardinality.One``, if your query returns zero or more than one result a :dn:class:`Gel.GelException` will be thrown. 

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

            :throws Gel.ResultCardinalityMismatchException:
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

                This is the default naming strategy for the :dn:class:`Gel.TypeBuilder`. 


        .. dn:method::  Convert(PropertyInfo property): string

            Converts the ``Gel.DocGenerator.docMemberSummaryParamref``'s name to the desired naming scheme. 

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

        An enum representing the transaction mode within a :dn:class:`Gel.Transaction`. 


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

            Converts this :dn:struct:`Gel.ObjectEnumerator` to a ``dynamic`` object. 

            :returns:

                A ``dynamic`` object.

        .. dn:method::  Flatten(): IDictionary<string,object>

            Flattens this :dn:struct:`Gel.ObjectEnumerator` into a dictionary with keys being property names. 

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

                A :dn:struct:`Gel.Optional<T>` with no value specified.

        .. dn:method::  Create<T>(T value): Optional<T>

            Creates an optional value. 

            :param T value:
                The value of the :dn:struct:`Gel.Optional<T>`.

            :param T:
                The inner type of the optional.

        .. dn:method::  ToNullable<T>(this Optional<T> val): T?

            Converts the :dn:struct:`Gel.Optional<T>` to a ``System.Nullable`1``. 

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

            Gets the value or the provided ``Gel.DocGenerator.docMemberSummaryParamref``. 

            :param T defaultValue:
                The default value of ``T`` to return if the current :dn:class:`Gel.Optional` does not have a value. 

            :returns:

                The ``Gel.Optional`1.Value``; or ``Gel.DocGenerator.docMemberSummaryParamref``.

        .. dn:method::  Equals(object other): bool

        .. dn:method::  GetHashCode(): int

        .. dn:method::  ToString(): string

    .. dn:class:: ProtocolVersion

        Represents a protocol version used within Gel. 


        :property ushort Major:
            Gets the major component of the protocol. 


        :property ushort Minor:
            Gets the minor version of the protocol. 


        .. dn:method:: ProtocolVersion(ushort major, ushort minor): ProtocolVersion

            Constructs a new :dn:class:`Gel.ProtocolVersion`. 

            :param ushort major:
                The major component of the protocol.

            :param ushort minor:
                The minor component of the protocol.

        .. dn:method::  ToString(): string

        .. dn:method::  Equals(object obj): bool

    .. dn:enum:: ServerErrorCodes

        Represents the different error codes sent by the server defined 


    .. dn:enum:: TLSSecurityMode

        Represents the TLS security mode the client will follow. 


    .. dn:class:: Transaction

        Represents a transaction within Gel. 


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

            :param TResult:
                The type of the return result of the query.

            :returns:

                A task representing the asynchronous query operation. The result of the task is the result of the query. 

        .. dn:method::  QuerySingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result or ``null``. 

            .. note::

                This method enforces ``Gel.Cardinality.AtMostOne``, if your query returns more than one result a :dn:class:`Gel.GelException` will be thrown. 

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

        .. dn:method::  QueryRequiredSingleAsync<TResult>(string query, IDictionary<string,object> args, Capabilities? capabilities, CancellationToken token): Task<TResult>

            Executes a given query and returns a single result. 

            .. note::

                This method enforces ``Gel.Cardinality.One``, if your query returns zero or more than one result a :dn:class:`Gel.GelException` will be thrown. 

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

    .. dn:enum:: TransactionState

        Represents the transaction state of the client. 


    .. dn:class:: TypeBuilder

        Represents the class used to build types from gel query results. 


        :property INamingStrategy SchemaNamingStrategy:
            Gets or sets the naming strategy used for deserialization of edgeql property names to dotnet property names. 

            .. note::

                All dotnet types passed to the type builder will have their properties converted to the edgeql version using this naming strategy, the naming convention of the dotnet type will be preserved. 

            .. note::

                If the naming strategy doesn't find a match, the ``Gel.TypeBuilder.AttributeNamingStrategy`` will be used. 


        .. dn:method::  AddOrUpdateTypeBuilder<TType>(Action<TType,IDictionary<string,object>> builder): void

            Adds or updates a custom type builder. 

            :param Action<TType, IDictionary<string, object>> builder:
                The builder for ``TType``.

            :param TType:
                The type of which the builder will build.

            :returns:

                The type info for ``TType``.

        .. dn:method::  AddOrUpdateTypeConverter<TConverter>(): void

            Adds or updates a custom :dn:class:`Gel.TypeConverters.GelTypeConverter<TSource, TTarget>`

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

        A method that will create a object from a :dn:struct:`Gel.ObjectEnumerator`. 

        The enumerator containing the property values.

        :returns:

            An instance of an object that represents the data read from the :dn:struct:`Gel.ObjectEnumerator`. 


.. dn:namespace:: Gel.Binary

.. dn:namespace:: Gel.Binary.Protocol

.. dn:namespace:: Gel.Binary.Protocol.V1._0.Packets

.. dn:namespace:: Gel.TypeConverters

    .. dn:class:: GelTypeConverter<TSource, TTarget>

        Represents a generic client-side type converter. 

        :param TSource:
            The client-side type which the converter is responsible for converting.

        :param TTarget:
            The database-side type which the converter is responsible for converting to.


        .. dn:method::  CanConvert(Type from, Type to): bool

            Checks if the type builder can convert one type to another. 

            :param Type from:
                The source type.

            :param Type to:
                The target type.

            :returns:

                if the source type can be converted to the target type; otherwise ``true``. ``false``

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

    .. dn:interface:: IGelTypeConverter

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

.. dn:namespace:: Gel.Models.DataTypes

    .. dn:struct:: MultiRange<T>

        Represents the ``multirange`` type in Gel. 

        :param T:
            The inner type of the multirange.


        :property int Length:
            Gets the length of this multirange. 


        :property Range<T>& Item:
            Gets a :dn:struct:`Gel.DataTypes.Range<T>` element within this multirange. 


        .. dn:method:: MultiRange<T>(HashSet<Range<T>> set): MultiRange<T>

            Constructs a new :dn:struct:`Gel.Models.DataTypes.MultiRange<T>`. 

            :param HashSet<Range<T>> set:
                A set of ranges to put within this multirange.

        .. dn:method::  ToSet(): HashSet<Range<T>>

            Returns a hashset that represents this multirange. 

            :returns:

                A hashset, derived from the contents of this multirange.

        .. dn:method::  GetEnumerator(): IEnumerator<Range<T>>

.. dn:namespace:: Gel.State

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

        Represents properties used to modify a :dn:class:`Gel.State.Config`. 


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


.. dn:namespace:: Gel.DataTypes

    .. dn:struct:: DateDuration

        A struct representing a span of time in days. 

        .. note::

            This type is only available in Gel 2.0 or later. 


        :property TimeSpan TimeSpan:
            Gets a ``System.TimeSpan`` that represents the current :dn:struct:`Gel.DataTypes.DateDuration`. 


        :property int Days:
            Gets the days component of this :dn:struct:`Gel.DataTypes.DateDuration`. 


        :property int Months:
            Gets the months component of this :dn:struct:`Gel.DataTypes.DateDuration`. 


        .. dn:method:: DateDuration(int days, int months): DateDuration

            Constructs a new :dn:struct:`Gel.DataTypes.DateDuration` with the given days and months. 

            :param int days:
                The amount of days this :dn:struct:`Gel.DataTypes.DateDuration` has.

            :param int months:
                The amount of months this :dn:struct:`Gel.DataTypes.DateDuration` has.

        .. dn:method:: DateDuration(TimeSpan timespan): DateDuration

            Constructs a new :dn:struct:`Gel.DataTypes.DateDuration` from a given timespan. 

            :param TimeSpan timespan:
                The timespan to use to contruct this :dn:struct:`Gel.DataTypes.DateDuration`.

        .. dn:method::  Equals(object obj): bool

        .. dn:method::  GetHashCode(): int

    .. dn:struct:: DateTime

        A struct representing a timezone-aware moment in time. 


        :property DateTimeOffset DateTimeOffset:
            Gets a ``Gel.DataTypes.DateTime.DateTimeOffset`` that represents this :dn:struct:`Gel.DataTypes.DateTime`. 


        :property DateTime SystemDateTime:
            Gets a ``System.DateTime`` that represents this :dn:struct:`Gel.DataTypes.DateTime`. 


        :property long Microseconds:
            Gets the microsecond component of this :dn:struct:`Gel.DataTypes.DateTime`; representing the amount of microseconds since January 1st 2000, 00:00. 


        :property DateTime Now:
            Gets a :dn:struct:`Gel.DataTypes.DateTime` object whos date and time are set to the current UTC time. 


        .. dn:method:: DateTime(DateTime datetime): DateTime

            Constructs a new :dn:struct:`Gel.DataTypes.DateTime`. 

            .. note::

                The supplied ``System.DateTime`` will be rounded to the nearest microsecond. 

            :param DateTime datetime:
                The ``System.DateTime`` to use to construct this :dn:struct:`Gel.DataTypes.DateTime`. 

        .. dn:method:: DateTime(DateTimeOffset datetime): DateTime

            Constructs a new :dn:struct:`Gel.DataTypes.DateTime`. 

            .. note::

                The supplied ``System.DateTimeOffset`` will be rounded to the nearest microsecond. 

            :param DateTimeOffset datetime:
                The ``System.DateTimeOffset`` to use to construct this :dn:struct:`Gel.DataTypes.DateTime`

        .. dn:method::  Equals(object obj): bool

        .. dn:method::  GetHashCode(): int

    .. dn:struct:: Duration

        A struct representing a span of time. 


        :property TimeSpan TimeSpan:
            Gets a ``System.TimeSpan`` that represents the current :dn:struct:`Gel.DataTypes.Duration`. 


        :property long Microseconds:
            Gets the microsecond component of this :dn:struct:`Gel.DataTypes.Duration`. 


        .. dn:method:: Duration(long microseconds): Duration

            Constructs a new :dn:struct:`Gel.DataTypes.Duration`. 

            :param long microseconds:
                The microsecond component of this duration.

        .. dn:method:: Duration(TimeSpan timespan): Duration

            Constructs a new :dn:struct:`Gel.DataTypes.Duration`. 

            .. note::

                The provided ``System.TimeSpan`` will be rounded to the nearest microsecond. 

            :param TimeSpan timespan:
                A timespan used to contruct this duration.

        .. dn:method::  Equals(object obj): bool

        .. dn:method::  GetHashCode(): int

    .. dn:struct:: Json

        Represents a standard json value. 


        .. dn:method:: Json(string value): Json

            Creates a new json type with a provided value. 

            :param string value:
                The raw json value of this json object.

        .. dn:method::  Deserialize<T>(JsonSerializer serializer): T

            Deserializes ``Gel.DataTypes.Json.Value`` into a dotnet type using Newtonsoft.Json. 

            .. note::

                If ``Gel.DataTypes.Json.Value`` is null, the ``default`` value of ``T`` will be returned. 

            :param JsonSerializer serializer:
                The optional custom serializer to use to deserialize ``Gel.DataTypes.Json.Value``. 

            :param T:
                The type to deserialize as.

            :returns:

                The deserialized form of ``Gel.DataTypes.Json.Value``; or ``default``. 

    .. dn:struct:: LocalDate

        A struct representing a date without a timezone. 


        :property DateOnly DateOnly:
            Gets a ``System.DateOnly`` that represents the current :dn:struct:`Gel.DataTypes.LocalDate`. 


        :property int Days:
            Gets the days component of this :dn:struct:`Gel.DataTypes.LocalDate`; representing the number of days since January 1st 2000. 


        .. dn:method:: LocalDate(int days): LocalDate

            Constructs a new :dn:struct:`Gel.DataTypes.LocalDate`. 

            :param int days:
                The number of days since January 1st 2000.

        .. dn:method:: LocalDate(DateOnly date): LocalDate

            Constructs a new :dn:struct:`Gel.DataTypes.LocalDate`

            :param DateOnly date:
                The ``System.DateOnly`` used to construct this :dn:struct:`Gel.DataTypes.LocalDate`.

        .. dn:method::  Equals(object obj): bool

        .. dn:method::  GetHashCode(): int

    .. dn:struct:: LocalDateTime

        A struct representing a date and time without a timezone. 


        :property DateTimeOffset DateTimeOffset:
            Gets a ``System.DateTimeOffset`` that represents this :dn:struct:`Gel.DataTypes.LocalDateTime`. 


        :property DateTime DateTime:
            Gets a ``System.DateTime`` that represents this :dn:struct:`Gel.DataTypes.LocalDateTime`


        :property long Microseconds:
            Gets the microsecond component of this :dn:struct:`Gel.DataTypes.LocalDateTime`; representing the amount of microseconds since January 1st 2000, 00:00. 


        :property LocalDateTime Now:
            Gets a :dn:struct:`Gel.DataTypes.LocalDateTime` object whos date and time are set to the current UTC time. 


        .. dn:method:: LocalDateTime(long microsecond): LocalDateTime

            Constucts a new :dn:struct:`Gel.DataTypes.LocalDateTime`. 

            :param long microsecond:
                The number of microseconds since January 1st 2000, 00:00.

        .. dn:method:: LocalDateTime(DateTime datetime): LocalDateTime

            Constucts a new :dn:struct:`Gel.DataTypes.LocalDateTime`. 

            .. note::

                The provided ``System.DateTime`` will be rounded to the nearest microsecond. 

            :param DateTime datetime:
                The ``System.DateTime`` used to construct this :dn:struct:`Gel.DataTypes.LocalDateTime`.

        .. dn:method:: LocalDateTime(DateTimeOffset datetime): LocalDateTime

            Constucts a new :dn:struct:`Gel.DataTypes.LocalDateTime`. 

            .. note::

                The provided ``System.DateTimeOffset`` will be rounded to the nearest microsecond. 

            :param DateTimeOffset datetime:
                The ``System.DateTimeOffset`` used to construct this :dn:struct:`Gel.DataTypes.LocalDateTime`.

        .. dn:method::  Equals(object obj): bool

        .. dn:method::  GetHashCode(): int

    .. dn:struct:: LocalTime

        A struct representing a time without a timezone. 


        :property TimeOnly TimeOnly:
            Gets a ``System.TimeOnly`` that represents this :dn:struct:`Gel.DataTypes.LocalTime`. 


        :property TimeSpan TimeSpan:
            Gets a ``System.TimeSpan`` that represents this :dn:struct:`Gel.DataTypes.LocalTime`. 


        :property long Microseconds:
            Gets the microsecond component of this :dn:struct:`Gel.DataTypes.LocalTime`; representing the number of microseconds since midnight. 


        .. dn:method:: LocalTime(long microseconds): LocalTime

            Constructs a new :dn:struct:`Gel.DataTypes.LocalTime`. 

            :param long microseconds:
                The number of microseconds since midnight.

        .. dn:method:: LocalTime(TimeOnly timeOnly): LocalTime

            Constructs a new :dn:struct:`Gel.DataTypes.LocalTime`. 

            .. note::

                The provided ``System.TimeOnly`` will be rounded to the nearest microsecond. 

            :param TimeOnly timeOnly:
                The ``System.TimeOnly`` used to construct this :dn:struct:`Gel.DataTypes.LocalTime`.

        .. dn:method:: LocalTime(TimeSpan timespan): LocalTime

            Constructs a new :dn:struct:`Gel.DataTypes.LocalTime`. 

            .. note::

                The provided ``System.TimeSpan`` will be rounded to the nearest microsecond. 

            :param TimeSpan timespan:
                The ``System.TimeSpan`` used to construct this :dn:struct:`Gel.DataTypes.LocalTime`.

        .. dn:method::  Equals(object obj): bool

        .. dn:method::  GetHashCode(): int

    .. dn:struct:: Memory

        Represents the memory type in Gel. 


        :property long TotalBytes:
            Gets the total amount of bytes for this memory object. 


        :property long TotalMegabytes:
            Gets the total amount of megabytes for this memory object. 


    .. dn:struct:: Range<T>

        Represents the `Range <https://www.geldata.com/docs/stdlib/range>`_ type in Gel. 

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

        .. dn:method::  Equals(object obj): bool

        .. dn:method::  GetHashCode(): int

        .. dn:method::  op_Equality(Range<T> left, Range<T> right): bool

        .. dn:method::  op_Inequality(Range<T> left, Range<T> right): bool

    .. dn:struct:: RelativeDuration

        A struct representing a relative span of time. 


        :property TimeSpan TimeSpan:
            Gets a ``System.TimeSpan`` that represents the current :dn:struct:`Gel.DataTypes.RelativeDuration`. 


        :property long Microseconds:
            Gets the microsecond component of this :dn:struct:`Gel.DataTypes.RelativeDuration`. 


        :property int Days:
            Gets the days component of this :dn:struct:`Gel.DataTypes.RelativeDuration`. 


        :property int Months:
            Gets the months component of this :dn:struct:`Gel.DataTypes.RelativeDuration`. 


        .. dn:method:: RelativeDuration(long microseconds, int days, int months): RelativeDuration

            Constructs a new :dn:struct:`Gel.DataTypes.RelativeDuration`. 

            :param long microseconds:
                The microsecond component.

            :param int days:
                The days component.

            :param int months:
                The months component,

        .. dn:method:: RelativeDuration(TimeSpan timespan): RelativeDuration

            Constructs a new :dn:struct:`Gel.DataTypes.RelativeDuration`. 

            .. note::

                The provided ``System.TimeSpan`` will be rounded to the nearest microsecond. 

            :param TimeSpan timespan:
                The timespan used to construct this :dn:struct:`Gel.DataTypes.RelativeDuration`.

        .. dn:method::  Equals(object obj): bool

        .. dn:method::  GetHashCode(): int

    .. dn:struct:: TransientTuple

        Represents an abstract tuple which is used for deserializing gel tuples to dotnet tuples. 


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

