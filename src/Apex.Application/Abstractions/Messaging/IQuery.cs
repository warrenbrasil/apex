namespace Apex.Application.Abstractions.Messaging;

/// <summary>
/// Represents a query that returns data without modifying state.
/// Queries are used for read operations (Get, List, Search).
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQuery<out TResponse>
{
}
