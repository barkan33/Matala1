using Matala1.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Matala1.Models
{

    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CourseName { get; set; }

        [Required]
        public int LecturerId { get; set; }

        public string Classroom { get; set; }

        //[ForeignKey("LecturerId")]
        public Lecturer Lecturer { get; set; }

        public List<StudentCourses>? StudentCourses { get; set; } = new List<StudentCourses>();

        public List<Assignment> Assignments { get; set; } = new List<Assignment>();

        public List<Exam> Exams { get; set; } = new List<Exam>();
    }


    [PrimaryKey(nameof(StudentId), nameof(CourseId))]
    public class StudentCourses
    {
        [Required]
        public int StudentId { get; set; }
        [Required]
        public int CourseId { get; set; }

        public Course Course { get; set; }
    }

    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public bool IsVisible { get; set; } = true;

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        public List<StudentAssignments>? StudentAssignments { get; set; } = new List<StudentAssignments>();
    }

    [PrimaryKey(nameof(StudentId), nameof(AssignmentId))]
    public class StudentAssignments
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        public string SubmissionStatus { get; set; }

        public DateTime? SubmissionTimestamp { get; set; }

        public int? Grade { get; set; }

        public Student Student { get; set; }
        public Assignment Assignment { get; set; }
    }
    public class Exam
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }

        public string Description { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public List<StudentExamGrades> StudentExamGrades { get; set; } = new List<StudentExamGrades>();
    }

    [PrimaryKey(nameof(StudentId), nameof(ExamId))]
    public class StudentExamGrades
    {
        [Required]

        public int StudentId { get; set; }
        [Required]

        public int ExamId { get; set; }

        public int? Grade { get; set; }

        public Student Student { get; set; }
        public Exam Exam { get; set; }
    }
    public class City
    {
        [Key]
        public int CityCode { get; set; }
        public string CityName { get; set; }
    }



    public class Building
    {
        [Key]
        public int BuildingCode { get; set; }
        public string BuildingName { get; set; }
    }

    public class Room
    {
        [Key]
        public int RoomCode { get; set; }
        public string RoomName { get; set; }
        public Building Building { get; set; }
    }
    public class Department
    {
        [Key]
        public int DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public int? HODId { get; set; }
    }

    public class PasswordResetSys
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PasswordResetToken { get; set; }
        [Required]
        public DateTime PasswordResetTokenExpiration { get; set; }

    }

    // Login Model
    public class LoginModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    // Change Password Model
    public class ChangePasswordModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }

    public static class PasswordHelper
    {
        // Password Hashing
        public static byte[] HashPassword(string password)
        {
            MD5 md5 = MD5.Create();
            return md5.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public static bool VerifyPasswordHash(string password, byte[] passwordHash)
        {
            MD5 md5 = MD5.Create();
            byte[] computedHash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }

    public class PasswordResetRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public int Id { get; set; }
    }

    public class PasswordResetModel
    {
        [Required]
        public string ResetToken { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }


    public class UserButtonClicks
    {
        [Key]

        public int UserId { get; set; }
        [Required]

        public int ButtonId { get; set; }
        [Required]

        public int ClickCount { get; set; }
        [Required]

        public DateTime LastClickTimestamp { get; set; }
    }
    public class Button
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ButtonName { get; set; }

        [Required]
        public int DefaultSize { get; set; }

        [Required]
        public int MaxSize { get; set; }

        [Required]
        public decimal SizeFactor { get; set; }
    }
    public class ButtonSize
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public DateTime LastClickTimestamp { get; set; }
    }
}
