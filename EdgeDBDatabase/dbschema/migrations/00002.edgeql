CREATE MIGRATION m1thdvyg6kv6q7fqxwlkxmxgd32jthb22zkvnobupd53kwtdbfwo4a
    ONTO m1nisvd7hagzaecihp5qin4bb5ayyymmi5zhtpykr63og2xy7jjqiq
{
  ALTER TYPE default::Media {
      DROP LINK author;
      DROP PROPERTY hashtags;
      DROP PROPERTY post_date;
  };
  ALTER TYPE default::UploadedMedia {
      DROP PROPERTY uri;
  };
  DROP TYPE default::Photo;
  DROP TYPE default::Text;
  DROP TYPE default::Video;
  DROP TYPE default::UploadedMedia;
  DROP TYPE default::Media;
  DROP TYPE default::Service;
  ALTER TYPE default::User {
      DROP LINK follows;
      DROP PROPERTY email;
  };
  ALTER TYPE default::User RENAME TO default::Person;
  ALTER TYPE default::Person {
      CREATE PROPERTY email -> std::str;
      ALTER PROPERTY name {
          RESET OPTIONALITY;
      };
  };
};
