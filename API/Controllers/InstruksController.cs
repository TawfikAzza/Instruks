using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

/// <summary>
/// CRUD and utilities for Instruks (procedural documents).
/// Doctors can create/update/delete; Doctors and Nurses can read and download PDFs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class InstruksController : ControllerBase
{
    private readonly IInstruksService _instruksService;
    private readonly IInstruksPdfService _pdfService;

    /// <summary>Creates a new <see cref="InstruksController"/>.</summary>
    public InstruksController(IInstruksService instruksService, IInstruksPdfService pdfService)
    {
        _instruksService = instruksService;
        _pdfService = pdfService;
    }

    /// <summary>Get all Instruks.</summary>
    /// <remarks>Accessible to Doctors and Nurses.</remarks>
    /// <response code="200">List of instruks.</response>
    [HttpGet]
    [Authorize(Policy = "CanReadInstruks")]
    [ProducesResponseType(typeof(IEnumerable<InstruksDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _instruksService.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Get Instruks by category.</summary>
    /// <remarks>Accessible to Doctors and Nurses.</remarks>
    /// <param name="categoryId">Category id.</param>
    /// <response code="200">List filtered by category.</response>
    [HttpGet("by-category/{categoryId:guid}")]
    [Authorize(Policy = "CanReadInstruks")]
    [ProducesResponseType(typeof(IEnumerable<InstruksDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var items = await _instruksService.GetByCategoryAsync(categoryId);
        return Ok(items);
    }

    /// <summary>Get a single Instruks by id.</summary>
    /// <remarks>Accessible to Doctors and Nurses.</remarks>
    /// <param name="id">Instruks id.</param>
    /// <response code="200">Instruks found.</response>
    /// <response code="404">Not found.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanReadInstruks")]
    [ProducesResponseType(typeof(InstruksDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _instruksService.GetByIdAsync(id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>Create a new Instruks.</summary>
    /// <remarks>Restricted to Doctors.</remarks>
    /// <param name="dto">Instruks payload (rich HTML content allowed; sanitized server-side).</param>
    /// <response code="201">Created; returns the created entity.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="403">Forbidden for non-Doctors.</response>
    [HttpPost]
    [Authorize(Policy = "CanManageInstruks")]
    [EnableRateLimiting("MutationLimiter")]
    [ProducesResponseType(typeof(InstruksDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] InstruksDto dto)
    {
        var created = await _instruksService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing Instruks.</summary>
    /// <remarks>Restricted to Doctors.</remarks>
    /// <param name="id">Instruks id.</param>
    /// <param name="dto">Updated values.</param>
    /// <response code="204">Updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="403">Forbidden for non-Doctors.</response>
    /// <response code="404">Instruks not found.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageInstruks")]
    [EnableRateLimiting("MutationLimiter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] InstruksDto dto)
    {
        var success = await _instruksService.UpdateAsync(id, dto);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>Delete an Instruks.</summary>
    /// <remarks>Restricted to Doctors.</remarks>
    /// <param name="id">Instruks id.</param>
    /// <response code="204">Deleted successfully.</response>
    /// <response code="403">Forbidden for non-Doctors.</response>
    /// <response code="404">Instruks not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanManageInstruks")]
    [EnableRateLimiting("MutationLimiter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _instruksService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>Create a new version of an existing Instruks.</summary>
    /// <remarks>
    /// Restricted to Doctors. Useful when you want to preserve history and publish a new version.
    /// </remarks>
    /// <param name="id">The source Instruks id.</param>
    /// <param name="dto">The new version payload.</param>
    /// <response code="201">Created; returns the created version.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="403">Forbidden for non-Doctors.</response>
    /// <response code="404">Source Instruks not found.</response>
    [HttpPost("{id:guid}/version")]
    [Authorize(Policy = "CanManageInstruks")]
    [EnableRateLimiting("MutationLimiter")]
    [ProducesResponseType(typeof(InstruksDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVersion(Guid id, [FromBody] InstruksDto dto)
    {
        var created = await _instruksService.CreateNewVersionAsync(id, dto);
        if (created is null)
            return NotFound();

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Download a PDF rendering of an Instruks.</summary>
    /// <remarks>Accessible to Doctors and Nurses.</remarks>
    /// <param name="id">Instruks id.</param>
    /// <response code="200">PDF binary stream.</response>
    /// <response code="404">Instruks not found or not renderable.</response>
    [HttpGet("{id:guid}/pdf")]
    [Authorize(Policy = "CanReadInstruks")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var bytes = await _pdfService.GeneratePdfAsync(id);
        if (bytes is null || bytes.Length == 0)
            return NotFound();

        var fileName = $"instruks-{id:N}.pdf";
        return File(bytes, "application/pdf", fileName);
    }
}
