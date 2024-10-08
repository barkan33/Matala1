﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Matala1.Models.Entities
{

    public class Student : IEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public short School_Year { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? Picture_URL { get; set; }
        [Required]
        public string? Address { get; set; }
        public int? City_Code { get; set; }
        public DateTime Enrollment { get; set; }

        public ICollection<StudentCourse>? StudentCourses { get; set; }
    }

}