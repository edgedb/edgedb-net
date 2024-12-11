CREATE MIGRATION m1cpt5buyevyky24nrcoyjzfl67idj3uculwgo47zpxjnkb442knjq
    ONTO initial
{
  CREATE MODULE tests IF NOT EXISTS;
  CREATE GLOBAL default::abc -> tuple<std::str, std::int64>;
  CREATE GLOBAL default::current_user_id -> std::uuid;
  CREATE ABSTRACT TYPE default::AbstractThing {
      CREATE REQUIRED PROPERTY name: std::str {
          CREATE CONSTRAINT std::exclusive;
      };
  };
  CREATE TYPE default::OtherThing EXTENDING default::AbstractThing {
      CREATE REQUIRED PROPERTY attribute: std::str;
  };
  CREATE TYPE default::Thing EXTENDING default::AbstractThing {
      CREATE REQUIRED PROPERTY description: std::str;
  };
  CREATE TYPE default::Person {
      CREATE LINK best_friend: default::Person;
      CREATE MULTI LINK friends: default::Person;
      CREATE REQUIRED PROPERTY email: std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name: std::str;
  };
  CREATE TYPE default::Movie {
      CREATE REQUIRED MULTI LINK actors: default::Person;
      CREATE REQUIRED LINK director: default::Person;
      CREATE REQUIRED PROPERTY title: std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY year: std::int32;
  };
  CREATE SCALAR TYPE default::State EXTENDING enum<NotStarted, InProgress, Complete>;
  CREATE TYPE default::TODO {
      CREATE REQUIRED PROPERTY date_created: std::datetime {
          SET default := (std::datetime_current());
      };
      CREATE REQUIRED PROPERTY description: std::str;
      CREATE REQUIRED PROPERTY state: default::State;
      CREATE REQUIRED PROPERTY title: std::str;
  };
  CREATE TYPE default::UserWithSnowflakeId {
      CREATE REQUIRED PROPERTY user_id: std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY username: std::str;
  };
  CREATE TYPE tests::Person {
      CREATE LINK best_friend: tests::Person;
      CREATE MULTI LINK friends: tests::Person;
      CREATE PROPERTY age: std::int32;
      CREATE PROPERTY email: std::str;
      CREATE PROPERTY name: std::str;
  };
  CREATE TYPE tests::Club {
      CREATE MULTI LINK admins: tests::Person;
      CREATE MULTI LINK members: tests::Person;
      CREATE PROPERTY name: std::str;
  };
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
