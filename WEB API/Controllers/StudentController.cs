using Matala1.Models;
using Matala1.Models.Entities;
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
    public class StudentController : ControllerBase
    {
        private readonly MyDbContext _context;

        public StudentController(MyDbContext context)
        {
            _context = context;
        }

        // Get student by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Staff, Student")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int userId = int.Parse(currentUserId);

            // Check if the current user is allowed to access this student's data
            if (userId != id && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Unauthorized("You are not authorized to access this student's data.");
            }

            var student = await _context.Students
        .Include(s => s.StudentCourses)
                    .ThenInclude(sc => sc.Course)
                        .ThenInclude(c => c.Lecturer)
                    .Include(s => s.StudentCourses)
                        .ThenInclude(sc => sc.Course)
                            .ThenInclude(c => c.Assignments)
                        .Include(s => s.StudentCourses)
                            .ThenInclude(sc => sc.Course)
                                .ThenInclude(c => c.Exams)
                    .FirstOrDefaultAsync(s => s.Id == id);

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            if (student == null)
            {
                return NotFound();
            }

            return Ok(JsonSerializer.Serialize(student, options));
        }

        // Get all students
        [HttpGet]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Students
                .Include(s => s.StudentCourses)
                .ThenInclude(sc => sc.Course)
                    .ThenInclude(c => c.Lecturer)
                .ToListAsync();
            return Ok(students);
        }

        // Enroll a student in a course (only for Admins and Staff)
        [HttpPost("{studentId}/courses/{courseId}")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> EnrollStudent(int studentId, int courseId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var existingEnrollment = _context.StudentCourses.FirstOrDefault(sc =>
                sc.StudentId == studentId && sc.CourseId == courseId);

            if (existingEnrollment != null)
            {
                return BadRequest("Student is already enrolled in this course.");
            }

            // Add the enrollment record
            _context.StudentCourses.Add(new StudentCourses
            {
                StudentId = studentId,
                CourseId = courseId
            });

            await _context.SaveChangesAsync();

            return Ok("Student enrolled successfully.");
        }

        // Unenroll a student from a course (only for Admins and Staff)
        [HttpDelete("{studentId}/courses/{courseId}")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> UnenrollStudent(int studentId, int courseId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var enrollment = _context.StudentCourses
                .FirstOrDefault(sc => sc.StudentId == studentId && sc.CourseId == courseId);

            if (enrollment == null)
            {
                return BadRequest("Student is not enrolled in this course.");
            }

            _context.StudentCourses.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Ok("Student unenrolled successfully.");
        }
    }
}
