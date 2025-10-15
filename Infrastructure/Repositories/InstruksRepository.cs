using Application.Interfaces.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Entity Framework Core repository for <see cref="Instruks"/> entities.
    /// <para>
    /// Provides read operations (with <c>AsNoTracking</c> where indicated) and
    /// basic CRUD methods. Note the save behavior:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="AddAsync(Instruks)"/> and <see cref="DeleteAsync(Instruks)"/> persist immediately by calling <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>.</description></item>
    ///   <item><description><see cref="UpdateAsync(Instruks)"/> does <strong>not</strong> save; callers should invoke <see cref="SaveChangesAsync"/>.</description></item>
    /// </list>
    /// </summary>
    public class InstruksRepository : IInstruksRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Creates a new <see cref="InstruksRepository"/>.
        /// </summary>
        /// <param name="context">The EF Core <see cref="AppDbContext"/>.</param>
        public InstruksRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns all <em>latest</em> versions of Instruks (rows where <see cref="Instruks.IsLatest"/> is <c>true</c>).
        /// </summary>
        /// <returns>A list of latest Instruks, not tracked by the change tracker.</returns>
        public Task<List<Instruks>> GetAllLatestAsync() =>
            _context.InstruksTable
                .Where(i => i.IsLatest)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// Returns all <em>latest</em> Instruks under a specific category.
        /// </summary>
        /// <param name="categoryId">Target category identifier.</param>
        /// <returns>A list of latest Instruks for the category, not tracked by the change tracker.</returns>
        public Task<List<Instruks>> GetByCategoryLatestAsync(Guid categoryId) =>
            _context.InstruksTable
                .Where(i => i.CategoryId == categoryId && i.IsLatest)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// Returns all Instruks (all versions), ordered by <see cref="Instruks.UpdatedAt"/> descending.
        /// </summary>
        /// <remarks>
        /// Returns tracked entities. If you only need read-only access, consider adding <c>AsNoTracking()</c>.
        /// </remarks>
        public async Task<List<Instruks>> GetAllAsync() =>
            await _context.InstruksTable
                .OrderByDescending(i => i.UpdatedAt)
                .ToListAsync();

        /// <summary>
        /// Retrieves a single Instruks by its identifier.
        /// </summary>
        /// <param name="id">Instruks identifier.</param>
        /// <returns>The entity if found; otherwise <c>null</c>.</returns>
        public async Task<Instruks?> GetByIdAsync(Guid id) =>
            await _context.InstruksTable.FirstOrDefaultAsync(x => x.Id == id);

        /// <summary>
        /// Retrieves the <em>latest</em> Instruks for a given document series.
        /// </summary>
        /// <param name="documentId">Stable document series identifier.</param>
        /// <returns>The latest version if found; otherwise <c>null</c>.</returns>
        public Task<Instruks?> GetLatestByDocumentIdAsync(Guid documentId) =>
            _context.InstruksTable.FirstOrDefaultAsync(i => i.DocumentId == documentId && i.IsLatest);

        /// <summary>
        /// Adds a new Instruks and saves changes immediately.
        /// </summary>
        /// <param name="instruks">The entity to add.</param>
        public async Task AddAsync(Instruks instruks)
        {
            _context.InstruksTable.Add(instruks);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Marks an Instruks as modified. Does <strong>not</strong> save changes.
        /// </summary>
        /// <param name="entity">The entity with updated values.</param>
        /// <returns>A task that completes when the entity is tracked as modified.</returns>
        /// <remarks>
        /// Call <see cref="SaveChangesAsync"/> afterward to persist the update.
        /// </remarks>
        public Task UpdateAsync(Instruks entity)
        {
            _context.InstruksTable.Update(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes an Instruks and saves changes immediately.
        /// </summary>
        /// <param name="instruks">The entity to remove.</param>
        public async Task DeleteAsync(Instruks instruks)
        {
            _context.InstruksTable.Remove(instruks);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns all <em>latest</em> Instruks for a category, ordered by <see cref="Instruks.UpdatedAt"/> descending.
        /// </summary>
        /// <param name="categoryId">Target category identifier.</param>
        public async Task<IEnumerable<Instruks>> GetByCategoryAsync(Guid categoryId)
        {
            return await _context.InstruksTable
                .Where(i => i.CategoryId == categoryId && i.IsLatest == true)
                .OrderByDescending(i => i.UpdatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves an Instruks by ID and includes its related <see cref="Domain.Category"/>.
        /// </summary>
        /// <param name="id">Instruks identifier.</param>
        /// <returns>The entity with its <see cref="Instruks.Category"/> populated, or <c>null</c> if not found.</returns>
        public async Task<Instruks?> GetWithCategoryAsync(Guid id)
        {
            return await _context.InstruksTable
                .AsNoTracking()
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        /// <summary>
        /// Persists any pending changes in the current <see cref="AppDbContext"/>.
        /// </summary>
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
