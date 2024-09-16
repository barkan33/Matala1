using Matala1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Matala1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LecturerController : ControllerBase
    {
        private readonly MyDbContext _context;

        public LecturerController(MyDbContext context)
        {
            _context = context;
        }

        // Get lecturer by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Staff, Lecturer")]
        public async Task<IActionResult> GetLecturerById(int id)
        {
            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int userId = int.Parse(currentUserId);

            // Check if the current user is allowed to access this Lecturer's data
            if (userId != id && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Unauthorized("You are not authorized to access this Lecturer's data.");
            }

            var lecturer = await _context.Lecturers
       .Include(l => l.Courses)
       .FirstOrDefaultAsync(l => l.Id == id);

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            if (lecturer == null)
            {
                return NotFound();
            }

            return Ok(JsonSerializer.Serialize(lecturer, options));
        }

        // Get all lecturers
        [HttpGet]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> GetAllLecturers()
        {
            var lecturers = await _context.Lecturers
                .Include(l => l.Courses)
                .ToListAsync();
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            if (lecturers == null)
            {
                return NotFound();
            }

            return Ok(JsonSerializer.Serialize(lecturers, options));

        }
    }
}
