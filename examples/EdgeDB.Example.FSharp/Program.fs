open EdgeDB

type Person = {
    Name: string;
    Email: ValueOption<string>;
}

let client = new EdgeDBClient()

let result = task {
    let! people = client.QueryAsync<Person>("SELECT Person { name, email }");

    printf "%A" people
}

result |> Async.AwaitTask |> Async.RunSynchronously
