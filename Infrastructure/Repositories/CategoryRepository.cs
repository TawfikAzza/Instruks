using Application.Interfaces.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync() =>
        await _context.CategorieTable.ToListAsync();

    public async Task<Category?> GetByIdAsync(Guid id) =>
        await _context.CategorieTable.FindAsync(id);

    public async Task AddAsync(Category category)
    {
        _context.CategorieTable.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.CategorieTable.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _context.CategorieTable.Remove(category);
        await _context.SaveChangesAsync();
    }
}