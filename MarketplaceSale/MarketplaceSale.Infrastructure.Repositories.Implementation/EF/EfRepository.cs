using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketplaceSale.Domain.Entities.Base;
using MarketplaceSale.Domain.Repositories.Abstractions;
using MarketplaceSale.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceSale.Infrastructure.Repositories.Implementation.EF
{
    public class EfRepository<TEntity, TId>(ApplicationDbContext context)
        : IRepository<TEntity, TId>
        where TEntity : Entity<TId>
        where TId : struct, IEquatable<TId>
    {
        protected readonly ApplicationDbContext _context = context;
        protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            bool asNoTracking = false)
        {
            var query = asNoTracking
                ? _dbSet.AsNoTracking()
                : _dbSet;

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<TEntity?> GetByIdAsync(
            TId id,
            CancellationToken cancellationToken,
            bool asNoTracking = false)
        {
            var query = asNoTracking
                ? _dbSet.AsNoTracking()
                : _dbSet;

            return await query.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            var entity = await GetByIdAsync(id, cancellationToken, asNoTracking: false);
            if (entity is null) return;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
