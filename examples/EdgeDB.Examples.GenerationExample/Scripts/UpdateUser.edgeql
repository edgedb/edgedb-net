WITH 
    new_name := <optional str>$name,
    new_email := <optional str>$email
UPDATE Person
FILTER .id = <uuid>$id
SET {
    name := new_name IF EXISTS new_name ELSE .name,
    email := new_email IF EXISTS new_email ELSE .email
}