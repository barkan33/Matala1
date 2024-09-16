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
}