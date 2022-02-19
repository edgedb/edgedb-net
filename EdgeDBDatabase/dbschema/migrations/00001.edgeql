CREATE MIGRATION m1nisvd7hagzaecihp5qin4bb5ayyymmi5zhtpykr63og2xy7jjqiq
    ONTO initial
{
  CREATE TYPE default::User {
      CREATE MULTI LINK follows -> default::User;
      CREATE REQUIRED PROPERTY email -> std::str {
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE REQUIRED PROPERTY name -> std::str;
  };
  CREATE ABSTRACT TYPE default::Media {
      CREATE REQUIRED LINK author -> default::User;
      CREATE MULTI PROPERTY hashtags -> std::str;
      CREATE REQUIRED PROPERTY post_date -> std::datetime {
          SET default := (std::datetime_current());
      };
  };
  CREATE ABSTRACT TYPE default::UploadedMedia EXTENDING default::Media {
      CREATE REQUIRED PROPERTY uri -> std::str;
  };
  CREATE TYPE default::Photo EXTENDING default::UploadedMedia;
  CREATE TYPE default::Text EXTENDING default::Media {
      CREATE REQUIRED PROPERTY body -> std::str;
      CREATE PROPERTY title -> std::str;
  };
  CREATE TYPE default::Video EXTENDING default::UploadedMedia;
  CREATE TYPE default::Service {
      CREATE REQUIRED PROPERTY cheap -> std::bool {
          SET default := false;
      };
      CREATE REQUIRED PROPERTY fast -> std::bool {
          SET default := false;
      };
      CREATE REQUIRED PROPERTY good -> std::bool {
          SET default := false;
      };
      CREATE CONSTRAINT std::expression ON (NOT (((.good AND .fast) AND .cheap)));
      CREATE REQUIRED PROPERTY name -> std::str;
  };
};
