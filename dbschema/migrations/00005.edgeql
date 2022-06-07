CREATE MIGRATION m1rgfzvgm77nvwkjt5i4hw3ruxtxoed4osu2hv7uj6huagohxxuu2q
    ONTO m1xp6ukrooc4chjfnhmaky4ywsoip5wvbz4ffm36tsqosspiyqjy6q
{
  CREATE SCALAR TYPE default::State EXTENDING enum<NotStarted, InProgress, Complete>;
  CREATE TYPE default::TODO {
      CREATE REQUIRED PROPERTY date_created -> std::datetime;
      CREATE REQUIRED PROPERTY description -> std::str;
      CREATE REQUIRED PROPERTY state -> default::State;
      CREATE REQUIRED PROPERTY title -> std::str;
  };
};
