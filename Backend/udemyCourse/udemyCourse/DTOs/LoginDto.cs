using System.ComponentModel.DataAnnotations;

namespace udemyCourse.DTOs
{
    public class LoginDto
    {
        [Required]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
    }
}
