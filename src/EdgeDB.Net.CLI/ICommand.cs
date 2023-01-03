using Serilog;

namespace EdgeDB.CLI;

/// <summary>
///     Represents a generic command that can be executed.
/// </summary>
interface ICommand
{
    /// <summary>
    ///     Executes the command, awaiting its completion.
    /// </summary>
    /// <param name="logger">The logger for the command.</param>
    /// <returns>
    ///     A task that represents the execution flow of the command.
    /// </returns>
    Task ExecuteAsync(ILogger logger);
}