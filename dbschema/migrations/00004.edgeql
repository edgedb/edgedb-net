CREATE MIGRATION m1xp6ukrooc4chjfnhmaky4ywsoip5wvbz4ffm36tsqosspiyqjy6q
    ONTO m1tpouzupcrd2nkhl45qsdykw3dzjbk4ecndr73tmatxskaxkixu3q
{
  ALTER TYPE default::Movie {
      ALTER PROPERTY title {
          CREATE CONSTRAINT std::exclusive;
      };
  };
};
