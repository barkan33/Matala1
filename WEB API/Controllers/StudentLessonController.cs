using Matala1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Matala1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentLessonController : ControllerBase
    {
        private readonly MyDbContext _context;

        public StudentLessonController(MyDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentLessons()
        {
            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int studentId = int.Parse(currentUserId);

            var lesson = await _context.StudentLessons
              .Where(sl => sl.StudentId == studentId)
              .Include(sl => sl.Lesson)
              .ThenInclude(l => l.WeekDay)
              .FirstOrDefaultAsync();

            if (lesson == null)
            {
                return NotFound();
            }

            return Ok(lesson);
        }
        // Get all student lessons for a course
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetStudentLessonsForCourse(int courseId)
        {


            var lessons = await _context.StudentLessons
                .Where(sl => sl.Lesson.CourseId == courseId)
                .Include(sl => sl.Lesson)
                    .ThenInclude(l => l.WeekDay)
                .ToListAsync();
            return Ok(lessons);
        }

        // Get student lessons by ID
        [HttpGet("{studentId}/{lessonId}")]
        public async Task<IActionResult> GetStudentLessonById(int studentId, int lessonId)
        {
            var lesson = await _context.StudentLessons
                .Where(sl => sl.StudentId == studentId && sl.LessonId == lessonId)
                .Include(sl => sl.Lesson)
                    .ThenInclude(l => l.WeekDay)
                .FirstOrDefaultAsync();

            if (lesson == null)
            {
                return NotFound();
            }

            return Ok(lesson);
        }

        // Update student lesson attendance (only for Admins and Staff)
        [HttpPut("{studentId}/{lessonId}")]
        [Authorize(Roles = "Admin, Staff, Lecturer")]
        public async Task<IActionResult> UpdateStudentLessonAttendance(int studentId, int lessonId, [FromBody] StudentLesson studentLesson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (studentId != studentLesson.StudentId || lessonId != studentLesson.LessonId)
            {
                return BadRequest("Invalid student ID or lesson ID.");
            }

            var existingLesson = await _context.StudentLessons
                .FirstOrDefaultAsync(sl => sl.StudentId == studentId && sl.LessonId == lessonId && sl.LessonDate == studentLesson.LessonDate);

            if (existingLesson == null)
            {
                return NotFound();
            }

            existingLesson.Attendance = studentLesson.Attendance;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentLessonExists(studentId, lessonId, studentLesson.LessonDate))
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

        // Create a new StudentLesson
        [HttpPost]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> CreateStudentLesson([FromBody] StudentLesson studentLesson)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEntry = await _context.StudentLessons
                .FirstOrDefaultAsync(sl =>
                    sl.StudentId == studentLesson.StudentId &&
                    sl.LessonId == studentLesson.LessonId &&
                    sl.LessonDate == studentLesson.LessonDate);

            if (existingEntry != null)
            {
                return BadRequest("A record for this student and lesson on this date already exists.");
            }

            _context.StudentLessons.Add(studentLesson);
            await _context.SaveChangesAsync();

            return Created();
        }
        // Helper function to check if a student lesson exists
        private bool StudentLessonExists(int studentId, int lessonId, DateTime lessonDate)
        {
            return _context.StudentLessons.Any(e => e.StudentId == studentId && e.LessonId == lessonId && e.LessonDate == lessonDate);
        }
    }
}