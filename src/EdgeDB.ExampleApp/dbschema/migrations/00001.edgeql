CREATE MIGRATION m1vrfklpftt2s6epq2vgccmk5gvb546dlwf3zdtwdqs4bodtfdwwna
    ONTO initial
{
  CREATE TYPE default::Account {
      CREATE REQUIRED PROPERTY username -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
  };
  CREATE ABSTRACT TYPE default::Content {
      CREATE REQUIRED PROPERTY title -> std::str;
  };
  ALTER TYPE default::Account {
      CREATE MULTI LINK watchlist -> default::Content;
  };
  CREATE TYPE default::Movie EXTENDING default::Content {
      CREATE PROPERTY release_year -> std::int32;
  };
  CREATE TYPE default::Show EXTENDING default::Content;
  CREATE TYPE default::Person {
      CREATE REQUIRED PROPERTY name -> std::str;
  };
  ALTER TYPE default::Content {
      CREATE MULTI LINK actors -> default::Person {
          CREATE PROPERTY character_name -> std::str;
      };
  };
  ALTER TYPE default::Person {
      CREATE LINK filmography := (.<actors[IS default::Content]);
  };
  CREATE TYPE default::Season {
      CREATE REQUIRED LINK show -> default::Show;
      CREATE REQUIRED PROPERTY number -> std::int32;
  };
  ALTER TYPE default::Show {
      CREATE PROPERTY num_seasons := (std::count(.<show[IS default::Season]));
  };
};
