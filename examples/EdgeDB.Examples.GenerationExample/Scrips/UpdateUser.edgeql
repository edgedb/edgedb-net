WITH 
    new_name := <str>$name,
    new_email := <str>$email
UPDATE Person
FILTER .id = <uuid>$id
SET {
    name := new_name IF EXISTS new_name ELSE .name,
    email := new_email IF EXISTS new_email ELSE .email
}