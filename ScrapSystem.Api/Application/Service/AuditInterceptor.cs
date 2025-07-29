using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Application.Service.IServices;
using ScrapSystem.Api.Domain.Common;

namespace ScrapSystem.Api.Application.Service
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateAuditableEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableEntities(DbContext context)
        {
            var userId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : "0";

            var now = DateTime.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedId = Convert.ToInt32(userId);
                        entry.Entity.CreatedDate = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedId = Convert.ToInt32(userId);
                        entry.Entity.UpdatedDate = now;
                        break;
                }
            }
        }
    }
}
