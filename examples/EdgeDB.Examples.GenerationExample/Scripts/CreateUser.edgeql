INSERT Person {
  name := <str>$name,
  email := <str>$email
}
UNLESS CONFLICT ON .email 
ELSE (SELECT Person)