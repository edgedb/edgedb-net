CREATE MIGRATION m1wuplszy5bldaagiqypo6nsdwikppnvhtmkosmksmkl3eipujwloq
    ONTO m1yp62tfavybznmg6ywpi7xkbf77ue7ezaubpye7btyv2pco4okf7q
{
  CREATE TYPE default::UserWithSnowflakeId {
      CREATE REQUIRED PROPERTY user_id -> std::str;
      CREATE REQUIRED PROPERTY username -> std::str;
  };
};
