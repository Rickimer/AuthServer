using AuthServer.DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AuthServer.DAL.Data.Repository
{
    public abstract class GeneralRepository<TEntity, TContext> : IRepository<TEntity>, IDisposable
            where TEntity : class, IEntity
            where TContext : DbContext
    {
        private readonly TContext context;
        private readonly DbSet<TEntity> dbSet;

        public GeneralRepository(TContext context)
        {
            this.context = context;
            dbSet = context.Set<TEntity>();
        }
        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            entity.Created = DateTime.UtcNow;
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task Delete(ulong id)
        {
            var entity = await dbSet.FindAsync(id);
            if (entity != null)
            {
                await Delete(entity);
            }
        }

        public virtual async Task Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
            await context.SaveChangesAsync();
        }

        public virtual IQueryable<TEntity> GetQuery(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>>? orderBy = null, string includeProperties = "")
        {
            var query = GetQuery(filter = null, orderBy, includeProperties);
            return await query.ToListAsync();
        }

        public async Task<TEntity?> GetByIDAsync(ulong id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return entity;
        }


        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
