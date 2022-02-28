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
    };
}
