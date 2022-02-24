CREATE MIGRATION m1gajj2cjvikxjfhwnczhcfrcdlycg5pbg7vvehuaohazd2ltajlnq
    ONTO initial
{
  CREATE TYPE default::Person {
      CREATE PROPERTY email -> std::str;
      CREATE PROPERTY name -> std::str;
  };
};
