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


        [HttpPost("student")]
        public async Task<IActionResult> RegisterStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            if (student.School_Year <= 0)
            {
                return BadRequest("Missing required fields for student registration.");
            }
            if (student.Enrollment == null || student.Enrollment == default(DateTime))
            {
                student.Enrollment = DateTime.Now;
            }

            return await RegisterUser(student, "Student");
        }

        [HttpPost("lecturer")]
        public async Task<IActionResult> RegisterLecturer([FromBody] Lecturer lecturer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            if (lecturer.Start_Date == default(DateTime))
            {
                lecturer.Start_Date = DateTime.Now;
            }
            return await RegisterUser(lecturer, "Lecturer");
        }


        [HttpPost("staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] Staff staff)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }



            return await RegisterUser(staff, "Staff");
        }

        private async Task<IActionResult> RegisterUser(IEntity user, string role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
    }
}
