CREATE MIGRATION m1isiclyxqa32luj6hdazr4mft2mvvq4tmmuoygl7m7k2iimxm5y3a
    ONTO m1dntdma2rrziv35tbt7mfl7qeg3wpzaqk73edwaw5i4kkz5fg6z6a
{
  ALTER TYPE default::AbstractThing {
      ALTER PROPERTY name {
          CREATE CONSTRAINT std::exclusive;
      };
  };
};
