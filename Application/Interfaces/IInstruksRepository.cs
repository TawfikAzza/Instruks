using Domain;

namespace Application.Interfaces.Repositories;

public interface IInstruksRepository
{
    Task<List<Instruks>> GetAllLatestAsync();
    Task<List<Instruks>> GetByCategoryLatestAsync(Guid categoryId);
    Task<Instruks?> GetLatestByDocumentIdAsync(Guid documentId);
    Task<List<Instruks>> GetAllAsync();
    Task<Instruks?> GetByIdAsync(Guid id);
    Task AddAsync(Instruks instruks);
    Task UpdateAsync(Instruks entity);
    Task DeleteAsync(Instruks instruks);
    Task<IEnumerable<Instruks>> GetByCategoryAsync(Guid categoryId);
    Task SaveChangesAsync();
}