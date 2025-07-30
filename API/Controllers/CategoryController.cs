using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: api/category
    [HttpGet]
    [Authorize] // Doctor and Nurse can read
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    // GET: api/category/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound();
        return Ok(category);
    }

    // POST: api/category
    [HttpPost]
    [Authorize(Roles = "Doctor")] // Only doctors can create
    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
    {
        var created = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/category/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Doctor")] // Only doctors can update
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryDto dto)
    {
        var success = await _categoryService.UpdateAsync(id, dto);
        if (!success)
            return NotFound();
        return NoContent();
    }

    // DELETE: api/category/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Doctor")] // Only doctors can delete
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _categoryService.DeleteAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
}