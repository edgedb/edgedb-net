CREATE MIGRATION m1wd74rplkqzjzyrfc56jt46tlenw6hgsbxgoahz6x2ks26wykiaca
    ONTO m1j4fessdh46ddxtfyk6xku2jbqpfh5kz3duiwao22byscf5u4nrrq
{
  CREATE TYPE default::e EXTENDING default::a {
      CREATE REQUIRED PROPERTY e -> std::str;
  };
};
