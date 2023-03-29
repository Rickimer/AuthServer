using AuthServer.DAL.Data.Models;
using System.Linq.Expressions;

namespace AuthServer.DAL.Data.Repository
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
    string includeProperties = "");

        IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "");

        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIDAsync(ulong id);
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task Delete(ulong id);
        Task Delete(TEntity entityToDelete);
    }
}
