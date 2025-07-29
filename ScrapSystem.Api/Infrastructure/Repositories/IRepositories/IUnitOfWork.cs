
using Microsoft.EntityFrameworkCore.Storage;
using ScrapSystem.Api.Infrastructure.Repositories;
using ScrapSystem.Api.Infrastructure.Repositories.IRepositories;

namespace ScrapSystem.Api.Data.Repositories.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        IScrapRepository ScrapRepository { get; }
        IScrapDetailRepository ScrapDetailRepository { get; }
        IMaterialNameRepository MaterialNameRepository { get; }
        IImageScrapRepository ImageScrapRepository { get; }
        IUserRefreshTokenRepository UserRefreshTokenRepository { get; }
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<IExecutionStrategy> CreateExecutionStrategy();
    }
}