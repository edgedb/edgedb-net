CREATE MIGRATION m1bthirxb7a7lq7mjtrdjsxdcjcy4or3tlprzh74azy44h7n3re6zq
    ONTO m1rgfzvgm77nvwkjt5i4hw3ruxtxoed4osu2hv7uj6huagohxxuu2q
{
  ALTER TYPE default::TODO {
      ALTER PROPERTY date_created {
          SET default := (std::datetime_current());
      };
  };
};
