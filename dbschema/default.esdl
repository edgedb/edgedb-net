module default {
  type Movie {
    required property title -> str {
      constraint exclusive;
    }
    required property year -> int32;
    required link director -> Person;
    required multi link actors -> Person;
  }
  type Person {
    required property name -> str;
    required property email -> str {
      constraint exclusive;
    }
  }
}
