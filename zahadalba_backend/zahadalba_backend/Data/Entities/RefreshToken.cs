namespace zahadalba_backend.Data.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime AbsoluteExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public RefreshToken(
            string tokenHash,
            Guid userId,
            DateTime createdAt,
            DateTime expiresAt,
            DateTime absoluteExpiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            TokenHash = tokenHash;
            IsRevoked = false;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
            AbsoluteExpiresAt = absoluteExpiresAt;
        }
    }
}
