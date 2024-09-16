using Matala1.Models;
using Matala1.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Matala1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UpdateController : ControllerBase
    {
        private readonly MyDbContext _context;

        public UpdateController(MyDbContext context)
        {
            _context = context;
        }


        [HttpPut("user")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the current logged-in user's ID
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (currentUserId != user.Id && !User.IsInRole("Admin"))
            {
                return Unauthorized("You are not authorized to update this user.");
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw e;
                }
            }

            return NoContent();
        }


        [HttpPut("student/{id}")]
        [Authorize(Roles = "Admin, Student")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            return await UpdateEntity(id, student);
        }


        [HttpPut("lecturer/{id}")]
        [Authorize(Roles = "Admin, Lecturer")]
        public async Task<IActionResult> UpdateLecturer(int id, [FromBody] Lecturer lecturer)
        {
            return await UpdateEntity(id, lecturer);
        }


        [HttpPut("staff/{id}")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] Staff staff)
        {
            return await UpdateEntity(id, staff);
        }


        private async Task<IActionResult> UpdateEntity<T>(int id, T entity)
            where T : class, IEntity
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != entity.Id)
            {
                return BadRequest("Query Id not equals to entity id");
            }

            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int userId = int.Parse(currentUserId);


            var existingEntity = await _context.Set<T>().FindAsync(entity.Id);

            if (existingEntity == null)
            {
                return NotFound();
            }

            // Check if the current user is allowed to update the entity
            if (userId != existingEntity.Id && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Unauthorized("You are not authorized to update this entity.");
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);

            var user = _context.Set<User>().FirstOrDefault(u => u.Id == entity.Id);
            if (user != null && user.Email != entity.Email)
                user.Email = entity.Email;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityExists<T>(entity.Id))
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


        private bool EntityExists<T>(int id) where T : class, IEntity
        {
            return _context.Set<T>().Any(e => e.Id == id);

        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
