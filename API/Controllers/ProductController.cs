using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("product")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public ActionResult<List<ProductResultDTO>> GetAllProducts()
    {
        return Ok(_productService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<ProductResultDTO> GetProductById(int id)
    {
        try
        {
            return Ok(_productService.GetById(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public ActionResult<ProductResultDTO> CreateProduct([FromBody] PostProductDTO dto)
    {
        try
        {
            var created = _productService.Create(dto);
            return Created($"product/{created.Id}", created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public ActionResult<ProductResultDTO> UpdateProduct(int id, [FromBody] UpdateProductDTO dto)
    {
        if (id != dto.Id)
            return BadRequest("ID in route and body must match.");

        try
        {
            return Ok(_productService.Update(dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        try
        {
            _productService.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}