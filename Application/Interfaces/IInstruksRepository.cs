using Domain;

namespace Application.Interfaces.Repositories;

public interface IInstruksRepository
{
    Task<List<Instruks>> GetAllAsync();
    Task<Instruks?> GetByIdAsync(Guid id);
    Task AddAsync(Instruks instruks);
    Task<Instruks> UpdateAsync(Guid id, Instruks incoming);
    Task DeleteAsync(Instruks instruks);
    Task<IEnumerable<Instruks>> GetByCategoryAsync(Guid categoryId);
}