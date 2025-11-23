using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using HabitTribe.Auth.Model;
using HabitTribe.Models;
using HabitTribe.Services;

namespace HabitTribe.Controllers;

[ApiController]
[Route("api/tags")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly UserManager<User> _userManager;

    public TagController(
        ITagService tagService,
        UserManager<User> userManager)
    {
        _tagService = tagService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        var tags = await _tagService.GetAllTagsAsync(token);
        if (!tags.Any()) return NotFound();

        return Ok(tags);
    }

    [HttpGet("{tagId}")]
    public async Task<IActionResult> Get(int tagId, CancellationToken token)
    {
        var tag = await _tagService.GetTagAsync(tagId, token);
        if (tag == null) return NotFound();

        return Ok(tag);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateUpdateTagDto dto, CancellationToken token)
    {
        try
        {
            var tag = await _tagService.CreateTagAsync(dto, token);
            return Created($"api/tags/{tag.Id}", tag);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPut("{tagId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int tagId, [FromBody] CreateUpdateTagDto tagDto, CancellationToken token)
    {
        var tag = await _tagService.UpdateTagAsync(tagId, tagDto, token);
        if (tag == null) return NotFound();

        return Ok(tag);
    }

    [HttpDelete("{tagId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int tagId, CancellationToken token)
    {
        var deleted = await _tagService.DeleteTagAsync(tagId, token);
        if (!deleted) return NotFound();

        return NoContent();
    }
}
