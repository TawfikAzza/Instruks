using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Security;
using AutoMapper;
using Domain;

namespace Application.Services;

/// <summary>
/// Application service for managing <see cref="Category"/> records used to group Instruks.
/// Read operations are available to Doctors and Nurses; mutating operations are limited to Doctors.
/// </summary>
public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Creates a new instance of <see cref="CategoryService"/>.
    /// </summary>
    /// <param name="repository">Category repository.</param>
    /// <param name="mapper">AutoMapper instance.</param>
    /// <param name="currentUser">Accessor for the currently authenticated user.</param>
    public CategoryService(
        ICategoryRepository repository,
        IMapper mapper,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Returns all categories.
    /// </summary>
    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    /// <summary>
    /// Returns a single category by identifier or <c>null</c> if not found.
    /// </summary>
    /// <param name="id">Category identifier.</param>
    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="dto">Payload for the new category.</param>
    /// <remarks>Only Doctors are allowed to create categories.</remarks>
    public async Task<CategoryDto> CreateAsync(CategoryDto dto)
    {
        // Defense-in-depth: controller policy should already restrict this.
        if (!_currentUser.IsDoctor)
            throw new UnauthorizedAccessException("Only doctors can create categories.");

        var entity = _mapper.Map<Category>(dto);
        entity.Id = Guid.NewGuid();

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync(); // Ensure persistence (was missing before)

        return _mapper.Map<CategoryDto>(entity);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">Category identifier.</param>
    /// <param name="dto">Updated payload.</param>
    /// <returns><c>true</c> when updated; <c>false</c> if not found.</returns>
    /// <remarks>Only Doctors are allowed to update categories.</remarks>
    public async Task<bool> UpdateAsync(Guid id, CategoryDto dto)
    {
        if (!_currentUser.IsDoctor)
            throw new UnauthorizedAccessException("Only doctors can update categories.");

        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        _mapper.Map(dto, entity);
        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync(); // Ensure persistence (was missing before)
        return true;
    }

    /// <summary>
    /// Deletes a category by identifier.
    /// </summary>
    /// <param name="id">Category identifier.</param>
    /// <returns><c>true</c> if deleted; otherwise <c>false</c> if not found.</returns>
    /// <remarks>Only Doctors are allowed to delete categories.</remarks>
    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!_currentUser.IsDoctor)
            throw new UnauthorizedAccessException("Only doctors can delete categories.");

        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        await _repository.DeleteAsync(entity);
        await _repository.SaveChangesAsync(); // Ensure persistence (was missing before)
        return true;
    }
}
