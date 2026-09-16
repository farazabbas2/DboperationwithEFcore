namespace Studentst.Models
{
    public class User
    {
        public int id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Plain password nahi, hash save hoga
        public string Role { get; set; } = "User"; // "Admin" ya "User"
    }
}