using DreamSoft.Application.Features.Customers.CreateCustomer;
using DreamSoft.Application.Features.Customers.DeleteCustomer;
using DreamSoft.Application.Features.Customers.GetCustomer;
using DreamSoft.Application.Features.Customers.GetCustomers;
using DreamSoft.Application.Features.Customers.Lookups;
using DreamSoft.Application.Features.Customers.UpdateCustomer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[Authorize(Policy = AuthPolicies.UserOnly)]
public class CustomerController : ApiControllerBase
{
    // ── GET /api/v1/customers ─────────────────────────────────────────────────

    /// <summary>
    /// Returns all active customers for the authenticated user's tenant and solution.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetCustomersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCustomersQuery(), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/customers/{id} ────────────────────────────────────────────

    /// <summary>
    /// Returns full detail for a single customer, including all related entity names.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetCustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCustomerQuery(id), cancellationToken);
        return Ok(result);
    }

    // ── POST /api/v1/customers ────────────────────────────────────────────────

    /// <summary>
    /// Creates a new customer scoped to the authenticated user's tenant and solution.
    /// At least one of Email, Phone, or Mobile must be provided.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateCustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/customers/{id} ────────────────────────────────────────────

    /// <summary>
    /// Fully updates an existing customer. All fields must be provided (PUT semantics).
    /// At least one of Email, Phone, or Mobile must be provided.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UpdateCustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCustomerRequest body,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            body.FirstName,
            body.LastName,
            body.CompanyName,
            body.CommercialName,
            body.ContactPerson,
            body.Email,
            body.Phone,
            body.Mobile,
            body.Website,
            body.TaxId,
            body.IdTypeId,
            body.TaxClassificationId,
            body.AddressLine1,
            body.AddressLine2,
            body.CountryId,
            body.ProvinceId,
            body.MunicipalityId,
            body.PostalCode,
            body.CreditLimit,
            body.PaymentTerms,
            body.DiscountPercentage,
            body.CurrencyId,
            body.CustomerCategory,
            body.CustomerStatusId,
            body.Notes);

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // ── DELETE /api/v1/customers/{id} ─────────────────────────────────────────

    /// <summary>
    /// Soft-deletes a customer (sets IsActive = false). The record is retained in the database.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCustomerCommand(id), cancellationToken);
        return NoContent();
    }

    // ── GET /api/v1/customers/lookups/statuses ────────────────────────────────

    /// <summary>
    /// Returns all active customer statuses (e.g. Active, Inactive, Blocked).
    /// </summary>
    [HttpGet("lookups/statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatuses(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCustomerStatusesQuery(), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/customers/lookups/types ───────────────────────────────────

    /// <summary>
    /// Returns all active customer types (e.g. Individual, Business).
    /// </summary>
    [HttpGet("lookups/types")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTypes(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCustomerTypesQuery(), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/customers/lookups/tax-classifications ─────────────────────

    /// <summary>
    /// Returns all active tax classifications with NCF type and RNC requirement info.
    /// </summary>
    [HttpGet("lookups/tax-classifications")]
    [ProducesResponseType(typeof(IReadOnlyList<TaxClassificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTaxClassifications(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTaxClassificationsQuery(), cancellationToken);
        return Ok(result);
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

/// <summary>Body for PUT /customers/{id}</summary>
public record UpdateCustomerRequest(
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string? CommercialName,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Mobile,
    string? Website,
    string? TaxId,
    int? IdTypeId,
    int? TaxClassificationId,
    string? AddressLine1,
    string? AddressLine2,
    int CountryId,
    int ProvinceId,
    int MunicipalityId,
    string? PostalCode,
    decimal CreditLimit,
    string? PaymentTerms,
    decimal DiscountPercentage,
    int CurrencyId,
    string? CustomerCategory,
    int CustomerStatusId,
    string? Notes);
