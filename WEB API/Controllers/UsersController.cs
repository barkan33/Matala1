using Matala1.Models;
using Matala1.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

/*
 
 {
    "id": 123123,
    "firstName": "Misha",
    "lastName": "kolomyza",
    "school_Year": 2,
    "phone": "0521236987",
    "email": "aaa@aaa.com",
    "picture_URL": "h",
    "address": "Stam",
    "city_Code": 1, 
    "enrollment": "2024-09-15T10:28:22.313Z"
  }

 {
    "id": 222,
    "firstName": "mmm",
    "lastName": "kkk",
    "school_Year": 2,
    "phone": "0521236937",
    "email": "bbb@bbb.com",
    "picture_URL": "h",
    "address": "Stam",
    "city_Code": 1, 
    "enrollment": "2024-09-15T10:28:22.313Z"
  }


*/

namespace Matala1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == model.Id);
                if (user == null)
                    return Unauthorized();
            }

            // Compare password hash
            if (!PasswordHelper.VerifyPasswordHash(model.Password, user.PasswordHash))
                return Unauthorized();

            // Authentication successful

            var result = new object();
            if (user.UserRole == 3)
                result = await _context.Set<Student>().FirstOrDefaultAsync(s => s.Id == model.Id);
            else if (user.UserRole == 2)
                result = await _context.Set<Lecturer>().FirstOrDefaultAsync(l => l.Id == model.Id);
            else if (user.UserRole == 1)
                result = await _context.Set<Staff>().FirstOrDefaultAsync(st => st.Id == model.Id);

            return Ok(result);
        }

        // Update User
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] User user)// How to use it
        {
            return NoContent();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw e;
                }
            }

            return NoContent();
        }

        // Change Password
        [HttpPut("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Id == model.Id);

            if (user == null || !PasswordHelper.VerifyPasswordHash(model.OldPassword, user.PasswordHash) || user.Email != model.Email)
                return BadRequest("One or More Fields Contain Incorrect Values");

            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw e;
                }
            }

            return Ok("Password updated successfully");
        }

        // Register Student
        [HttpPost("register/student")]
        public async Task<IActionResult> RegisterStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            byte[] passwordHash = PasswordHelper.HashPassword(student.Id.ToString());

            var user = new User
            {
                Id = student.Id,
                Email = student.Email,
                PasswordHash = passwordHash,
                UserRole = 3
            };

            _context.Set<User>().Add(user);
            await _context.SaveChangesAsync();

            _context.Set<Student>().Add(student);
            await _context.SaveChangesAsync();

            return Ok("Student registered successfully");
        }

        // Register Lecturer
        [HttpPost("register/lecturer")]
        public async Task<IActionResult> RegisterLecturer([FromBody] Lecturer lecturer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            byte[] passwordHash = PasswordHelper.HashPassword(lecturer.Id.ToString());

            var user = new User
            {
                Id = lecturer.Id,
                Email = lecturer.Email,
                PasswordHash = passwordHash,
                UserRole = 2
            };

            _context.Set<User>().Add(user);
            await _context.SaveChangesAsync();

            _context.Set<Lecturer>().Add(lecturer);
            await _context.SaveChangesAsync();

            return Ok("Lecturer registered successfully");
        }

        // Register Staff
        [HttpPost("register/staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] Staff staff)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            byte[] passwordHash = PasswordHelper.HashPassword(staff.Id.ToString());

            var user = new User
            {
                Id = staff.Id,
                Email = staff.Email,
                PasswordHash = passwordHash,
                UserRole = 1
            };

            _context.Set<User>().Add(user);
            await _context.SaveChangesAsync();


            _context.Set<Staff>().Add(staff);
            await _context.SaveChangesAsync();

            return Ok("Staff member registered successfully");
        }




        [HttpGet("getstudent/{id}")]
        [Authorize(Roles = "Admin, Staff, Student")] // Allow only Admins and Students
        public async Task<IActionResult> GetStudentById(int id)
        {
            if (!User.IsInRole("Admin") || !User.IsInRole("Student") || !User.IsInRole("Staff")) //if ConnectedUser.Token.GetId() != id
            {
                return Unauthorized("You are not authorized to access this student's data.");
            }


            var student = await _context.Set<Student>().FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }


            return Ok(student);
        }

        [HttpGet("getlecturer/{id}")]
        [Authorize(Roles = "Admin, Staff, Lecturer")] // Allow only Admins and Lecturer
        public async Task<IActionResult> GetLecturerById(int id)
        {
            if (!User.IsInRole("Admin") || !User.IsInRole("Lecturer") || !User.IsInRole("Staff")) //if ConnectedUser.Token.GetId() != id
            {
                return Unauthorized("You are not authorized to access this Lecturer's data.");
            }

            var lecturer = await _context.Set<Lecturer>().FirstOrDefaultAsync(l => l.Id == id);

            if (lecturer == null)
            {
                return NotFound();
            }

            return Ok(lecturer);
        }

        [HttpGet("getstaff/{id}")]
        [Authorize(Roles = "Admin, Staff")] // Allow only Admins and Staff
        public async Task<IActionResult> GetStaffById(int id)
        {

            if (!User.IsInRole("Admin") || !User.IsInRole("Staff")) //if ConnectedUser.Token.GetId() != id
            {
                return Unauthorized("You are not authorized to access this Staff's data.");
            }

            var Staff = await _context.Set<Staff>().FirstOrDefaultAsync(l => l.Id == id);

            if (Staff == null)
            {
                return NotFound();
            }


            return Ok(Staff);
        }

        // Update Student (Admin and Student Only)
        [HttpPut("updatestudent/{id}")]
        [Authorize(Roles = "Admin, Staff, Student")] // Allow only Admins and Students
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != student.Id)
            {
                return BadRequest("Query Id not equals to Student id");
            }

            // Get the current logged-in passResetSys's ID

            //var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var existingStudent = await _context.Students.FindAsync(student.Id);

            if (existingStudent == null)
            {
                return NotFound();
            }

            // Check if the current passResetSys is the student or an admin

            if (false)//if ConnectedUser.Token.GetId() != id
            {
                return Unauthorized("You are not authorized to update this student's data.");
            }

            // Update the student properties (map or use AutoMapper)

            if (existingStudent.Email != student.Email)
            {
                var user = _context.Set<User>().FirstOrDefault(u => u.Id == student.Id);
                if (user != null)
                    user.Email = student.Email;
            }
            existingStudent.FirstName = student.FirstName;
            existingStudent.LastName = student.LastName;
            existingStudent.School_Year = student.School_Year;
            existingStudent.Phone = student.Phone;
            existingStudent.Email = student.Email;
            existingStudent.Picture_URL = student.Picture_URL;
            existingStudent.Address = student.Address;
            existingStudent.City_Code = student.City_Code;
            existingStudent.Enrollment = student.Enrollment;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Indicate successful update
        }

        // Update Lecturer (Admin and Lecturer Only)
        [HttpPut("updatelecturer/{id}")]
        [Authorize(Roles = "Admin, Staff, Lecturer")]
        public async Task<IActionResult> UpdateLecturer(int id, [FromBody] Lecturer lecturer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != lecturer.Id)
            {
                return BadRequest("Query Id not equals to Lecturer id");
            }

            //var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var existingLecturer = await _context.Lecturers.FindAsync(lecturer.Id);

            if (existingLecturer == null)
            {
                return NotFound();
            }

            if (false)
            {
                return Unauthorized("You are not authorized to update this Lecturer's data.");
            }

            existingLecturer.FirstName = lecturer.FirstName;
            existingLecturer.LastName = lecturer.LastName;
            existingLecturer.Phone = lecturer.Phone;
            existingLecturer.Email = lecturer.Email;
            existingLecturer.Academic_Degree = lecturer.Academic_Degree;
            existingLecturer.Start_Date = lecturer.Start_Date;
            existingLecturer.Address = lecturer.Address;
            existingLecturer.City_Code = lecturer.City_Code;

            var user = _context.Users.FirstOrDefault(u => u.Id == existingLecturer.Id);
            if (user != null && user.Email != lecturer.Email)
            {
                user.Email = lecturer.Email;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LecturerExists(lecturer.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // Update Staff (Admin and Staff Only)
        [HttpPut("updatestaff/{id}")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] Staff staff)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != staff.Id)
            {
                return BadRequest("Query Id not equals to Staff id");
            }

            //var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var existingStaff = await _context.Staff.FindAsync(staff.Id);

            if (existingStaff == null)
            {
                return NotFound();
            }

            if (false)
            {
                return Unauthorized("You are not authorized to update this Staff's data.");
            }

            existingStaff.FirstName = staff.FirstName;
            existingStaff.LastName = staff.LastName;
            existingStaff.Phone = staff.Phone;
            existingStaff.Email = staff.Email;
            existingStaff.Role_code = staff.Role_code;

            var user = _context.Users.FirstOrDefault(u => u.Id == existingStaff.Id);
            if (user != null && user.Email != staff.Email)
            {
                user.Email = staff.Email;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaffExists(staff.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }




        // Request Password Reset
        [HttpPost("passwordreset/request")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || user.Id != model.Id)
            {
                //Return a success response to avoid revealing if the passResetSys exists
                return Ok("Password reset email sent.");
            }

            var resetToken = GenerateRandomToken();

            var passResetSys = new PasswordResetSys
            {
                Id = user.Id,
                Email = user.Email,
                PasswordResetToken = resetToken,
                PasswordResetTokenExpiration = DateTime.Now.AddMinutes(15)
            };

            _context.Set<PasswordResetSys>().Add(passResetSys);

            await _context.SaveChangesAsync();

            // Send password reset email
            try
            {
                await SendPasswordResetEmail(user.Email, resetToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error sending password reset email.");
            }

            return Ok("Password reset email sent.");
        }

        // Reset Password
        [HttpPut("passwordreset")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resetToken = model.ResetToken;

            if (string.IsNullOrEmpty(resetToken))
            {
                return BadRequest("Missing reset token.");
            }

            var passResetSys = await _context.PasswordResetSys
                .FirstOrDefaultAsync(u => u.PasswordResetToken == resetToken &&
                                          u.PasswordResetTokenExpiration > DateTime.Now);

            if (passResetSys == null)
            {
                return BadRequest("Invalid or expired reset token.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == passResetSys.Id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            // Hash the new password
            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);

            // Clear the reset token
            _context.PasswordResetSys.Remove(passResetSys);


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error resetting password." + ex.Message);
            }

            return Ok("Password reset successfully.");
        }


        // Helper Functions
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
        private bool LecturerExists(int id)
        {
            return _context.Lecturers.Any(e => e.Id == id);
        }
        private bool StaffExists(int id)
        {
            return _context.Staff.Any(e => e.Id == id);
        }


        private string GenerateRandomToken(int length = 64)
        {
            char[] allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

            Random random = new Random();
            StringBuilder token = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                token.Append(allowedChars[random.Next(allowedChars.Length)]);
            }

            return token.ToString();
        }
        private string GenerateJwtToken(int userId, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]); // Get the key from appsettings.json

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role) // Add the role to the token
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task SendPasswordResetEmail(string email, string resetToken)
        {
            IConfiguration emailSettings = _configuration.GetSection("EmailSettings");

            string smtpServer = emailSettings["SmtpServer"];
            int smtpPort = int.Parse(emailSettings["SmtpPort"]);
            string smtpUsername = emailSettings["SmtpUsername"];
            string smtpPassword = emailSettings["SmtpPassword"];


            // Construct the email message
            MailAddress to = new MailAddress(email);
            MailAddress from = new MailAddress(smtpUsername);
            MailMessage emailMsg = new MailMessage(from, to);

            emailMsg.Subject = "Password Reset Request";
            emailMsg.Body = $"You have requested to reset your password. Click on this link to reset it: {GetResetLink(resetToken)}";



            SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
            smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;

            try
            {
                smtp.Send(emailMsg);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetResetLink(string resetToken)
        {
            return $"http://localhost:3000/passwordreset?token={resetToken}"; //TODO
        }

    }

}


