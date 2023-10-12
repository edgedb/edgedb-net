﻿namespace EdgeDB;

/// <summary>
///     A enum containing the cardinality specification of a command.
/// </summary>
public enum Cardinality : byte
{
    /// <summary>
    ///     The command will return no result.
    /// </summary>
    NoResult = 0x6e,

    /// <summary>
    ///     The command will return at most one result.
    /// </summary>
    AtMostOne = 0x6f,

    /// <summary>
    ///     The command will return a single result.
    /// </summary>
    One = 0x41,

    /// <summary>
    ///     The command will return zero to infinite results.
    /// </summary>
    Many = 0x6d,

    /// <summary>
    ///     The command will return one to infinite results.
    /// </summary>
    AtLeastOne = 0x4d
}
