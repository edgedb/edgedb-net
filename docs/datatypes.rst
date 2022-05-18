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
    [EdgeDBProperty("name")]
    public string? Name { get; set; }
    
    [EdgeDBProperty("email")]
    public string? Email { get; set; }
  }

.. note:: 

  Since the naming convention of properties is diffent from the EdgeDB naming convention,
  you can specify the name with the ``EdgeDBProperty`` attribute. You can also specify 
  the name of the type with the ``EdgeDBType`` attribute.

You can find an example with custom types `here <https://github.com/quinchs/EdgeDB.Net/blob/dev/examples/EdgeDB.ExampleApp/Examples/QueryResults.cs>`_