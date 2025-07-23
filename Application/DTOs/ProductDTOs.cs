namespace Application.DTOs;

public class PostProductDTO
{
    public int Price { get; set; }
    public string Name { get; set; }
    public int OrderId { get; set; }
}

public class UpdateProductDTO
{
    public int Id { get; set; }
    public int Price { get; set; }
    public string Name { get; set; }
    public int OrderId { get; set; }
}

public class ProductResultDTO
{
    public int Id { get; set; }
    public int Price { get; set; }
    public string Name { get; set; }
    public int OrderId { get; set; }
}