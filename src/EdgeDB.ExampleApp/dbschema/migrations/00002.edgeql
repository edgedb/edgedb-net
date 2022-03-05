CREATE MIGRATION m1dyzwont7n7by4nrj7jm3tcozqqkzfzzyskdsgexetjheroiirlyq
    ONTO m1vrfklpftt2s6epq2vgccmk5gvb546dlwf3zdtwdqs4bodtfdwwna
{
  DROP TYPE default::Account;
  ALTER TYPE default::Content {
      ALTER LINK actors {
          DROP PROPERTY character_name;
      };
  };
  ALTER TYPE default::Person {
      DROP LINK filmography;
  };
  ALTER TYPE default::Content {
      DROP LINK actors;
      DROP PROPERTY title;
  };
  DROP TYPE default::Movie;
  ALTER TYPE default::Show {
      DROP PROPERTY num_seasons;
  };
  DROP TYPE default::Season;
  DROP TYPE default::Show;
  DROP TYPE default::Content;
  ALTER TYPE default::Person {
      CREATE PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
  };
  ALTER TYPE default::Person {
      ALTER PROPERTY name {
          RESET OPTIONALITY;
      };
  };
};
