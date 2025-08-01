﻿namespace Domain;

public class Product {
    public int Id { get; set; }
    public int  Price { get; set; }
    public string Name { get; set; }
    public int OrderId { get; set; }
    public virtual Order? Order { get; set; }
}
