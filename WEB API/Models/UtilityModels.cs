using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Matala1.Models
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
    }
    public enum UserRole
    {
        Student = 1,
        Lecturer = 2,
        Staff = 3
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

}
