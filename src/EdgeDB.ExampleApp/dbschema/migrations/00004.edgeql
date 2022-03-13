CREATE MIGRATION m1td32u3bhqhuw53jgsmubutmkuil5sguux4pwzhmmzu726rry5bpq
    ONTO m1ogyh446cja4uxhhgnk7bijemlmgke2kaqsybpvabrqdtspz27qwq
{
  ALTER TYPE default::Person {
      CREATE SINGLE LINK bestFriend -> default::Person;
  };
};
