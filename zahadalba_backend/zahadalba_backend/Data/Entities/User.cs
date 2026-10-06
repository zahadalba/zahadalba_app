namespace zahadalba_backend.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Role { get; set; } = "User";
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
