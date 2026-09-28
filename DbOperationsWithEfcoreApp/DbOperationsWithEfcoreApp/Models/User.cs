namespace DbOperationsWithEfcoreApp.Models
{
    public class User
    {

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string passwordHash { get; set; } = string.Empty;

        public bool isActive { get; set; } = true;

        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        public string Role { get; set; } = "User";

    }
}
