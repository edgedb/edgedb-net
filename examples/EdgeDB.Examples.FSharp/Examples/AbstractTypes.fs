namespace Examples

open EdgeDB
open Microsoft.Extensions.Logging
open System.Linq

type Thing = { Name: string; Description: string }

type OtherThing = { Name: string; Attribute: string }

type AbstractThing =
    | Thing of Thing
    | OtherThing of OtherThing

type AbstractTypesExample() =
    interface IExample with
        member this.ExecuteAsync(client: EdgeDBClient, logger: ILogger) =
            task {
                // select the abstract type from the schema.
                // Note that the type builder will 'discover' the types that inherit
                // our F# union type.
                let! result =
                    client.QueryAsync<AbstractThing>("select AbstractThing { name }")
                    |> Async.AwaitTask

                // select only 'Thing' types
                let things =
                    result.Where(fun x ->
                        match x with
                        | Thing _ -> true
                        | _ -> false)

                // select only 'OtherThing' types
                let otherThings =
                    result.Where(fun x ->
                        match x with
                        | OtherThing _ -> true
                        | _ -> false)

                let isResult =
                    client.QueryAsync<AbstractThing>(
                        "select AbstractThing { name, [is Thing].description, [is OtherThing].attribute }"
                    )

                isResult |> ignore
            }
