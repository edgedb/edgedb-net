module default {
    abstract annotation `🍿`;
    abstract constraint `🚀🍿`(max: std::int64) extending std::max_len_value;
    function `💯`(named only `🙀`: std::int64) ->  std::int64 {
        volatility := 'Immutable';
        annotation default::`🍿` := 'fun!🚀';
        using (select
            (100 - `🙀`)
        )
    ;};
    abstract link movie_character {
        property character_name -> std::str;
    };
    scalar type Genre extending enum<Horror, Action, RomCom>;
    scalar type bag_seq extending std::sequence;
    scalar type مرحبا extending default::你好 {
        constraint default::`🚀🍿`(100);
    };
    scalar type 你好 extending std::str;
    scalar type `🚀🚀🚀` extending default::مرحبا;
    type A {
        required link `s p A m 🤞` -> default::`S p a M`;
    };
    type Bag extending default::HasName, default::HasAge {
        property bigintField -> std::bigint;
        property boolField -> std::bool;
        property datetimeField -> std::datetime;
        property decimalField -> std::decimal;
        property durationField -> std::duration;
        property enumArr -> array<default::Genre>;
        property float32Field -> std::float32;
        property float64Field -> std::float64;
        property genre -> default::Genre;
        property int16Field -> std::int16;
        property int32Field -> std::int32;
        property int64Field -> std::int64;
        property localDateField -> cal::local_date;
        property localDateTimeField -> cal::local_datetime;
        property localTimeField -> cal::local_time;
        property namedTuple -> tuple<x: std::str, y: std::int64>;
        property secret_identity -> std::str;
        property seqField -> default::bag_seq;
        multi property stringMultiArr -> array<std::str>;
        property stringsArr -> array<std::str>;
        required multi property stringsMulti -> std::str;
        property unnamedTuple -> tuple<std::str, std::int64>;
    };
    abstract type HasAge {
        property age -> std::int64;
    };
    abstract type HasName {
        property name -> std::str;
    };
    type Hero extending default::Person {
        multi link villains := (.<nemesis[is default::Villain]);
        property number_of_movies -> std::int64;
        property secret_identity -> std::str;
    };
    type Movie {
        multi link characters extending default::movie_character -> default::Person;
        link profile -> default::Profile {
            constraint std::exclusive;
        };
        property genre -> default::Genre;
        property rating -> std::float64;
        required property release_year -> std::int16 {
            default := (<std::int16>std::datetime_get(std::datetime_current(), 'year'));
        };
        required property title -> std::str {
            constraint std::exclusive;
        };
    };
    type MovieShape;
    abstract type Person {
        required property name -> std::str {
            constraint std::exclusive;
        };
    };
    type Profile {
        property plot_summary -> std::str;
    };
    type `S p a M` {
        property c100 := (select
            default::`💯`(`🙀` := .`🚀`)
        );
        required property `🚀` -> std::int32;
    };
    type Simple extending default::HasName, default::HasAge;
    type User {
        required link favourite_movie -> default::Movie;
        required property username -> std::str;
    };
    type Villain extending default::Person {
        link nemesis -> default::Hero;
    };
    type Łukasz {
        index on (.`Ł🤞`);
        link `Ł💯` -> default::A {
            property `🙀مرحبا🙀` -> default::مرحبا {
                constraint default::`🚀🍿`(200);
            };
            property `🙀🚀🚀🚀🙀` -> default::`🚀🚀🚀`;
        };
        required property `Ł🤞` -> default::`🚀🚀🚀` {
            default := (<default::`🚀🚀🚀`>'你好🤞');
        };
    };
};
module `💯💯💯` {
    function `🚀🙀🚀`(`🤞`: default::`🚀🚀🚀`) ->  default::`🚀🚀🚀` using (select
        <default::`🚀🚀🚀`>(`🤞` ++ 'Ł🙀')
    );
};