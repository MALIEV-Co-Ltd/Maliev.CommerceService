using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CommerceService.Api.Authorization;
using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CommerceService.Api.Controllers;

/// <summary>
/// Product collection endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("commerce/v{version:apiVersion}/collections")]
public sealed class CollectionsController(ICommerceService commerceService) : ControllerBase
{
    private readonly ICommerceService _commerceService = commerceService;

    /// <summary>
    /// Lists published product collections.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CollectionResponse>>> ListCollections(CancellationToken cancellationToken)
    {
        var result = await _commerceService.ListCollectionsAsync(includeUnpublished: false, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lists all product collections for employee catalog management.
    /// </summary>
    [HttpGet("manage")]
    [RequirePermission(CommercePermissions.CollectionsRead)]
    public async Task<ActionResult<IReadOnlyList<CollectionResponse>>> ListManagedCollections(CancellationToken cancellationToken)
    {
        var result = await _commerceService.ListCollectionsAsync(includeUnpublished: true, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a collection by handle.
    /// </summary>
    [HttpGet("{handle}")]
    [AllowAnonymous]
    public async Task<ActionResult<CollectionResponse>> GetCollection(string handle, CancellationToken cancellationToken)
    {
        var result = await _commerceService.GetCollectionAsync(handle, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Creates a product collection.
    /// </summary>
    [HttpPost]
    [RequirePermission(CommercePermissions.CollectionsCreate)]
    public async Task<ActionResult<CollectionResponse>> CreateCollection([FromBody] CreateCollectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _commerceService.CreateCollectionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCollection), new { handle = result.Handle }, result);
    }

    /// <summary>
    /// Updates a product collection.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(CommercePermissions.CollectionsUpdate)]
    public async Task<ActionResult<CollectionResponse>> UpdateCollection(Guid id, [FromBody] UpdateCollectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _commerceService.UpdateCollectionAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Unpublishes a product collection.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(CommercePermissions.CollectionsDelete)]
    public async Task<IActionResult> UnpublishCollection(Guid id, CancellationToken cancellationToken)
    {
        var unpublished = await _commerceService.UnpublishCollectionAsync(id, cancellationToken);
        return unpublished ? NoContent() : NotFound();
    }
}
