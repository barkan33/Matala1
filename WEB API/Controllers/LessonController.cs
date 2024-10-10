using Matala1.Models;
using Matala1.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Matala1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LessonController : ControllerBase
    {
        private readonly MyDbContext _context;

        public LessonController(MyDbContext context)
        {
            _context = context;
        }

        // Get all lessons for a course
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetLessonsForCourse(int courseId)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .Include(l => l.WeekDay)
                .ToListAsync();

            if (lessons == null)
            {
                return NotFound();
            }
            return Ok(lessons);
        }

        // Get lesson by ID
        [HttpGet("byId/{lessonId}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.WeekDay)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            return Ok(lesson);
        }

        // Create a new lesson (only for Lecturers)
        [HttpPost]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> CreateLesson([FromBody] TempLesson tempLesson)
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

            int userId = int.Parse(currentUserId);

            var course = await _context.Courses.FindAsync(tempLesson.CourseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            if (course.LecturerId != userId)
            {
                return Unauthorized("You are not authorized to create lessons for this course.");
            }

            Lesson lesson = new Lesson
            {
                CourseId = tempLesson.CourseId,
                WeekDayId = tempLesson.WeekDayId,
                Classroom = tempLesson.Classroom,
                StartTime = TimeSpan.Parse(tempLesson.StartTime),
                EndTime = TimeSpan.Parse(tempLesson.EndTime)
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return Created();
        }

        // Update a lesson (only for Lecturers)
        [HttpPut("{lessonId}")]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] TempLesson tempLesson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tempLesson.Id)
            {
                return BadRequest("Invalid lesson ID.");
            }

            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }
            int userId = int.Parse(currentUserId);


            var existingLesson = await _context.Lessons
        .Include(l => l.WeekDay)
        .Include(l => l.Course)
        .FirstOrDefaultAsync(l => l.Id == id);

            if (existingLesson == null)
            {
                return NotFound();
            }

            // Check if the lecturer teaches the course
            if (existingLesson.Course?.LecturerId != userId)
            {
                return Unauthorized("You are not authorized to update lessons for this course.");
            }
            existingLesson.WeekDayId = tempLesson.WeekDayId != 0 ? tempLesson.WeekDayId : existingLesson.WeekDayId;
            existingLesson.Classroom = tempLesson.Classroom != null ? tempLesson.Classroom : existingLesson.Classroom;

            if (tempLesson.StartTime != null && tempLesson.EndTime != null && TimeSpan.Parse(tempLesson.StartTime) - TimeSpan.Parse(tempLesson.EndTime) < TimeSpan.Zero)
            {
                existingLesson.StartTime = TimeSpan.Parse(tempLesson.StartTime);
                existingLesson.EndTime = TimeSpan.Parse(tempLesson.EndTime);
            }


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LessonExists(id))
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

        // Delete a lesson (only for Lecturers)
        [HttpDelete("{lessonId}")]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> DeleteLesson(int id)
        {

            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int userId = int.Parse(currentUserId);


            var lesson = await _context.Lessons
        .Include(l => l.Course)
        .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            // Check if the lecturer teaches the course
            if (lesson.Course?.LecturerId != userId)
            {
                return Unauthorized("You are not authorized to delete lessons for this course.");
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper function to check if a lesson exists
        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }
    }

}
