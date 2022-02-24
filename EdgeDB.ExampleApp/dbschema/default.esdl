module default {
	type Person {
        property email -> std::str {
            constraint exclusive;
        };
        property name -> std::str;
    };
}
