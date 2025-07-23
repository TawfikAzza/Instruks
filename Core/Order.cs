namespace Domain;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public virtual List<Product> products { get; set; }
}