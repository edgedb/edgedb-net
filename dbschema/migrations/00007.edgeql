CREATE MIGRATION m1dntdma2rrziv35tbt7mfl7qeg3wpzaqk73edwaw5i4kkz5fg6z6a
    ONTO m1bthirxb7a7lq7mjtrdjsxdcjcy4or3tlprzh74azy44h7n3re6zq
{
  CREATE ABSTRACT TYPE default::AbstractThing {
      CREATE REQUIRED PROPERTY name -> std::str;
  };
  CREATE TYPE default::OtherThing EXTENDING default::AbstractThing {
      CREATE REQUIRED PROPERTY attribute -> std::str;
  };
  CREATE TYPE default::Thing EXTENDING default::AbstractThing {
      CREATE REQUIRED PROPERTY description -> std::str;
  };
};
