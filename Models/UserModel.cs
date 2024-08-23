using System.ComponentModel.DataAnnotations;

namespace gelirGiderTakip.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
        public IFormFile Image { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Register
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordCheck { get; set; }
        [Required]
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
