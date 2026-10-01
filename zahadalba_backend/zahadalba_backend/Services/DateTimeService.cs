namespace zahadalba_backend.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow { get; } = DateTime.UtcNow;
    }
}
