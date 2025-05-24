using System.ComponentModel.DataAnnotations;

namespace vrmo.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string? TwoFactorSecret { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime? GuestExpiresAt { get; set; }
    }
}
