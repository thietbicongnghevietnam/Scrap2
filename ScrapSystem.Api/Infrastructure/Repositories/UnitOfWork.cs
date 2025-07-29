using ScrapSystem.Api.Data.Repositories.IRepositories;
using ScrapSystem.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Infrastructure.Repositories.IRepositories;
using ScrapSystem.Api.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ScrapSystem.Api.Data.Repositories
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private readonly AppDbContext _context;
        private bool _disposed;

        private IUserRepository _userRepository;
        private IScrapRepository _scrapRepository;
        private IScrapDetailRepository _scrapDetailRepository;
        private IUserRefreshTokenRepository _userRefreshTokenRepository;
        private IMaterialNameRepository _materialNameRepository;
        private IImageScrapRepository _imageScrapRepository;


        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IUserRepository UserRepository
        {
            get
            {
                return _userRepository ??= new UserRepository(_context);
            }
        }

        public IUserRefreshTokenRepository UserRefreshTokenRepository
        {
            get
            {
                return _userRefreshTokenRepository ??= new UserRefreshTokenRepository(_context);
            }
        }

        public IScrapRepository ScrapRepository
        {
            get
            {
                return _scrapRepository ??= new ScrapRepository(_context);
            }
        }

        public IScrapDetailRepository ScrapDetailRepository
        {
            get
            {
                return _scrapDetailRepository ??= new ScrapDetailRepository(_context);
            }
        }

        public IMaterialNameRepository MaterialNameRepository
        {
            get
            {
                return _materialNameRepository ??= new MaterialNameRepository(_context);
            }
        }

        public IImageScrapRepository ImageScrapRepository
        {
            get
            {
                return _imageScrapRepository ??= new ImageScrapRepository(_context);
            }
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task<IExecutionStrategy> CreateExecutionStrategy()
        {
            return  _context.Database.CreateExecutionStrategy();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}