CREATE MIGRATION m1mn5jztqujs34dpeybcaenl7rwx4xt6zw36zrsin3rwjhix4pj4fq
    ONTO m1vqxxs4ie4so3x35nk2wyu3jy3hmolscpyxu6bccucaofjtnrtj3a
{
  CREATE SCALAR TYPE default::PersonNumber EXTENDING std::sequence;
  ALTER TYPE default::Person {
      CREATE PROPERTY number -> default::PersonNumber {
          CREATE CONSTRAINT std::exclusive;
      };
  };
};
