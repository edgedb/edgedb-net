.. _gel-dotnet-connection-parameters:

=====================
Connection Parameters
=====================

The ``GelClientPool`` constructor can accept an ``GelConnection`` class which
specifies how the client should connect to Gel. The main way to to construct
a ``GelConnection`` is to use the static helper method ``Create``

.. dn:class:: GelConnection
    :no_link:

    Represents a class containing information on how to connect to a gel instance. 


    :property string Hostname:
        Gets the hostname of the gel instance to connect to. 

    :property int Port:
        Gets the port of the gel instance to connect to. 

        .. note::
            This property defaults to 5656 

    :property string Database:
        Gets the database name to use when connecting. 

        .. note::
            This property defaults to ``edgedb``. It is mutually exclusive with ``Gel.GelConnection.Branch``. 


    :property string Branch:
        Gets the branch name to use when connecting. 

        .. note::
            This property defaults to ``__default__``. It is mutually exclusive with ``Gel.GelConnection.Database``

    :property string Username:
        Gets the username used to connect to the database. 

        .. note::
            This property defaults to ``edgedb`` 

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


.. _Priority Levels: https://www.geldata.com/docs/reference/connection#ref-reference-connection-priority
.. _gel DSN: https://www.geldata.com/docs/reference/dsn

