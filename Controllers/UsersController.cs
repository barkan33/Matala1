using First.Models;
using Microsoft.AspNetCore.Mvc;

namespace First.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult AllUsers()
        {
            try
            {
                List<User> users = UsersDBServices.AllUsers();
                if (users.Count == 0)
                {
                    return NotFound();
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult GetUser(int id)
        {
            try
            {
                User users = UsersDBServices.GetUserById(id);
                if (users == null)
                {
                    return NotFound();
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Login([FromBody] User userData)
        {
            try
            {

                User user = UsersDBServices.Login(userData.Username, userData.Password);
                if (user == null)
                {
                    return NotFound("Username or password not match");
                }

                return Ok(user);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("registration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Registration([FromBody] User user)
        {
            try
            {
                if (user == null)
                    return BadRequest();

                int res = UsersDBServices.Registration(user.Username, user.Password);

                if (res != -1)
                {
                    return Ok(new { id = res });
                }
                else
                    return BadRequest(new { message = res });

            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }

        }


        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public IActionResult DeleteUser([FromBody] int id)
        {
            try
            {
                string res = UsersDBServices.DeleteUser(id);
                if (res == "deleted")
                {
                    return NoContent();
                }
                else if (res == "not found")
                    return NotFound();
                else
                    return BadRequest(new { message = res });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

    }
}
