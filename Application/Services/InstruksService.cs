using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain;
using Application.Interfaces.Security;
using Application.Interfaces.Content;
namespace Application.Services;

/// <summary>
/// Application service for managing <see cref="Instruks"/> documents,
/// including read operations, creation, updates, versioning, and deletion.
/// 
/// <para>
/// Authorization is primarily enforced at the controller level via policies,
/// but this service also performs defense-in-depth checks using <see cref="ICurrentUser"/>
/// before any mutating operation.
/// </para>
/// 
/// <para>
/// The HTML content of an Instruks is sanitized server-side using
/// <see cref="IHtmlSanitizerService"/> on create, update, and version creation.
/// </para>
/// </summary>
public sealed class InstruksService : IInstruksService
{
    private readonly IInstruksRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly IHtmlSanitizerService _htmlSanitizer;

    /// <summary>
    /// Creates a new instance of <see cref="InstruksService"/>.
    /// </summary>
    /// <param name="repository">Repository for persistence access.</param>
    /// <param name="mapper">AutoMapper instance for DTO/entity mapping.</param>
    /// <param name="currentUser">Accessor for the currently authenticated user.</param>
    /// <param name="htmlSanitizer">Server-side HTML sanitizer for rich content.</param>
    public InstruksService(
        IInstruksRepository repository,
        IMapper mapper,
        ICurrentUser currentUser,
        IHtmlSanitizerService htmlSanitizer)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _htmlSanitizer = htmlSanitizer;
    }

    /// <summary>
    /// Returns all latest Instruks (or all rows depending on repository behavior).
    /// </summary>
    public async Task<IEnumerable<InstruksDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return _mapper.Map<List<InstruksDto>>(items);
    }

    /// <summary>
    /// Returns Instruks filtered by category.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    public async Task<IEnumerable<InstruksDto>> GetByCategoryAsync(Guid categoryId)
    {
        var items = await _repository.GetByCategoryAsync(categoryId);
        return _mapper.Map<IEnumerable<InstruksDto>>(items);
    }

    /// <summary>
    /// Returns a single Instruks by identifier or <c>null</c> if not found.
    /// </summary>
    /// <param name="id">Instruks identifier.</param>
    public async Task<InstruksDto?> GetByIdAsync(Guid id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? null : _mapper.Map<InstruksDto>(item);
    }

    /// <summary>
    /// Creates a new Instruks (version 1 of a new document series).
    /// </summary>
    /// <param name="dto">Payload for the new Instruks.</param>
    /// <remarks>
    /// Only Doctors are allowed to create new Instruks.
    /// The HTML <see cref="Instruks.Content"/> is sanitized before storage.
    /// </remarks>
    public async Task<InstruksDto> CreateAsync(InstruksDto dto)
    {
        // Defense-in-depth: controller policy should already restrict this.
        if (!_currentUser.IsDoctor)
            throw new UnauthorizedAccessException("Only doctors can create Instruks.");

        var docId = Guid.NewGuid();
        var entity = _mapper.Map<Instruks>(dto);

        entity.Id = Guid.NewGuid();
        entity.DocumentId = docId;
        entity.VersionNumber = 1;
        entity.IsLatest = true;
        entity.CreatedAt = DateTime.UtcNow;

        // Server-side sanitize rich HTML content
        entity.Content = _htmlSanitizer.Sanitize(dto.Content);

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InstruksDto>(entity);
    }

    /// <summary>
    /// Updates an existing latest Instruks in place (does not increment version).
    /// </summary>
    /// <param name="id">The Instruks identifier to update.</param>
    /// <param name="dto">Updated payload.</param>
    /// <returns><c>true</c> when updated; <c>false</c> if not found or not latest.</returns>
    /// <remarks>
    /// Only Doctors are allowed to update Instruks.
    /// The HTML <see cref="Instruks.Content"/> is sanitized before storage.
    /// </remarks>
    public async Task<bool> UpdateAsync(Guid id, InstruksDto dto)
    {
        // Defense-in-depth
        if (!_currentUser.IsDoctor)
            throw new UnauthorizedAccessException("Only doctors can update Instruks.");

        var current = await _repository.GetByIdAsync(id);
        if (current is null || !current.IsLatest) return false;

        current.Title = dto.Title;
        current.Description = dto.Description;
        current.Content = _htmlSanitizer.Sanitize(dto.Content);
        current.CategoryId = dto.CategoryId;
        current.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(current);
        await _repository.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Creates a new version for an existing Instruks document (increments version and marks it as latest).
    /// </summary>
    /// <param name="id">The source Instruks identifier to version from.</param>
    /// <param name="dto">Payload for the new version.</param>
    /// <returns>The newly created version as DTO, or <c>null</c> if the source was not found.</returns>
    /// <remarks>
    /// Only Doctors are allowed to version Instruks.
    /// The HTML <see cref="Instruks.Content"/> is sanitized before storage.
    /// </remarks>
    public async Task<InstruksDto?> CreateNewVersionAsync(Guid id, InstruksDto dto)
    {
        // Defense-in-depth
        if (!_currentUser.IsDoctor)
            throw new UnauthorizedAccessException("Only doctors can create new versions.");

        var current = await _repository.GetByIdAsync(id);
        if (current is null) return null;

        // Demote current latest
        current.IsLatest = false;
        await _repository.UpdateAsync(current);

        var next = new Instruks
        {
            Id = Guid.NewGuid(),
            DocumentId = current.DocumentId,
            VersionNumber = current.VersionNumber + 1,
            IsLatest = true,
            PreviousVersionId = current.Id,
            Title = dto.Title,
            Description = dto.Description,
            Content = _htmlSanitizer.Sanitize(dto.Content),
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(next);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InstruksDto>(next);
    }

    /// <summary>
    /// Deletes an Instruks by identifier.
    /// </summary>
    /// <param name="id">Instruks identifier.</param>
    /// <returns><c>true</c> if deleted; otherwise <c>false</c> if not found.</returns>
    /// <remarks>Only Doctors are allowed to delete Instruks.</remarks>
    public async Task<bool> DeleteAsync(Guid id)
    {
        // Defense-in-depth
        if (!_currentUser.IsDoctor)
            throw new UnauthorizedAccessException("Only doctors can delete Instruks.");

        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        await _repository.DeleteAsync(entity);
        await _repository.SaveChangesAsync(); 
        return true;
    }
}
