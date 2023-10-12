.. _edgedb-dotnet-transactions:

============
Transactions
============

Transactions allow you to execute transactional code via the ``TransactionAsync`` API, retrying your 
queries if a retryable error (e.g. a network failure) occurs. If an non-retryable error happens, 
the queries performed within the transactions are automatically rolled back:

.. tabs::

  .. code-tab:: cs
    :caption: C#

    var client = new EdgeDBClient();

    await client.TransactionAsync(async tx => 
        await tx.ExecuteAsync("INSERT User { name := 'John Smith' }");
    );

  .. code-tab:: fsharp
    :caption: F#

    let client = new EdgeDBClient()

    client.TransactionAsync(
      fun tx -> tx.ExecuteAsync("INSERT User { name := 'John Smith' }")
    ) |> Async.AwaitTask |> Async.Ignore |> ignore

.. note::
  Code blocks in transactions may run multiple times. It’s good practice to only perform safe to re-run operations in transaction blocks.

The ``TransactionAsync`` method proxies the result of the transaction, allowing you to 
get the result of a query executed in a transaction:

.. tabs::

  .. code-tab:: cs
    :caption: C#

    var client = new EdgeDBClient();

    var transactionResult = await client.TransactionAsync(async tx => 
        await tx.QueryRequiredSingleAsync<string>("SELECT 'Hello from Transaction!'");
    );

    Console.WriteLine(transactionResult);

  .. code-tab:: fsharp
    :caption: F#

    let client = new EdgeDBClient()

    client.TransactionAsync(
      fun tx -> tx.QueryRequiredSingleAsync<string>("SELECT 'Hello from Transaction!'")
    )
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> fun r -> printfn "Transaction result: %s" r

