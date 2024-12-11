namespace EdgeDB;

/// <summary>
///     Represents a generic object within EdgeDB.
/// </summary>
public sealed class EdgeDBObject
{
    /// <summary>
    ///     Constructs a new <see cref="EdgeDBObject" /> with the given data.
    /// </summary>
    /// <param name="data">The raw data for this object.</param>
    [EdgeDBDeserializer]
    internal EdgeDBObject(IDictionary<string, object?> data)
    {
        Id = (Guid)data["id"]!;
    }

    /// <summary>
    ///     Gets the unique identifier for this object.
    /// </summary>
    [EdgeDBProperty("id")]
    public Guid Id { get; }
}
