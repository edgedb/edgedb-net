CREATE MIGRATION m1vqxxs4ie4so3x35nk2wyu3jy3hmolscpyxu6bccucaofjtnrtj3a
    ONTO m1gajj2cjvikxjfhwnczhcfrcdlycg5pbg7vvehuaohazd2ltajlnq
{
  ALTER TYPE default::Person {
      ALTER PROPERTY email {
          CREATE CONSTRAINT std::exclusive;
      };
  };
};
