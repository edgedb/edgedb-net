CREATE MIGRATION m1cpqrhf5f5vdvux4bsgtljnq2sicll7vz4sfmjmztr5ilwgymzc5a
    ONTO m1wuplszy5bldaagiqypo6nsdwikppnvhtmkosmksmkl3eipujwloq
{
  ALTER TYPE default::UserWithSnowflakeId {
      ALTER PROPERTY user_id {
          CREATE CONSTRAINT std::exclusive;
      };
  };
};
