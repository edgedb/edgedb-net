.. _edgedb-dotnet-datatypes:

Datatypes
=========

The .NET driver maps the following EdgeDB types to the corresponding:

.. note:: 

  Currently, EdgeDB does not support unsigned numbers. Using them with the
  .NET driver may result in unexpected behaviour.

+------------------------------------+------------------------------------+
| EdgeDB Type                        | .NET Type                          |
+====================================+====================================+
| ``Set``                            | ``IEnumerable<T>``                 |
+------------------------------------+------------------------------------+
| :eql:type:`array`                  | ``T[]``                            |
+------------------------------------+------------------------------------+
| :eql:type:`anytuple`               | ``Tuple<T>``                       |
+------------------------------------+------------------------------------+
| :eql:type:`anyenum`                | ``Enum``                           |
+------------------------------------+------------------------------------+
| :eql:type:`Object`                 | ``object``                         |
+------------------------------------+------------------------------------+
| :eql:type:`bool`                   | ``bool``                           |
+------------------------------------+------------------------------------+
| :eql:type:`bytes`                  | ``byte[]``                         |
+------------------------------------+------------------------------------+
| :eql:type:`str`                    | ``string``                         |
+------------------------------------+------------------------------------+
| :eql:type:`cal::local_date`        | ``DataTypes.LocalData`` \*         |
+------------------------------------+------------------------------------+
| :eql:type:`cal::local_time`        | ``DataTypes.LocalTime`` \*         |
+------------------------------------+------------------------------------+
| :eql:type:`cal::local_datetime`    | ``DataTypes.LocalDateTime`` \*     |
+------------------------------------+------------------------------------+
| :eql:type:`cal::relative_duration` | ``DataTypes.RelativeDuration`` \*  |
+------------------------------------+------------------------------------+
| :eql:type:`datetime`               | ``DataTypes.DateTime`` \*          |
+------------------------------------+------------------------------------+
| :eql:type:`duration`               | ``DataTypes.Duration`` \*          |
+------------------------------------+------------------------------------+
| :eql:type:`float32`                | ``float``                          |
+------------------------------------+------------------------------------+
| :eql:type:`float64`                | ``double``                         |
+------------------------------------+------------------------------------+
| :eql:type:`int16`                  | ``short``                          |
+------------------------------------+------------------------------------+
| :eql:type:`int32`                  | ``int``                            |
+------------------------------------+------------------------------------+
| :eql:type:`int64`                  | ``long``                           |
+------------------------------------+------------------------------------+
| :eql:type:`bigint`                 | ``BigInt``                         |
+------------------------------------+------------------------------------+
| :eql:type:`decimal`                | ``decimal``                        |
+------------------------------------+------------------------------------+
| :eql:type:`json`                   | ``EdgeDB.DataTypes.Json``          |
+------------------------------------+------------------------------------+
| :eql:type:`uuid`                   | ``Guid``                           |
+------------------------------------+------------------------------------+
| :eql:func:`range`                  | ``EdgeDB.DataTypes.Range<T>`` \*\* |
+------------------------------------+------------------------------------+

\* These types are only available in >=1.0.5, previous versions use the 
.NET system date/time types. 

.NET system date/time types will be implicitly converted to the corresponding 
``DataTypes.*`` type, and vice versa. It is important to note that the precision 
of EdgeDB types is microseconds, while .NETs are nanoseconds. This means that 
when converting from EdgeDB to .NET, the precision will be lost. When converting 
from .NET to EdgeDB, the precision will be rounded to the nearest microsecond.
It's highly recommended to use the ``DataTypes.*`` types when working with temporals
to ensure the precision is not lost.

\*\* The ``System.Range`` type will be implicitly converted to ``range<int32>``. 
Note: since the .NET ``Range`` type only supports ``int32`` values, using any 
other EdgeDB range type that isn't ``int32`` will not work. This is only 
available in >=1.0.5