using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matala1.Models.Entities
{
    public class Lecturer : IEntity
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
        [EmailAddress]
        public string Email { get; set; }

        public string Academic_Degree { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Start_Date { get; set; }

        public string? Address { get; set; }

        public int? City_Code { get; set; }
        public ICollection<Course>? Courses { get; set; }
    }
}