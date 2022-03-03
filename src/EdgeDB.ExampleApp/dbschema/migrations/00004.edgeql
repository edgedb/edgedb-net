CREATE MIGRATION m1g5un4nog22zfimjyzbn7b4f22phw4doinndcgp736t7t7c52lxwq
    ONTO m1mn5jztqujs34dpeybcaenl7rwx4xt6zw36zrsin3rwjhix4pj4fq
{
  CREATE TYPE default::Hobby {
      CREATE PROPERTY name -> std::str;
  };
  ALTER TYPE default::Person {
      CREATE MULTI LINK hobbies -> default::Hobby;
  };
};
