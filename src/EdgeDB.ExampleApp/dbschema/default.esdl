module default {
  type Person {
    property name -> str;
    property email -> str {
      constraint exclusive;
    }
    multi link hobbies -> Hobby;
    single link bestFriend -> Person;
  }
  type Hobby {
    property name -> str;
  }
}