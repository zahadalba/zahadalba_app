using Microsoft.EntityFrameworkCore;
using zahadalba_backend.Data;

namespace zahadalba_backend.Services
{
    public class ExpiredTokensCleanerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromDays(1);
        public ExpiredTokensCleanerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var timer = new PeriodicTimer(_period);
            await DoWorkAsync(cancellationToken);

            while (await timer.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
            {
                await DoWorkAsync(cancellationToken);
            }
        }
        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var dateTimeService = scope.ServiceProvider.GetRequiredService<IDateTimeService>();
                int deletedCount = await dbContext.RefreshTokens
                    .Where(t => t.ExpiresAt < dateTimeService.UtcNow || t.AbsoluteExpiresAt < dateTimeService.UtcNow || t.IsRevoked)
                    .ExecuteDeleteAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
