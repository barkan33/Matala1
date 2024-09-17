using Matala1.Models;
using Matala1.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace Matala1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;


        public PasswordController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpPut("change")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {

            // Get the current logged-in user's ID from the JWT token
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User ID not found in token.");
            }

            int userId = int.Parse(currentUserId);

            if (userId != model.Id && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Unauthorized("You are not authorized to update this entity.");
            }


            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id);

            if (user == null || !PasswordHelper.VerifyPasswordHash(model.OldPassword, user.PasswordHash) || user.Email != model.Email)
                return BadRequest("One or More Fields Contain Incorrect Values");

            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);

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

            return Ok("Password updated successfully");
        }


        [HttpPost("reset-request")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || user.Id != model.Id)
            {
                //Return a success response to avoid revealing if the passResetSys exists
                return Ok("Password reset email sent.");
            }

            string resetToken = GenerateRandomToken();

            PasswordResetSys passResetSys = new PasswordResetSys
            {
                Id = user.Id,
                Email = user.Email,
                PasswordResetToken = resetToken,
                PasswordResetTokenExpiration = DateTime.Now.AddMinutes(15)
            };

            _context.Set<PasswordResetSys>().Add(passResetSys);

            await _context.SaveChangesAsync();

            // Send password reset email
            try
            {
                await SendPasswordResetEmail(user.Email, resetToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error sending password reset email.");
            }

            return Ok(new { message = "Password reset email sent." });
        }


        [HttpPut("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resetToken = model.ResetToken;

            if (string.IsNullOrEmpty(resetToken))
            {
                return BadRequest("Missing reset token.");
            }

            var passResetSys = await _context.PasswordResetSys
                .FirstOrDefaultAsync(u => u.PasswordResetToken == resetToken &&
                                          u.PasswordResetTokenExpiration > DateTime.Now);

            if (passResetSys == null)
            {
                return BadRequest("Invalid or expired reset token.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == passResetSys.Id);
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            // Hash the new password
            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);

            // Clear the reset token
            _context.PasswordResetSys.Remove(passResetSys);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error resetting password." + ex.Message);
            }

            return Ok("Password reset successfully.");
        }


        private string GenerateRandomToken(int length = 64)
        {
            char[] allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

            Random random = new Random();
            StringBuilder token = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                token.Append(allowedChars[random.Next(allowedChars.Length)]);
            }

            return token.ToString();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private async Task SendPasswordResetEmail(string email, string resetToken)
        {
            IConfiguration emailSettings = _configuration.GetSection("EmailSettings");

            string smtpServer = emailSettings["SmtpServer"];
            int smtpPort = int.Parse(emailSettings["SmtpPort"]);
            string smtpUsername = emailSettings["SmtpUsername"];
            string smtpPassword = emailSettings["SmtpPassword"];


            // Construct the email message
            MailAddress to = new MailAddress(email);
            MailAddress from = new MailAddress(smtpUsername);
            MailMessage emailMsg = new MailMessage(from, to);

            emailMsg.Subject = "Password Reset Request";
            emailMsg.Body = $"You have requested to reset your password. Click on this link to reset it: {GetResetLink(resetToken)}";



            SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
            smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;

            try
            {
                smtp.Send(emailMsg);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //TODO: Change To Real URL
        private string GetResetLink(string resetToken)
        {
            return $"http://localhost:5173/passwordreset?token={resetToken}"; //TODO
        }

    }
}
