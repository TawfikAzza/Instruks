using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("order")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public ActionResult<List<OrderResultDTO>> GetAllOrders()
    {
        return Ok(_orderService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<OrderResultDTO> GetOrderById(int id)
    {
        try
        {
            return Ok(_orderService.GetById(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public ActionResult<OrderResultDTO> CreateOrder([FromBody] PostOrderDTO dto)
    {
        var created = _orderService.Create(dto);
        return Created($"order/{created.Id}", created);
    }

    [HttpPut("{id}")]
    public ActionResult<OrderResultDTO> UpdateOrder(int id, [FromBody] UpdateOrderDTO dto)
    {
        if (id != dto.Id)
            return BadRequest("ID in route and body must match.");

        try
        {
            return Ok(_orderService.Update(dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteOrder(int id)
    {
        try
        {
            _orderService.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}