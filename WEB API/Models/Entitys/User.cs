using System.ComponentModel.DataAnnotations;

namespace Matala1.Models.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public short UserRole { get; set; }

    }
}
