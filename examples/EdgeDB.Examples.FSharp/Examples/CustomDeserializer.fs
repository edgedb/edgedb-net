module CustomDeserializer

open Examples
open EdgeDB
open Microsoft.Extensions.Logging
open System.Threading.Tasks
open System.Threading
open System
open System.Collections.Generic

type PersonConstructor() =
    let mutable name = ""
    let mutable email = ""
    member this.Name with get() = name and set(v) = name <- v
    member this.Email with get() = email and set(v) = email <- v

    [<EdgeDBDeserializer()>]
    new(raw: IDictionary<string, obj>) as this =
        PersonConstructor()
        then
            this.Name <- raw.["name"] :?> string
            this.Email <- raw.["email"] :?> string

type PersonMethod() =
    member val Name = "" with get, set
    member val Email = "" with get, set

    [<EdgeDBDeserializer()>]
    member this.Deserialize(raw: IDictionary<string, obj>) =
        this.Name <- raw.["name"] :?> string
        this.Email <- raw.["email"] :?> string

type PersonGlobal() =
    member val Name = String.Empty with get, set
    member val Email = String.Empty with get, set

type IPerson =
    abstract member Name: string with get, set
    abstract member Email: string with get, set

type PersonImpl() =
    member val Name = String.Empty with get, set
    member val Email = String.Empty with get, set

    interface IPerson with
        member self.Name with get() = self.Name and set v = self.Name <- v
        member self.Email with get() = self.Email and set v = self.Email <- v

type CustomDeserializer() =
    interface IExample with
        member this.ExecuteAsync(client: EdgeDBClient, logger: ILogger) =
            task {
                 // Define our queries
                let insertQuery = "insert Person { name := \"example\", email := \"example@example.com\" } unless conflict on .email";
                let selectQuery = "select Person { name, email } filter .email = \"example@example.com\"";

                // Insert john
                client.ExecuteAsync(insertQuery)
                |> Async.AwaitTask |> Async.Ignore |> ignore

                // Define a custom deserializer for the 'PersonGlobal' type
                TypeBuilder.AddOrUpdateTypeBuilder<PersonGlobal>(fun person data ->
                    logger.LogInformation("Custom deserializer was called")
                    person.Name <- data.["name"] :?> string
                    person.Email <- data.["email"] :?> string
                )

                // Define a custom creator for the 'PersonImmutable' type
                TypeBuilder.AddOrUpdateTypeFactory<IPerson>(fun enumerator ->
                    let data = enumerator.ToDynamic() :?> IDictionary<string, obj>

                    logger.LogInformation("Custom factory was called")

                    let person = PersonImpl()
                    person.Name <- data.["name"] :?> string
                    person.Email <- data.["email"] :?> string

                    person
                )

                let! exampleConstructor = client.QueryRequiredSingleAsync<PersonConstructor>(selectQuery)
                let! exampleMethod = client.QueryRequiredSingleAsync<PersonMethod>(selectQuery)
                let! exampleGlobal = client.QueryRequiredSingleAsync<PersonGlobal>(selectQuery)
                let! exampleInterface = client.QueryRequiredSingleAsync<IPerson>(selectQuery)

                logger.LogInformation("Constructor deserializer: {@Person}", exampleConstructor)
                logger.LogInformation("Method defined deserializer {@Person}", exampleMethod)
                logger.LogInformation("Globally defined deserializer: {@Person}", exampleGlobal)
                logger.LogInformation("Factory defined deserializer {@Person}", exampleInterface)
            }
