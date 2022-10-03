.. _edgedb-dotnet-custom-types:

================
Custom Datatypes
================

The .NET driver supports custom type deserialization of query results with 
classes, records and structs. By default, all properties of these are mapped
with a one-to-one relation with the result of a query. This can be customized
via. the ``[EdgeDBProperty]`` attribute and ``EdgeDBClientConfig.SchemaNamingStrategy``.

.. _edgedb-dotnet-property-attribute:

Using Attributes
----------------

Much like the ``[JsonProperty]`` attribute in Newtonsoft.Json, the 
``[EdgeDBProperty]`` attribute can be used to customize the mapping of a
property to a results' property:

.. tabs::
  
  .. code-tab:: cs#CSharp
    
    public class Person
    {
        [EdgeDBProperty("name")]
        public string? Name { get; set; }

        [EdgeDBProperty("age")]
        public int Age { get; set; }
    }
  
  .. code-tab:: fs#FSharp
    
    type Person = {
      [<EdgeDBProperty("name")>]
      Name: string option
      
      [<EdgeDBProperty("age")>]
      Age: int
    }

``EdgeDBProperty`` takes in a positional argument for setting a name. This will
result in the above type mapping the object with lowercase ``name`` and
``age`` properties.

.. _edgedb-dotnet-naming-strategy:

Using a naming strategy
-----------------------

The ``EdgeDBClientConfig.SchemaNamingStrategy`` property is used to define the
naming strategy within schema. Changing this will implicitly convert all
property names to the new naming strategy:

.. tabs::
  
  .. code-tab:: cs#CSharp
    
    var config = new EdgeDBClientConfig
    {
        SchemaNamingStrategy = INamingStrategy.SnakeCase
    };

    var client = new EdgeDBClient(config);
  
  .. code-tab:: fs#FSharp
    
    let mutable config = new EdgeDBClientConfig()
    config.SchemaNamingStrategy <- INamingStrategy.SnakeCase

    let client = new EdgeDBClient(config)

Every property within your custom types will automatically have their property
names converted to the set to the naming strategy you pick, with ``snake_case``
in this example.

.. _edgedb-dotnet-polymorphism:

Polymorphic Types
-----------------

EdgeDB.Net supports polymorphic custom types, reflcting inheritance in EdgeDB.
When the result of a query is an interface or abstract class, the driver will
try to scan the assembly of the result for all types inheriting or implementing
such and deserialize it based on its name into the proper child.

It is very important the names of the implementing types to match those in the schema, 
if this isn't possible you could use the ``EdgeDBType`` attribute on the class to 
specify the name of the type in the schema.

.. tabs::

  .. code-tab:: cs#CSharp
    
    public abstract class Content
    {
        public string? Title { get; set; }
    }

    public class Movie : Content
    {
        public long ReleaseYear { get; set; }
    }

    public class TVShow : Content
    {
        public long Seasons { get; set; }
    }

    var content = await client.QueryAsync<Content>("SELECT Content");

    var shows = content.Where(x => x is TVShow).Cast<TVShow>();
    var movies = content.Where(x => x is Movie).Cast<Movie>();

  .. code-tab:: fs#FSharp

    type Content = {
      Title: string option
    }

    type Movie = {
      inherit Content
      ReleaseYear: int64
    }

    type TVShow = {
      inherit Content
      Seasons: int64
    }

    let content = client.QueryAsync<Content>("SELECT Content")

    let shows = content.Where(fun x -> x :? TVShow).Cast<TVShow>()
    let movies = content.Where(fun x -> x :? Movie).Cast<Movie>()

.. note:: 

  To implement custom behaviour for deserializing abstract/interface types, see
  :ref:`custom deserialization <edgedb-dotnet-custom-deserialization>`.


.. _edgedb-dotnet-custom-deserialization:

Custom Type Deserialization
---------------------------

You can add custom methods for deserializing types 