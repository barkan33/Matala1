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
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

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

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Return token in response
            return Ok(new { token, user.Id });
        }

        // Update User
        [HttpPut]
        [Authorize] // Require authentication
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the current logged-in user's ID
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (currentUserId != user.Id)
            {
                return Unauthorized("You are not authorized to update this user.");
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
        [Authorize] // Require authentication
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id);

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
                UserRole = "Student"
            };

            _context.Users.Add(user);

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
                UserRole = "Lecturer"
            };

            _context.Set<User>().Add(user);

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
                UserRole = "Staff"

            };

            _context.Set<User>().Add(user);

            _context.Set<Staff>().Add(staff);
            await _context.SaveChangesAsync();

            return Ok("Staff member registered successfully");
        }

        [HttpGet("getstudent/{id}")]
        [Authorize(Roles = "Admin, Staff, Student")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            return await GetEntityById<Student>(id);
        }

        [HttpGet("getlecturer/{id}")]
        [Authorize(Roles = "Admin, Staff, Lecturer")]
        public async Task<IActionResult> GetLecturerById(int id)
        {
            return await GetEntityById<Lecturer>(id);
        }

        [HttpGet("getstaff/{id}")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            return await GetEntityById<Staff>(id);
        }

        private async Task<IActionResult> GetEntityById<T>(int id)
             where T : class, IEntity
        {
            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int userId = int.Parse(currentUserId);

            // Check if the current user is allowed to access the data
            if (userId != id && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Unauthorized($"You are not authorized to access this data.");
            }

            var entity = await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        [HttpPut("updatestudent/{id}")]
        //[Authorize(Roles = "Admin, Student")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            return await UpdateEntity(id, student);
        }

        [HttpPut("updatelecturer/{id}")]
        [Authorize(Roles = "Admin, Lecturer")]
        public async Task<IActionResult> UpdateLecturer(int id, [FromBody] Lecturer lecturer)
        {
            return await UpdateEntity(id, lecturer);
        }

        [HttpPut("updatestaff/{id}")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] Staff staff)
        {
            return await UpdateEntity(id, staff);
        }
        // Helper function for updating entities
        private async Task<IActionResult> UpdateEntity<T>(int id, T entity)
            where T : class, IEntity
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != entity.Id)
            {
                return BadRequest("Query Id not equals to entity id");
            }

            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int userId = int.Parse(currentUserId);


            var existingEntity = await _context.Set<T>().FindAsync(entity.Id);

            if (existingEntity == null)
            {
                return NotFound();
            }

            // Check if the current user is allowed to update the entity
            if (userId != existingEntity.Id && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Unauthorized("You are not authorized to update this entity.");
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);

            var user = _context.Set<User>().FirstOrDefault(u => u.Id == entity.Id);
            if (user != null && user.Email != entity.Email)
                user.Email = entity.Email;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityExists<T>(entity.Id))
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

        private bool EntityExists<T>(int id) where T : class, IEntity
        {
            return _context.Set<T>().Any(e => e.Id == id);

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

            string resetToken = GenerateRandomToken();

            PasswordResetSys passResetSys = new PasswordResetSys
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

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                //new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.UserRole.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
