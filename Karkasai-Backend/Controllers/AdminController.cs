using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTribe.Auth.Model;
using HabitTribe.Data;
using HabitTribe.Models;

namespace HabitTribe.Controllers;

[ApiController]
[Route("api")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("all")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _context.Groups
            .Include(g => g.OwnerUser)
            .Include(g => g.Members)
            .Include(g => g.Tags)
            .Include(g => g.Posts)
            .ThenInclude(p => p.User)           
            .Include(g => g.Posts)
            .ThenInclude(p => p.Comments)
            .ThenInclude(c => c.User)           
            .ToListAsync();

        
        var result = groups.Select(g => new 
        {
            g.Id,
            g.Title,
            g.Description,
            g.CurrentMembers,
            g.MaxMembers,
            g.DateCreated,
            OwnerUser = new UserDto(g.OwnerUser.UserName!),
            Members = g.Members.Select(m => new UserDto(m.UserName!)).ToList(),
            Tags = g.Tags.Select(t => new TagDto(t.Id, t.Name!, t.Usable)).ToList(),
            Posts = g.Posts.Select(p => new 
            {
                p.Id,
                p.Title,
                p.DateCreated,
                User = new UserDto(p.User.UserName!),
                Comments = p.Comments.Select(c => new CommentDto(
                    c.Id,
                    c.Content,
                    c.DateCreated,
                    new UserDto(c.User.UserName!)
                )).ToList()
            }).ToList()
        });

        return Ok(result);
    }
}