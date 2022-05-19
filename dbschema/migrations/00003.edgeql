CREATE MIGRATION m1tpouzupcrd2nkhl45qsdykw3dzjbk4ecndr73tmatxskaxkixu3q
    ONTO m1vqxxs4ie4so3x35nk2wyu3jy3hmolscpyxu6bccucaofjtnrtj3a
{
  CREATE TYPE default::Movie {
      CREATE REQUIRED MULTI LINK actors -> default::Person;
      CREATE REQUIRED LINK director -> default::Person;
      CREATE REQUIRED PROPERTY title -> std::str;
      CREATE REQUIRED PROPERTY year -> std::int32;
  };
  ALTER TYPE default::Person {
      ALTER PROPERTY email {
          SET REQUIRED USING ('e');
      };
  };
  ALTER TYPE default::Person {
      ALTER PROPERTY name {
          SET REQUIRED USING ('e');
      };
  };
};
