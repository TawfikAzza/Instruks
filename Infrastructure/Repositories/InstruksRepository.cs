using Application.Interfaces.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class InstruksRepository : IInstruksRepository
{
    private readonly AppDbContext _context;

    public InstruksRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Instruks>> GetAllAsync() =>
        await _context.InstruksTable
            .OrderByDescending(i => i.UpdatedAt)
            .ToListAsync();

    public async Task<Instruks?> GetByIdAsync(Guid id) =>
        await _context.InstruksTable.FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(Instruks instruks)
    {
        _context.InstruksTable.Add(instruks);
        await _context.SaveChangesAsync();
    }

    public async Task<Instruks> UpdateAsync(Guid id, Instruks incoming)
    {
        var existing = await _context.InstruksTable
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existing == null)
            throw new KeyNotFoundException("Instruks not found");

        // Copy allowed fields (never touch Id / CreatedAt)
        existing.Title       = incoming.Title;
        existing.Description = incoming.Description;
        existing.Content     = incoming.Content;
        existing.CategoryId  = incoming.CategoryId;
        existing.UpdatedAt   = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Instruks instruks)
    {
        _context.InstruksTable.Remove(instruks);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Instruks>> GetByCategoryAsync(Guid categoryId)
    {
        return await _context.InstruksTable
            .Where(i => i.CategoryId == categoryId)
            .OrderByDescending(i => i.UpdatedAt)
            .ToListAsync();
    }
}