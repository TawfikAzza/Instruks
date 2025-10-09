using Application.DTOs;

namespace Application.Interfaces;

public interface IInstruksService
{
    Task<IEnumerable<InstruksDto>> GetAllAsync();
    Task<InstruksDto?> GetByIdAsync(Guid id);
    Task<InstruksDto> CreateAsync(InstruksDto dto);
    Task<bool> UpdateAsync(Guid id, InstruksDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<InstruksDto>> GetByCategoryAsync(Guid categoryId);
}