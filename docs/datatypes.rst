.. _edgedb-dotnet-datatypes:

Datatypes
=========

The dotnet driver maps the following edgedb types to the corresponding dotnet types:

+----------------------------+---------------------------+
| EdgeDB Type                | Python Type               |
+============================+===========================+
| ``Set``                    | ``IEnumberable<object>``  |
+----------------------------+---------------------------+
| ``array<anytype>``         | ``object[]``              |
+----------------------------+---------------------------+
| ``anytuple``               | ``Tuple<>``               |
+----------------------------+---------------------------+
| ``anyenum``                | ``Enum``                  |
+----------------------------+---------------------------+
| ``Object``                 | ``object``                |
+----------------------------+---------------------------+
| ``bool``                   | ``bool``                  |
+----------------------------+---------------------------+
| ``bytes``                  | ``byte[]``                |
+----------------------------+---------------------------+
| ``str``                    | ``string``                |
+----------------------------+---------------------------+
| ``cal::local_date``        | ``DateTime``\*            |
+----------------------------+---------------------------+
| ``cal::local_time``        | ``TimeSpan``              |
+----------------------------+---------------------------+
| ``cal::local_datetime``    | ``DateTime``\*            |
+----------------------------+---------------------------+
| ``cal::relative_duration`` | ``TimeSpan``              |
+----------------------------+---------------------------+
| ``datetime``               | ``DateTimeOffset``        |
+----------------------------+---------------------------+
| ``duration``               | ``TimeSpan``              |
+----------------------------+---------------------------+
|| ``float32``,              || ``float``                |
|| ``float64``               || ``double``               |
+----------------------------+---------------------------+
|| ``int16``,                || ``short``                |
|| ``int32``,                || ``int``                  |
|| ``int64``,                || ``long``                 |
|| ``bigint``                || ``BigInt``               |
+----------------------------+---------------------------+
| ``decimal``                | ``decimal``               |
+----------------------------+---------------------------+
| ``json``                   | ``EdgeDB.DataTypes.Json`` |
+----------------------------+---------------------------+
| ``uuid``                   | ``Guid``                  |
+----------------------------+---------------------------+

\* Both ``local_date`` and ``local_datetime`` will be deserialized 
as ``DateTime``. When serializing, the library will favor ``local_datetime``.


Custom datatypes
----------------
The dotnet driver can map schema defined types to dotnet classes. Take this person type as an example

.. code-block:: sdl

  type Person {
    property name -> str;
    property email -> str {
      constraint exclusive;
    }
  }

We can write at class that represents person as follows:

.. code-block:: c#

  public class Person
  {
    public string? Name { get; set; }
    
    public string? Email { get; set; }
  }

.. note:: 

  Since the naming convention of properties is diffent from the EdgeDB naming convention, 
  The class responsible for deserializing the schema type will use a ``INamingStrategy`` 
  to map the EdgeDB properties to the dotnet properties. You can change the default naming 
  strategy in the config. You can also specify the name with the ``EdgeDBProperty`` attribute. 
  You can also specify the name of the type with the ``EdgeDBType`` attribute.

You can find an example with custom types `here <https://github.com/quinchs/EdgeDB.Net/blob/dev/examples/EdgeDB.ExampleApp/Examples/QueryResults.cs>`_


Custom Deserializers
--------------------
You can specify how the Driver should deserialize your custom types.

Method-Based Deserializers
___________________________

You can decorate both methods and constructors with the ``EdgeDBDeserializer`` attribute to 
specify that the corresponding method should be used to deserialize the type. 

.. note:: Any method/constructor that is marked with the ``EdgeDBDeserializer`` attribute will be 
  ignored if the methods parameter is not of type ``IDictionary<string, object?>``

.. code-block:: c#

  public class PersonConstructor
  {
      public string Name { get; set; }
      public string Email { get; set; }

      [EdgeDBDeserializer]
      public PersonConstructor(IDictionary<string, object?> raw)
      {
          Name = (string)raw["name"]!;
          Email = (string)raw["email"]!;
      }
  }

.. code-block:: c#

  public class PersonMethod
  {
      public string? Name { get; set; }
      public string? Email { get; set; }

      [EdgeDBDeserializer]
      public void PersonBuilder(IDictionary<string, object?> raw)
      {
          Name = (string)raw["name"]!;
          Email = (string)raw["email"]!;
      }
  }

Global-Based Deserializers
__________________________
You can specify a callback to populate your type with the specified data like so:

.. code-block:: c#

  // Define a custom deserializer for the 'PersonGlobal' type
  TypeBuilder.AddOrUpdateTypeBuilder<PersonGlobal>((person, data) =>
  {
      Logger?.LogInformation("Custom deserializer was called");
      person.Name = (string)data["name"]!;
      person.Email = (string)data["email"]!;
  });

If you need to remove a custom deserializer, you can do so by calling the following:

.. code-block:: c#

  TypeBuilder.TryRemoveTypeFactory<PersonGlobal>(out var factory);

Interface-Based Deserializers
_____________________________
You can have interface-based deserializers. This is useful if you want to return different types based off of the returned data.

.. code-block:: c#

  // Define a custom creator for the 'PersonImmutable' type
  TypeBuilder.AddOrUpdateTypeFactory<IPerson>(data =>
  {
      Logger?.LogInformation("Custom factory was called");
      return new PersonImpl
      {
          Email = (string)data["email"]!,
          Name = (string)data["name"]!
      };
  });

The full source code for custom deserializers can be found `here <https://github.com/quinchs/EdgeDB.Net/blob/dev/examples/EdgeDB.ExampleApp/Examples/CustomDeserializer.cs#L10>`_