CREATE MIGRATION m1yp62tfavybznmg6ywpi7xkbf77ue7ezaubpye7btyv2pco4okf7q
    ONTO m1isiclyxqa32luj6hdazr4mft2mvvq4tmmuoygl7m7k2iimxm5y3a
{
  CREATE GLOBAL default::current_user_id -> std::uuid;
};
