using Apex.Api.Models;
using Apex.Application.Customers.Commands.CreateCustomer;
using Apex.Application.Customers.DTOs;
using Apex.Application.Customers.Queries.GetCustomer;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Api.Controllers;

/// <summary>
/// Controller for customer operations.
/// </summary>
[Route("api/[controller]")]
public class CustomersController : ApiControllerBase
{
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ILogger<CustomersController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="request">Customer creation request.</param>
    /// <param name="commandHandler">Command handler (injected).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created customer.</returns>
    /// <response code="201">Customer created successfully.</response>
    /// <response code="400">Invalid request or validation error.</response>
    /// <response code="409">Customer already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        [FromServices] CreateCustomerCommandHandler commandHandler,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating customer with ApiId: {ApiId}, Document: {Document}, Company: {Company}",
            request.ApiId,
            request.Document,
            request.Company);

        var command = new CreateCustomerCommand(
            ApiId: request.ApiId,
            Document: request.Document,
            Company: request.Company,
            SinacorId: request.SinacorId,
            LegacyExternalId: request.LegacyExternalId);

        var result = await commandHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Failed to create customer. Error: {ErrorCode} - {ErrorMessage}",
                result.Error.Code,
                result.Error.Message);
        }
        else
        {
            _logger.LogInformation(
                "Customer created successfully with ID: {CustomerId}, ApiId: {ApiId}",
                result.Value.Id,
                result.Value.ApiId);
        }

        return ToCreatedAtActionResult(
            result,
            nameof(GetCustomer),
            new { id = result.IsSuccess ? result.Value.Id : 0 });
    }

    /// <summary>
    /// Gets a customer by ID.
    /// </summary>
    /// <param name="id">Customer ID.</param>
    /// <param name="queryHandler">Query handler (injected).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer if found.</returns>
    /// <response code="200">Customer found.</response>
    /// <response code="404">Customer not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(
        [FromRoute] int id,
        [FromServices] GetCustomerQueryHandler queryHandler,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting customer with ID: {CustomerId}", id);

        var query = new GetCustomerQuery(Id: id);
        var result = await queryHandler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Customer not found. ID: {CustomerId}, Error: {ErrorCode}",
                id,
                result.Error.Code);
        }
        else
        {
            _logger.LogInformation(
                "Customer found with ID: {CustomerId}, ApiId: {ApiId}",
                id,
                result.Value.ApiId);
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a customer by API ID.
    /// </summary>
    /// <param name="apiId">Customer API ID.</param>
    /// <param name="queryHandler">Query handler (injected).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer if found.</returns>
    /// <response code="200">Customer found.</response>
    /// <response code="404">Customer not found.</response>
    [HttpGet("by-api-id/{apiId}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerByApiId(
        [FromRoute] string apiId,
        [FromServices] GetCustomerQueryHandler queryHandler,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting customer with ApiId: {ApiId}", apiId);

        var query = new GetCustomerQuery(ApiId: apiId);
        var result = await queryHandler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Customer not found. ApiId: {ApiId}, Error: {ErrorCode}",
                apiId,
                result.Error.Code);
        }
        else
        {
            _logger.LogInformation(
                "Customer found with ApiId: {ApiId}, ID: {CustomerId}",
                apiId,
                result.Value.Id);
        }

        return ToActionResult(result);
    }
}
