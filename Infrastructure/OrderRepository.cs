using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context) {
        _context = context;
        DatabaseInitializer.EnsureCreated(_context);
    }
    
    

    public Order Create(Order order) {
        _context.OrderTable.Add(order);
        _context.SaveChanges();
        return order;
    }

    public Order Update(Order order) {
        _context.Attach(order);
        _context.Entry(order).State = EntityState.Modified;
        _context.SaveChanges();
        return order;
    }

    public void Delete(int id) {
        var order = _context.OrderTable.Include(o => o.products).FirstOrDefault(o => o.Id == id);
        if (order != null)
        {
            _context.OrderTable.Remove(order); // 👈 this will cascade delete products
            _context.SaveChanges();
        }
    }
    public void Detach(Order order)
    {
        _context.Entry(order).State = EntityState.Detached;
    }

    public Order GetById(int id) => _context.OrderTable
        .Include(o => o.products)
        .FirstOrDefault(o => o.Id == id);

    public List<Order> GetAll() => _context.OrderTable.Include(o => o.products).ToList();
}