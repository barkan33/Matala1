using System.ComponentModel.DataAnnotations;

namespace Matala1.Models.Entities
{
    public interface IEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        public int? City_Code { get; set; }
    }
}
