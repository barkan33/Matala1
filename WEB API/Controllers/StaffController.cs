using Matala1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Matala1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly MyDbContext _context;

        public StaffController(MyDbContext context)
        {
            _context = context;
        }

        // Get staff by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int userId = int.Parse(currentUserId);

            // Check if the current user is allowed to access this Staff's data
            if (userId != id && !User.IsInRole("Admin"))
            {
                return Unauthorized("You are not authorized to access this Staff's data.");
            }

            var staff = await _context.Staff
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
            {
                return NotFound();
            }

            return Ok(staff);
        }

        // Get all staff
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllStaff()
        {
            var staffMembers = await _context.Staff
                .ToListAsync();
            return Ok(staffMembers);
        }
    }
}
