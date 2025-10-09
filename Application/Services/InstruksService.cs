using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain;

namespace Application.Services;

public class InstruksService : IInstruksService
{
    private readonly IInstruksRepository _repository;
    private readonly IMapper _mapper;

    public InstruksService(IInstruksRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InstruksDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return _mapper.Map<List<InstruksDto>>(items);
    }
    // Application/Services/InstruksService.cs
    public async Task<IEnumerable<InstruksDto>> GetByCategoryAsync(Guid categoryId)
    {
        var q = await _repository.GetByCategoryAsync(categoryId);

        return _mapper.Map<IEnumerable<InstruksDto>>(q);
    }

    public async Task<InstruksDto?> GetByIdAsync(Guid id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? null : _mapper.Map<InstruksDto>(item);
    }

    public async Task<InstruksDto> CreateAsync(InstruksDto dto)
    {
        var entity = _mapper.Map<Instruks>(dto);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.AddAsync(entity);
        return _mapper.Map<InstruksDto>(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, InstruksDto dto)
    {
        var incoming = new Instruks
        {
            Title       = dto.Title,
            Description = dto.Description,
            Content     = dto.Content,
            CategoryId  = dto.CategoryId
        };

        var updated = await _repository.UpdateAsync(id, incoming);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        await _repository.DeleteAsync(entity);
        return true;
    }
}