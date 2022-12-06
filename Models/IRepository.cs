using System.Linq.Expressions;

namespace AuthServer.Models
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            string includeProperties = "");
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIDAsync(int id);
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task Delete(int id);
    }
}
