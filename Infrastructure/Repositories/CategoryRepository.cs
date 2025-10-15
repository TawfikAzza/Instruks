using Application.Interfaces.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Entity Framework Core repository for <see cref="Category"/> entities.
    /// <para>
    /// This repository performs basic CRUD operations against <c>AppDbContext.CategoryTable</c>.
    /// It persists changes immediately within each method by calling
    /// <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>.
    /// </para>
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Creates a new <see cref="CategoryRepository"/>.
        /// </summary>
        /// <param name="context">The EF Core <see cref="AppDbContext"/>.</param>
        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <returns>A list of <see cref="Category"/> entities.</returns>
        /// <remarks>
        /// Returns tracked entities. If you only need read-only access, consider
        /// using <c>AsNoTracking()</c> to avoid change tracking overhead.
        /// </remarks>
        public async Task<List<Category>> GetAllAsync() =>
            await _context.CategoryTable.ToListAsync();

        /// <summary>
        /// Retrieves a category by its identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>
        /// The <see cref="Category"/> if found; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        /// Uses <see cref="DbSet{TEntity}.FindAsync(object?[])"/> which may return a cached entity if it is already tracked.
        /// </remarks>
        public async Task<Category?> GetByIdAsync(Guid id) =>
            await _context.CategoryTable.FindAsync(id);

        /// <summary>
        /// Adds a new category and saves changes.
        /// </summary>
        /// <param name="category">The category to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(Category category)
        {
            _context.CategoryTable.Add(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing category and saves changes.
        /// </summary>
        /// <param name="category">The category with updated values.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// The entity must be attached to the context or have a valid key; otherwise EF will attach it.
        /// </remarks>
        public async Task UpdateAsync(Category category)
        {
            _context.CategoryTable.Update(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a category and saves changes.
        /// </summary>
        /// <param name="category">The category to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(Category category)
        {
            _context.CategoryTable.Remove(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Persists any pending changes in the current <see cref="AppDbContext"/>.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
