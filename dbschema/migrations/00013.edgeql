CREATE MIGRATION m1xgcfneyhgejgvqtk3e46njjnngnxdwqeq3jh5idjfjhjxppfv5ya
    ONTO m1qobo3k5z5dg56j3vymsaktqjnff5uv4ndpytjpqfwkda7lxinllq
{
  CREATE MODULE tests IF NOT EXISTS;
  CREATE TYPE tests::ScalarContainer {
      CREATE PROPERTY a: std::int16;
      CREATE PROPERTY b: std::int32;
      CREATE PROPERTY c: std::int64;
      CREATE PROPERTY d: std::str;
      CREATE PROPERTY e: std::bool;
      CREATE PROPERTY f: std::float32;
      CREATE PROPERTY g: std::float64;
      CREATE PROPERTY h: std::bigint;
      CREATE PROPERTY i: std::decimal;
      CREATE PROPERTY j: std::uuid;
      CREATE PROPERTY k: std::json;
      CREATE PROPERTY l: std::datetime;
      CREATE PROPERTY m: cal::local_datetime;
      CREATE PROPERTY n: cal::local_date;
      CREATE PROPERTY o: cal::local_time;
      CREATE PROPERTY p: std::duration;
      CREATE PROPERTY q: cal::relative_duration;
      CREATE PROPERTY r: cal::date_duration;
      CREATE PROPERTY s: std::bytes;
  };
};
