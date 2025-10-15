using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

/// <summary>
/// CRUD for categories used to organize Instruks.
/// Doctors can create, update, and delete. Doctors and Nurses can read.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    /// <summary>Creates a new <see cref="CategoryController"/>.</summary>
    public CategoryController(ICategoryService categoryService)
        => _categoryService = categoryService;

    /// <summary>Get all categories.</summary>
    /// <remarks>Accessible to Doctors and Nurses.</remarks>
    /// <response code="200">List of categories.</response>
    [HttpGet]
    [Authorize(Policy = "CanReadInstruks")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>Get a single category by id.</summary>
    /// <remarks>Accessible to Doctors and Nurses.</remarks>
    /// <param name="id">Category id.</param>
    /// <response code="200">Category found.</response>
    /// <response code="404">No category with the given id.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanReadInstruks")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    /// <summary>Create a new category.</summary>
    /// <remarks>Restricted to Doctors.</remarks>
    /// <param name="dto">Category payload.</param>
    /// <response code="201">Created; returns the created category.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="403">Forbidden for non-Doctors.</response>
    [HttpPost]
    [Authorize(Policy = "CanManageInstruks")]
    [EnableRateLimiting("MutationLimiter")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
    {
        var created = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing category.</summary>
    /// <remarks>Restricted to Doctors.</remarks>
    /// <param name="id">Category id.</param>
    /// <param name="dto">Updated values.</param>
    /// <response code="204">Updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="403">Forbidden for non-Doctors.</response>
    /// <response code="404">Category not found.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageInstruks")]
    [EnableRateLimiting("MutationLimiter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryDto dto)
    {
        var success = await _categoryService.UpdateAsync(id, dto);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>Delete a category.</summary>
    /// <remarks>Restricted to Doctors.</remarks>
    /// <param name="id">Category id.</param>
    /// <response code="204">Deleted successfully.</response>
    /// <response code="403">Forbidden for non-Doctors.</response>
    /// <response code="404">Category not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanManageInstruks")]
    [EnableRateLimiting("MutationLimiter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _categoryService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
