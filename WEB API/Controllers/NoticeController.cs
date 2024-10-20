using Matala1.Models;
using Matala1.Models.Entitys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Matala1.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NoticeController : ControllerBase
    {
        private readonly MyDbContext _context;
        public NoticeController(MyDbContext context)
        {
            _context = context;
        }

        // Get all notices
        [HttpGet]
        public async Task<IActionResult> GetAllNotices()
        {
            var notices = await _context.Notices.ToListAsync();

            if (notices == null)
            {
                return NotFound("No notices found.");
            }



            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            return Ok(JsonSerializer.Serialize(notices, options));
        }

        // Get notice by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoticeById(int id)
        {
            var notice = await _context.Notices.FindAsync(id);

            if (notice == null)
            {
                return NotFound("Notice not found.");
            }

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            return Ok(JsonSerializer.Serialize(notice, options));
        }

        // Create a new notice (only for Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNotice([FromBody] Notice notice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Notices.Add(notice);
            await _context.SaveChangesAsync();

            return Ok("Notice created successfully");
        }

        // Update a notice (only for Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateNotice(int id, [FromBody] Notice notice)
        {
            if (id != notice.NoticeID)
            {
                return BadRequest("Notice ID mismatch.");
            }

            var existingNotice = await _context.Notices.FindAsync(id);

            if (existingNotice == null)
            {
                return NotFound("Notice not found.");
            }

            _context.Entry(existingNotice).CurrentValues.SetValues(notice);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoticeExists(id))
                {
                    return NotFound("Notice no longer exists.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // Delete a notice (only for Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNotice(int id)
        {
            var notice = await _context.Notices.FindAsync(id);

            if (notice == null)
            {
                return NotFound("Notice not found.");
            }

            _context.Notices.Remove(notice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper function to check if a notice exists
        private bool NoticeExists(int id)
        {
            return _context.Notices.Any(n => n.NoticeID == id);
        }
    }
}
