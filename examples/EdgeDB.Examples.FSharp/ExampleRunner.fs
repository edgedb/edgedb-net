module ExampleRunner

open EdgeDB
open Examples
open System.Linq
open Microsoft.Extensions.Logging
open System

type ExampleRunner(client: EdgeDBClient, logger: ILogger<ExampleRunner>, factory: ILoggerFactory) =
    member this.Client = client
    member this.Logger = logger
    member this.Factory = factory

    member this.RunAsync() =
        task {
            // get all classes that implement IExample
            let examples =
                typeof<IExample>.Assembly
                    .GetTypes()
                    .Where(fun x -> x.IsAssignableTo(typeof<IExample>) && x <> typeof<IExample>)
                    .ToArray()

            for i = 0 to examples.Length - 1 do
                let example = examples[i]
                this.Logger.LogInformation("Running {example}..", $"{example.Name}.fs")

                try
                    let inst = Activator.CreateInstance(example) :?> IExample

                    let! _ = inst.ExecuteAsync(this.Client, this.Factory.CreateLogger(example.Name))

                    this.Logger.LogInformation("{example} complete!", $"{example.Name}.fs")

                with ex ->
                    this.Logger.LogError(ex, "Failed to run {example}", $"{example.Name}.fs")
        }
