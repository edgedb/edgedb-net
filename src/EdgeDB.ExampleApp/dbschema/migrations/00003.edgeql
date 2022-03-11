CREATE MIGRATION m1ogyh446cja4uxhhgnk7bijemlmgke2kaqsybpvabrqdtspz27qwq
    ONTO m1dyzwont7n7by4nrj7jm3tcozqqkzfzzyskdsgexetjheroiirlyq
{
  CREATE TYPE default::Hobby {
      CREATE PROPERTY name -> std::str;
  };
  ALTER TYPE default::Person {
      CREATE MULTI LINK hobbies -> default::Hobby;
  };
};
