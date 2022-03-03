module default {
    scalar type PersonNumber extending sequence;

	type Person {
        property email -> std::str {
            constraint exclusive;
        };
        property name -> std::str;
        property number -> PersonNumber {
            constraint exclusive;        
        }

        multi link hobbies -> Hobby;
    };

    type Hobby {
        property name -> std::str;
    }
}
