using System.ComponentModel.DataAnnotations;
namespace UserService.Models
{
    public class UserProfile
    {
        [Key]
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}