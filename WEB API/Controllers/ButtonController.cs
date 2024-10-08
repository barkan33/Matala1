﻿using Matala1.Models;
using Matala1.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class ButtonController : ControllerBase
{
    private readonly MyDbContext _context;

    public ButtonController(MyDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetButtonsSize()
    {
        TimeSpan SizeReductionDelay = TimeSpan.FromHours(24);
        const float SizeReductionFactor = 0.95f;


        // Get the current logged-in user's ID from the JWT token
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return BadRequest("User ID not found in token.");
        }

        int userId = int.Parse(currentUserId);
        Console.WriteLine(userId);


        var buttons = await _context.Buttons.ToListAsync();
        var userButtonClicks = await _context.UserButtonClicks
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var buttonSizes = buttons.Select(button =>
        {
            var buttonClick = userButtonClicks.FirstOrDefault(c => c.ButtonId == button.Id);

            if (buttonClick != null && DateTime.Now - buttonClick.LastClickTimestamp > SizeReductionDelay)
            {

                buttonClick.ClickCount = Math.Max((int)(buttonClick.ClickCount * SizeReductionFactor), button.DefaultSize);
            }

            int newSize = buttonClick != null ? Math.Min(button.DefaultSize + (int)(buttonClick.ClickCount * button.SizeFactor), button.MaxSize) : button.DefaultSize;

            return new ButtonSize
            {
                Id = button.Id,
                Size = newSize,
                LastClickTimestamp = buttonClick?.LastClickTimestamp ?? DateTime.MinValue
            };
        }).ToList();

        await _context.SaveChangesAsync();

        return Ok(buttonSizes);
    }


    //TODO: Bug, add click to all buttons
    [HttpPost("{buttonId}")]
    public async Task<IActionResult> RegisterButtonClick(int buttonId)
    {
        // Get the current logged-in user's ID from the JWT token
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return BadRequest("User ID not found in token.");
        }

        int userId = int.Parse(currentUserId);

        // Try to find the existing button click record for the user and button
        var buttonClicks = await _context.UserButtonClicks
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ButtonId == buttonId);

        // If the record doesn't exist, create a new one
        if (buttonClicks == null)
        {
            buttonClicks = new UserButtonClicks
            {
                UserId = userId,
                ButtonId = buttonId,
                ClickCount = 1,
                LastClickTimestamp = DateTime.Now
            };
            _context.UserButtonClicks.Add(buttonClicks);
        }
        else
        {
            // Increment the click count and update the timestamp
            buttonClicks.ClickCount++;
            buttonClicks.LastClickTimestamp = DateTime.Now;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return Ok();
    }



}

