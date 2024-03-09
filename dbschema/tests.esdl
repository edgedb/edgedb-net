module tests {
  type ScalarContainer {
    a: int16;
    b: int32;
    c: int64;
    d: str;
    e: bool;
    f: float32;
    g: float64;
    h: bigint;
    i: decimal;
    j: uuid;
    k: json;
    l: datetime;
    m: cal::local_datetime;
    n: cal::local_date;
    o: cal::local_time;
    p: duration;
    q: cal::relative_duration;
    r: cal::date_duration;
    s: bytes;
  }

  type Person {
    name: str;
    age: int32;
    email: str;
    best_friend: Person;
    multi friends: Person;
  }

  type Club {
    name: str;
    multi members: Person;
    multi admins: Person;
  }
}