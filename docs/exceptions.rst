.. _edgedb-dotnet-exceptions:

==========
Exceptions
==========

.. dn:class:: EdgeDB.ConfigurationException

    Represents a generic configuration error. 


    .. dn:method:: ConfigurationException(string message): ConfigurationException

        Creates a new :dn:class:`EdgeDB.ConfigurationException`. 

        :param string message:
            The configuration error message.

    .. dn:method:: ConfigurationException(string message, Exception inner): ConfigurationException

        Creates a new :dn:class:`EdgeDB.ConfigurationException`. 

        :param string message:
            The configuration error message.

        :param Exception inner:
            An inner exception.

.. dn:class:: EdgeDB.ConnectionFailedException

    Represents a connection failure that cannot be retried. 


    :property int Attempts:
        Gets the number of attempts the client made to reconnect. 


    .. dn:method:: ConnectionFailedException(int attempts): ConnectionFailedException

        Constructs a new :dn:class:`EdgeDB.ConnectionFailedException` with the number of connection attempts made. 

        :param int attempts:
            The number of attempts made to connect.

.. dn:class:: EdgeDB.ConnectionFailedTemporarilyException

    Represents a temporary connection failiure exception. 


    :property SocketError SocketError:
        Gets the socket error that caused the connection to fail. 


    .. dn:method:: ConnectionFailedTemporarilyException(SocketError error): ConnectionFailedTemporarilyException

        Constructs a new :dn:class:`EdgeDB.ConnectionFailedTemporarilyException` with the specified socket error. 

        :param SocketError error:
            The underlying socket error that caused this exception to be thrown.

.. dn:class:: EdgeDB.CustomClientException

    Represents a generic error with custom clients. 


    .. dn:method:: CustomClientException(string message): CustomClientException

        Constructs a new :dn:class:`EdgeDB.CustomClientException` with the specified error message. 

        :param string message:
            The error message describing why this exception was thrown.

.. dn:class:: EdgeDB.EdgeDBErrorException

    Represents an exception that was caused by an error from EdgeDB. 


    :property string Details:
        Gets the details related to the error. 


    :property string ServerTraceBack:
        Gets the server traceback log for the error. 


    :property string Hint:
        Gets the hint for the error. 


    :property string Query:
        Gets the query that caused this error. 


    .. dn:method::  ToString(): string

        Prettifies the error if it was a result of a bad query string; otherwise formats it. 

.. dn:class:: EdgeDB.EdgeDBException

    Represents a generic exception that occured with the edgedb library. 


    .. dn:method:: EdgeDBException(bool shouldRetry, bool shouldReconnect): EdgeDBException

        Constructs a new :dn:class:`EdgeDB.EdgeDBException`. 

        :param bool shouldRetry:
            Whether or not this exception is retryable.

        :param bool shouldReconnect:
            Whether or not the client who caught this exception should reconnect.

    .. dn:method:: EdgeDBException(string message, bool shouldRetry, bool shouldReconnect): EdgeDBException

        Constructs a new :dn:class:`EdgeDB.EdgeDBException` with the specified error message. 

        :param string message:
            The error message describing why this exception was thrown.

        :param bool shouldRetry:
            Whether or not this exception is retryable.

        :param bool shouldReconnect:
            Whether or not the client who caught this exception should reconnect.

    .. dn:method:: EdgeDBException(string message, Exception innerException, bool shouldRetry, bool shouldReconnect): EdgeDBException

        Constructs a new :dn:class:`EdgeDB.EdgeDBException` with the specified error message and inner exception. 

        :param string message:
            The error message describing why this exception was thrown.

        :param Exception innerException:
            The inner exception.

        :param bool shouldRetry:
            Whether or not this exception is retryable.

        :param bool shouldReconnect:
            Whether or not the client who caught this exception should reconnect.

.. dn:class:: EdgeDB.InvalidConnectionException

    Represents an error with the provided connection details. 


    .. dn:method:: InvalidConnectionException(string message): InvalidConnectionException

        Constructs a new :dn:class:`EdgeDB.InvalidConnectionException` with the specified error message. 

        :param string message:
            The error message describing why this exception was thrown.

.. dn:class:: EdgeDB.InvalidSignatureException

    Represents an exception that occurs when the server signature is incorrect. 


    .. dn:method:: InvalidSignatureException(): InvalidSignatureException

        Constructs a new :dn:class:`EdgeDB.InvalidSignatureException`. 

.. dn:class:: EdgeDB.MissingCodecException

    Represents an exception that occurs when the client doesn't have a codec for incoming or outgoing data. 


    .. dn:method:: MissingCodecException(string message): MissingCodecException

        Constructs a new :dn:class:`EdgeDB.MissingCodecException` with the specified error message. 

        :param string message:
            The error message describing why this exception was thrown.

.. dn:class:: EdgeDB.MissingRequiredException

    Represents an exception that occurs when required data isn't returned. 


    .. dn:method:: MissingRequiredException(): MissingRequiredException

        Constructs a new :dn:class:`EdgeDB.MissingRequiredException`. 

.. dn:class:: EdgeDB.NoTypeConverterException

    Represents an exception thrown when no type converter could be found. 


    .. dn:method:: NoTypeConverterException(Type target, Type source): NoTypeConverterException

        Constructs a new :dn:class:`EdgeDB.NoTypeConverterException` with the target and source types. 

        :param Type target:
            The target type that ``EdgeDB.DocGenerator.docMemberSummaryParamref`` was going to be converted to.

        :param Type source:
            The source type.

    .. dn:method:: NoTypeConverterException(Type target, Type source, Exception inner): NoTypeConverterException

        Constructs a new :dn:class:`EdgeDB.NoTypeConverterException` with the target and source type, and inner exception. 

        :param Type target:
            The target type that ``EdgeDB.DocGenerator.docMemberSummaryParamref`` was going to be converted to.

        :param Type source:
            The source type.

        :param Exception inner:
            The inner exception.

    .. dn:method:: NoTypeConverterException(string message, Exception inner): NoTypeConverterException

        Constructs a new :dn:class:`EdgeDB.NoTypeConverterException` with the specified error message. 

        :param string message:
            The error message describing why this exception was thrown.

        :param Exception inner:
            An optional inner exception.

.. dn:class:: EdgeDB.ResultCardinalityMismatchException

    Represents an exception that occurs when a queries cardinality isn't what the client was expecting. 


    .. dn:method:: ResultCardinalityMismatchException(Cardinality expected, Cardinality actual): ResultCardinalityMismatchException

        Constructs a new :dn:class:`EdgeDB.ResultCardinalityMismatchException`. 

        :param Cardinality expected:
            The expected cardinality.

        :param Cardinality actual:
            The actual cardinality

.. dn:class:: EdgeDB.TransactionException

    Represents an exception that occurs within transactions. 


    .. dn:method:: TransactionException(string message, Exception innerException): TransactionException

        Constructs a new :dn:class:`EdgeDB.TransactionException` with a specified error message. 

        :param string message:
            The error message describing why this exception was thrown.

        :param Exception innerException:
            An optional inner exception.

.. dn:class:: EdgeDB.UnexpectedMessageException

    Represents an exception that occurs when the client receives an unexpected message. 


