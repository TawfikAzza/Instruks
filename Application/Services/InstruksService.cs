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
        var docId = Guid.NewGuid();
        var entity = _mapper.Map<Instruks>(dto);
        entity.Id = Guid.NewGuid();
        entity.DocumentId = docId;
        entity.VersionNumber = 1;
        entity.IsLatest = true;
        entity.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InstruksDto>(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, InstruksDto dto)
    {
        var current = await _repository.GetByIdAsync(id);
        if (current is null || !current.IsLatest) return false;

        // modify in place (no bump)
        current.Title = dto.Title;
        current.Description = dto.Description;
        current.Content = dto.Content;
        current.CategoryId = dto.CategoryId;
        current.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(current);
        await _repository.SaveChangesAsync();
        return true;
    }
    public async Task<InstruksDto?> CreateNewVersionAsync(Guid id, InstruksDto dto)
    {
        var current = await _repository.GetByIdAsync(id);
        if (current is null) return null;

        // demote current
        current.IsLatest = false;
        await _repository.UpdateAsync(current);

        // create new version
        var next = new Instruks
        {
            Id = Guid.NewGuid(),
            DocumentId = current.DocumentId,
            VersionNumber = current.VersionNumber + 1,
            IsLatest = true,
            PreviousVersionId = current.Id,
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(next);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InstruksDto>(next);
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        await _repository.DeleteAsync(entity);
        return true;
    }
}