using Domain;

namespace Application.Interfaces.Repositories;

public interface IInstruksRepository
{
    Task<List<Instruks>> GetAllAsync();
    Task<Instruks?> GetByIdAsync(Guid id);
    Task AddAsync(Instruks instruks);
    Task UpdateAsync(Instruks instruks);
    Task DeleteAsync(Instruks instruks);
}