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
    public class ExamController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ExamController(MyDbContext context)
        {
            _context = context;
        }

        // Get all exams for a course
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetExamsForCourse(int courseId)
        {
            var exams = await _context.Exams
                .Where(e => e.CourseId == courseId)
                .Include(e => e.StudentExamGrades)
                .ToListAsync();
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            if (exams == null)
            {
                return NotFound();
            }

            return Ok(JsonSerializer.Serialize(exams, options));

        }

        //Get exam by ID
        [HttpGet("byId/{id}")]
        public async Task<IActionResult> GetExamById(int id)
        {
            var exam = await _context.Exams
                .Where(e => e.Id == id)
                .Include(e => e.StudentExamGrades)
                .ToListAsync();

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            if (exam == null)
            {
                return NotFound();
            }

            return Ok(JsonSerializer.Serialize(exam, options));
        }

        // Create a new exam (only for Lecturers)
        [HttpPost]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> CreateExam([FromBody] Exam exam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int currentLecturerId = int.Parse(currentUserId);

            // Find the course and check if the lecturer teaches it
            var course = await _context.Courses.FindAsync(exam.CourseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            if (course.LecturerId != currentLecturerId)
            {
                return Unauthorized("You are not authorized to create exams for this course.");
            }

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            return Ok("Exam created successfully");
        }

        // Update an exam (only for Lecturers)
        [HttpPut("{id}")]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> UpdateExam(int id, [FromBody] Exam exam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != exam.Id)
            {
                return BadRequest("Invalid exam ID.");
            }

            var existingExam = await _context.Exams.FindAsync(id);

            if (existingExam == null)
            {
                return NotFound();
            }

            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int currentLecturerId = int.Parse(currentUserId);

            // Check if the lecturer teaches the course
            if (existingExam.Course.LecturerId != currentLecturerId)
            {
                return Unauthorized("You are not authorized to update exams for this course.");
            }

            _context.Entry(existingExam).CurrentValues.SetValues(exam);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamExists(id))
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

        // Delete an exam (only for Lecturers)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var exam = await _context.Exams.FindAsync(id);

            if (exam == null)
            {
                return NotFound();
            }

            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int currentLecturerId = int.Parse(currentUserId);

            // Check if the lecturer teaches the course
            if (exam.Course.LecturerId != currentLecturerId)
            {
                return Unauthorized("You are not authorized to update exams for this course.");
            }


            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper function to check if an exam exists
        private bool ExamExists(int id)
        {
            return _context.Exams.Any(e => e.Id == id);
        }
    }

}
