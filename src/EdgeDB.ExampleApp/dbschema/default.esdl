module default{
  type Person {
    property name -> str;
    property email -> str {
      constraint exclusive;
    }
  }
}