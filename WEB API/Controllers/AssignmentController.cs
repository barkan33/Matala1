using Matala1.Models;
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
    public class AssignmentController : ControllerBase
    {
        private readonly MyDbContext _context;

        public AssignmentController(MyDbContext context)
        {
            _context = context;
        }

        // Get all assignments for a course
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetAssignmentsForCourse(int courseId)
        {
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .Include(a => a.StudentAssignments)
                .ToListAsync();

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            if (assignments == null)
            {
                return NotFound();
            }

            return Ok(JsonSerializer.Serialize(assignments, options));
        }

        // Get assignment by ID
        [HttpGet("byId/{id}")]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            var assignment = await _context.Assignments
                .Where(a => a.Id == id)
                .Include(a => a.StudentAssignments)
                .ToListAsync();

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            if (assignment == null)
            {
                return NotFound();
            }

            return Ok(JsonSerializer.Serialize(assignment, options));
        }

        // Create a new assignment (only for Lecturers)
        [HttpPost]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> CreateAssignment([FromBody] Assignment assignment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the current logged-in lecturer's ID
            var currentLecturerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Find the course and check if the lecturer teaches it
            var course = await _context.Courses.FindAsync(assignment.CourseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            if (course.LecturerId != currentLecturerId)
            {
                return Unauthorized("You are not authorized to create assignments for this course.");
            }

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok("Assignment created successfully");
        }

        // Update an assignment (only for Lecturers)
        [HttpPut("{id}")]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] Assignment assignment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != assignment.Id)
            {
                return BadRequest("Invalid assignment ID.");
            }

            var existingAssignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingAssignment == null)
            {
                return NotFound();
            }

            // Get the current logged-in lecturer's ID
            var currentLecturerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if the lecturer teaches the course
            if (existingAssignment.Course.LecturerId != currentLecturerId)
            {
                return Unauthorized("You are not authorized to update assignments for this course.");
            }

            _context.Entry(existingAssignment).CurrentValues.SetValues(assignment);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssignmentExists(id))
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

        // Delete an assignment (only for Lecturers)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.Assignments
               .Include(a => a.Course)
               .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            // Get the current logged-in lecturer's ID
            var currentLecturerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if the lecturer teaches the course
            if (assignment.Course.LecturerId != currentLecturerId)
            {
                return Unauthorized("You are not authorized to delete assignments for this course.");
            }

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper function to check if an assignment exists
        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(e => e.Id == id);
        }
    }
}
