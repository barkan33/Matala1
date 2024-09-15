using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matala1.Models.Entities
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public short Role_code { get; set; }
        [Required]
        public string Email { get; set; }
        [Phone]
        public string Phone { get; set; }
    }
    public class RoleCode
    {
        [Key]
        public short Role_code { get; set; }
        public string Role_Name { get; set; }
    }
}
