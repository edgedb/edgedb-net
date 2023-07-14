.. _edgedb-dotnet-custom-types:

================
Custom Datatypes
================

Custom types are deserializable from a query's results by either using classes,
records or structs. By default, all properties are mapped with a one-to-one
relationship to the result of any given query.

.. _edgedb-dotnet-property-attribute:

Using attributes
----------------

Much like the ``[JsonProperty]`` attribute in Newtonsoft.Json, the 
``[EdgeDBProperty]`` attribute can be used to customize the mapping of a
property to a results' property name:

.. tabs::
  
  .. code-tab:: cs
    
    public class Person
    {
        [EdgeDBProperty("name")]
        public string? Name { get; set; }

        [EdgeDBProperty("age")]
        public int Age { get; set; }
    }
  
  .. code-tab:: fsharp
    
    type Person = {
      [<EdgeDBProperty("name")>]
      Name: string option
      
      [<EdgeDBProperty("age")>]
      Age: int
    }

.. _edgedb-dotnet-naming-strategy:

Using a naming strategy
-----------------------

Naming strategies can be forced in EdgeDB.Net by using the
``EdgeDBClientPoolConfig.SchemaNamingStrategy`` property. Changing its value will
result in all property names being implicitly converted to what is chosen:

Each property in a custom type will automatically have their property
names converted to the set to the naming strategy you pick, with ``snake_case``
in this example.

.. tabs::
  
  .. code-tab:: cs
    
    var config = new EdgeDBClientPoolConfig
    {
        SchemaNamingStrategy = INamingStrategy.SnakeCase
    };

    var client = new EdgeDBClient(config);
  
  .. code-tab:: fsharp
    
    let config = EdgeDBClientPoolConfig(
      SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy
    )

    let client = EdgeDBClient(config)

.. _edgedb-dotnet-polymorphism:

Polymorphic types
-----------------

.. This is oddly worded. Last sentence could use better wording.

EdgeDB.Net supports polymorphic custom types, reflecting inheritance found in
EdgeDB. When the return type of a query is an interface or abstract class,
EdgeDB.Net will try and scan the assembly of a result for all types
inheriting or implementing the return type, and deserialize them into
children based off of their parent.

It's very important to note that the names of types implemented must match
those found in a schema. If this isn't possible, try using the ``EdgeDBType``
attribute on a class instead for specification.

.. note:: 

  To implement custom behaviour for deserializing abstract/interface types, see
  :ref:`custom deserialization <edgedb-dotnet-custom-deserialization>`.

.. tabs::

  .. code-tab:: cs
    
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

  .. code-tab:: fsharp

    type Movie = {
        ReleaseYear: int
        Title: string
    }
    
    type TVShow = {
        Seasons: int64
        Title: string
    }
    
    type Content =
        | Movie of Movie
        | Show of Show

    let! content = client.QueryAsync<Content>("SELECT Content")

    let movies = content.Where(fun x -> match x with Movie -> true | _ -> false)
    let shows = content.Where(fun x -> match x with TVShow -> true | _ -> false)

.. _edgedb-dotnet-custom-deserialization:

Custom deserializers
--------------------

Custom methods and callbacks may be defined when trying to deserialize custom
types using the ``TypeBuilder`` class. These methods will be called once
EdgeDB.Net begins deserializing a user-defined type.

There are two ways to add custom deserialization methods: attributes and
callbacks. Both methods result in the same behaviour.

Attributes
^^^^^^^^^^

Methods and constructors can be marked with the ``[EdgeDBDeserializer]``
attribute, but only one may be applied per method.

The method or constructor must also take in a ``IDictionary<string, object?>``
type as its only parameter, as the dictionary contains all properties and their
values.

.. note:: 
  
  The keys of ``IDictionary`` are what's received from EdgeDB. The names
  of each key may not reflect properties found in the type - only 
  the names of fields returned from EdgeDB.

.. tabs::
  
  .. code-tab:: cs
    
    public class Person
    {
        public string? Name { get; set; }

        public int Age { get; set; }

        // constructor
        [EdgeDBDeserializer]
        public Person(IDictionary<string, object?> data)
        {
            Name = (string?)data["name"];
            Age = (int)data["age"]!;
        }

        // method
        [EdgeDBDeserializer]
        public void Deserialize(IDictionary<string, object?> data)
        {
            Name = (string?)data["name"];
            Age = (int)data["age"]!;
        }
    }

  .. code-tab:: fsharp

    type Person() =
        let mutable name = ""
        let mutable email = ""
        member this.Name with get() = name and set(v) = name <- v
        member this.Email with get() = email and set(v) = email <- v

        // constructor
        [<EdgeDBDeserializer()>]
        new(raw: IDictionary<string, obj>) as this =
            PersonConstructor()
            then
                this.Name <- raw.["name"] :?> string
                this.Email <- raw.["email"] :?> string

        // method
        [<EdgeDBDeserializer()>]
        member this.Deserialize(raw: IDictionary<string, obj>) =
            this.Name <- raw.["name"] :?> string
            this.Email <- raw.["email"] :?> string

.. note:: 

  Having both a method and a constructor with the ``EdgeDBDeserializer`` 
  attribute will not work. Your type will need to have at least one of either
  in order to work.

Callbacks
^^^^^^^^^

There are two different types of callbacks for building: factories and
builders. Factories are responsible for returning an implementation or instance
of the specified types, while builders are responsible for populating a given
instance.

.. tabs::
  
  .. code-tab:: cs
    
    public class Person
    {
        public string? Name { get; set; }

        public int Age { get; set; }
    }

    TypeBuilder.AddOrUpdateTypeBuilder<Person>((person, data) =>
    {
        person.Name = (string)data["name"]!;
        person.Email = (string)data["email"]!;
    });

    TypeBuilder.AddOrUpdateTypeFactory<Person>((ref ObjectEnumerator enumerator) =>
    {
        var data = (IDictionary<string, object?>)enumerator.ToDynamic()!;

        return new Person
        {
            Email = (string)data["email"]!,
            Name = (string)data["name"]!
        };
    });

  .. code-tab:: fsharp

    type Person() =
        member val Name = String.Empty with get, set
        member val Age = 0 with get, set

    TypeBuilder.AddOrUpdateTypeBuilder<Person>(fun person data -> 
        person.Name <- data.["name"] :?> string
        person.Email <- data.["age"] :?> int
    )

    TypeBuilder.AddOrUpdateTypeFactory<Person>(fun (ref ObjectEnumerator enumerator) ->
        let data = enumerator.ToDynamic()

        let person = new Person()

        person.Name <- data.["name"] :?> string
        person.Email <- data.["age"] :?> int
    )