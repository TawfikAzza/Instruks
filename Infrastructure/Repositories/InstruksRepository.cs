using Application.Interfaces.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class InstruksRepository : IInstruksRepository {
    private readonly AppDbContext _context;

    public InstruksRepository(AppDbContext context) {
        _context = context;
    }

    public async Task<List<Instruks>> GetAllAsync() =>
        await _context.InstruksTable.ToListAsync();

    public async Task<Instruks?> GetByIdAsync(Guid id) =>
        await _context.InstruksTable.FindAsync(id);

    public async Task AddAsync(Instruks instruks) {
        _context.InstruksTable.Add(instruks);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Instruks instruks) {
        _context.InstruksTable.Update(instruks);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Instruks instruks) {
        _context.InstruksTable.Remove(instruks);
        await _context.SaveChangesAsync();
    }
}