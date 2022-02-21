var net = require("net");
var tls = require("tls");

// var client = new net.Socket();
// client.connect(10701, "127.0.0.1", function () {
//     console.log("Connected");
//     client.write(
//         new Uint8Array([
//             // hello data
//             86, 0, 0, 0, 44, 0, 1, 0, 0, 0, 1, 0, 0, 0, 8, 100, 97, 116, 97, 98,
//             97, 115, 101, 0, 0, 0, 14, 69, 100, 103, 101, 68, 66, 68, 97, 116,
//             97, 98, 97, 115, 101, 0, 0, 0, 0,
//         ])
//     );
// });

const client = tls.connect(
    10701,
    "127.0.0.1",
    {
        rejectUnauthorized: false,
        strictSSL: false,
    },
    function () {
        // Check if the authorization worked
        if (client.authorized) {
            console.log("Connection authorized by a Certificate Authority.");
        } else {
            console.log(
                "Connection not authorized: " + client.authorizationError
            );
        }

        client.write(
            // hello data
            new Uint8Array([
                86, 0, 0, 0, 44, 0, 1, 0, 0, 0, 1, 0, 0, 0, 8, 100, 97, 116, 97,
                98, 97, 115, 101, 0, 0, 0, 14, 69, 100, 103, 101, 68, 66, 68,
                97, 116, 97, 98, 97, 115, 101, 0, 0, 0, 0,
            ])
        );
    }
);

client.on("data", function (data) {
    console.log("Received: " + data);
});

client.on("close", function () {
    console.log("Connection closed");
});

const edgedb = require("edgedb");

async function main() {
    const client = edgedb.createClient();

    console.log(
        await client.query(
            `select Person { name, email } filter .name = <str>$name;`,
            { name: "Quin" }
        )
    );
}

main();
