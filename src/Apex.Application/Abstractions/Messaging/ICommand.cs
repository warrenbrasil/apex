namespace Apex.Application.Abstractions.Messaging;

/// <summary>
/// Represents a command with no return value.
/// Commands are used to modify state (Create, Update, Delete operations).
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Represents a command with a return value.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommand<out TResponse>
{
}
