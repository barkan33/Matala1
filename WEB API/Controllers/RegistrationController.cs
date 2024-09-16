using Matala1.Models;
using Matala1.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Matala1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Staff")]
    public class RegistrationController : ControllerBase
    {
        private readonly MyDbContext _context;

        public RegistrationController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost("{role}")]
        public async Task<IActionResult> RegisterUser(string role, [FromBody] IEntity user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsValidRole(role))
            {
                return BadRequest("Invalid user role.");
            }

            byte[] passwordHash = PasswordHelper.HashPassword(user.Id.ToString());

            var newUser = new User
            {
                Id = user.Id,
                Email = user.Email,
                PasswordHash = passwordHash,
                UserRole = role
            };

            _context.Users.Add(newUser);

            switch (role)
            {
                case "Student":
                    _context.Set<Student>().Add((Student)user);
                    break;
                case "Lecturer":
                    _context.Set<Lecturer>().Add((Lecturer)user);
                    break;
                case "Staff":
                    _context.Set<Staff>().Add((Staff)user);
                    break;
                default:
                    return BadRequest("Invalid user role.");
            }

            await _context.SaveChangesAsync();

            return Ok($"{role} registered successfully");
        }

        private bool IsValidRole(string role)
        {
            return role == "Student" || role == "Lecturer" || role == "Staff";
        }
    }
}
