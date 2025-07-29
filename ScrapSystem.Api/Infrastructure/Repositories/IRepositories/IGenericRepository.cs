using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ScrapSystem.Api.Data.Repositories.IRepositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<IReadOnlyList<TEntity>> GetAll();
        Task<bool> Exists(int id);
        Task<TEntity?> Get(int id);
        Task<TEntity> Add(TEntity entity);
        Task<List<TEntity>> AddMultiEntities(List<TEntity> entitys);
        Task<TEntity> Update(TEntity entity);
        Task<TEntity> Delete(TEntity entity);
        Task<List<T>> ExecuteStoredProcedureAsync<T>(
          string storedProcedureName,
          object parameters = null,
          CommandType commandType = CommandType.StoredProcedure);

        Task<(List<TItem>, TResult2)> ExecuteStoredProcedureAsync<TItem, TResult2>(
            string storedProcedureName,
            object parameters = null,
            CommandType commandType = CommandType.StoredProcedure);

        Task<(List<TItem1>, List<TItem2>)> ExecuteStoredProcedureMultiDataAsync<TItem1, TItem2>(
            string storedProcedureName,
            object parameters = null,
            CommandType commandType = CommandType.StoredProcedure);



    }
}
