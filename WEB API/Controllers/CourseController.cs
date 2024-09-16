using Matala1.Models;
using Matala1.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Matala1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly MyDbContext _context;

        public CourseController(MyDbContext context)
        {
            _context = context;
        }

        // Get all courses
        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Lecturer) // Include the lecturer for each course
                .ToListAsync();
            return Ok(courses);
        }

        // Get course by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }

        // Create a new course (only for Admins and Lecturers)
        [HttpPost]
        [Authorize(Roles = "Admin, Lecturer")]
        public async Task<IActionResult> CreateCourse([FromBody] Course course)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok("Course created successfully");
        }

        // Update a course (only for Admins and Lecturers)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Lecturer")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] Course course)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != course.Id)
            {
                return BadRequest("Invalid course ID.");
            }

            var existingCourse = await _context.Courses.FindAsync(id);

            if (existingCourse == null)
            {
                return NotFound();
            }

            // Update the course properties (you can use AutoMapper for more complex mapping)
            _context.Entry(existingCourse).CurrentValues.SetValues(course);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Indicate successful update
        }

        // Delete a course (only for Admins)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper function to check if a course exists
        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }



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