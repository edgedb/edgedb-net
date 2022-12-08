CREATE MIGRATION m1j4fessdh46ddxtfyk6xku2jbqpfh5kz3duiwao22byscf5u4nrrq
    ONTO m1cpqrhf5f5vdvux4bsgtljnq2sicll7vz4sfmjmztr5ilwgymzc5a
{
  CREATE TYPE default::c {
      CREATE REQUIRED PROPERTY c -> std::str;
  };
  CREATE TYPE default::d {
      CREATE REQUIRED PROPERTY d -> std::str;
  };
  CREATE TYPE default::a EXTENDING default::c, default::d {
      CREATE REQUIRED PROPERTY a -> std::str;
  };
  CREATE TYPE default::b EXTENDING default::c {
      CREATE REQUIRED PROPERTY b -> std::str;
  };
};
