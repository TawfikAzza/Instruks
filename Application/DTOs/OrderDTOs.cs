namespace Application.DTOs;

public class PostOrderDTO
{
    public string OrderNumber { get; set; }
}

public class UpdateOrderDTO
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
}

public class OrderResultDTO
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public List<ProductResultDTO> Products { get; set; }
}