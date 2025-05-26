using System.ComponentModel.DataAnnotations;

namespace vrmo.Dto
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}
