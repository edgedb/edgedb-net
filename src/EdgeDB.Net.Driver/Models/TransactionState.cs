namespace EdgeDB;

/// <summary>
///     Represents the transaction state of the client.
/// </summary>
public enum TransactionState : byte
{
    /// <summary>
    ///     The client isn't in a transaction.
    /// </summary>
    NotInTransaction = 0x49,

    /// <summary>
    ///     The client is in a transaction.
    /// </summary>
    InTransaction = 0x54,

    /// <summary>
    ///     The client is in a failed transaction.
    /// </summary>
    InFailedTransaction = 0x45
}
