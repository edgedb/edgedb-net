module default {
  global current_user_id -> uuid;
  global abc -> tuple<str, int64>;
  
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
    link best_friend -> Person;
    multi link friends -> Person;
  }

  # for example todo app
  scalar type State extending enum<NotStarted, InProgress, Complete>;
  type TODO {
    required property title -> str;
    required property description -> str;
    required property date_created -> std::datetime {
      default := std::datetime_current();
    }
    required property state -> State;
  }

  # for integration tests & examples
  abstract type AbstractThing {
    required property name -> str {
      constraint exclusive;
    }
  }
  type Thing extending AbstractThing {
    required property description -> str;
  }
  type OtherThing extending AbstractThing {
    required property attribute -> str;
  }

  # for type converter example
  type UserWithSnowflakeId {
    required property user_id -> str {
      constraint exclusive;
    }
    required property username -> str;
  }
}
