
using zahadalba_backend.Data.Entities;

namespace zahadalba_backend.Data
{
    public class AppDbContext
    {
        public List<User> Users { get; } = new();
        public List<RefreshToken> RefreshTokens { get; } = new();
    }
}
