using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstruksController : ControllerBase
{
    private readonly IInstruksService _instruksService;
    private readonly IInstruksPdfService _pdfService;
    public InstruksController(IInstruksService instruksService,IInstruksPdfService pdfService)
    {
        _instruksService = instruksService;
        _pdfService = pdfService;
    }

    // GET: api/instruks
    [HttpGet]
    [Authorize] // Doctor and Nurse can read
    public async Task<IActionResult> GetAll()
    {
        var items = await _instruksService.GetAllAsync();
        return Ok(items);
    }
    // API/Controllers/InstruksController.cs
    [HttpGet("by-category/{categoryId:guid}")]
    [Authorize] // both Doctor & Nurse can read
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var items = await _instruksService.GetByCategoryAsync(categoryId);
        return Ok(items);
    }
    // GET: api/instruks/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _instruksService.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }

    // POST: api/instruks
    [HttpPost]
    [Authorize(Roles = "Doctor")] // Only doctors can create
    public async Task<IActionResult> Create([FromBody] InstruksDto dto)
    {
        var created = await _instruksService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/instruks/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Doctor")] // Only doctors can update
    public async Task<IActionResult> Update(Guid id, [FromBody] InstruksDto dto)
    {
        var success = await _instruksService.UpdateAsync(id, dto);
        if (!success)
            return NotFound();
        return NoContent();
    }

    // DELETE: api/instruks/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Doctor")] // Only doctors can delete
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _instruksService.DeleteAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
    
    // POST: api/instruks/{id}/version
    [HttpPost("{id}/version")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> CreateVersion(Guid id, [FromBody] InstruksDto dto)
    {
        var created = await _instruksService.CreateNewVersionAsync(id, dto);
        if (created is null) return NotFound();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    [HttpGet("{id:guid}/pdf")]
    [Authorize] // optional
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var bytes = await _pdfService.GeneratePdfAsync(id);
        var fileName = $"instruks-{id:N}.pdf";
        return File(bytes, "application/pdf", fileName);
    }
}