using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context)
    {
        _context = context;
        DatabaseInitializer.EnsureCreated(_context);
    } 

    public Product Create(Product product)
    {
        _context.ProductTable.Add(product);
        _context.SaveChanges();
        return product;
    }

    public Product Update(Product product)
    {
        _context.Attach(product);
        _context.Entry(product).State = EntityState.Modified;
        _context.SaveChanges();
        return product;
    }
    public void Detach(Product product)
    {
        _context.Entry(product).State = EntityState.Detached;
    }
    public void Delete(int id)
    {
        var product = _context.ProductTable.Find(id);
        if (product != null)
        {
            _context.ProductTable.Remove(product);
            _context.SaveChanges();
        }
    }

    public Product GetById(int id) => _context.ProductTable.Find(id);
    public List<Product> GetAll() => _context.ProductTable.ToList();
}