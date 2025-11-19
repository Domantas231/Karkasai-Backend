using Karkasai.Models;
using Karkasai.Services;
using Microsoft.AspNetCore.Mvc;

namespace Karkasai.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
	private readonly GroupService _groupService;

	public GroupController(GroupService groupService)
	{
		_groupService = groupService;
	}
	
	[HttpGet]
	public async Task<IActionResult> GetAllAsync()
	{
		var result = await _groupService.LoadEntities();
		return Ok(result);
	}


	[HttpGet("{id}")]
	public async Task<IActionResult> GetAsync(int id, CancellationToken token = default)
	{
		var group = await _groupService.LoadEntity(id, token);
		
		if (group is null) return NotFound();

		return Ok(group);
	}
	
	[HttpPost]
	public async Task<IActionResult> CreateAsync(CreateGroupVm model, CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid) return BadRequest(ModelState);
		
		var result = await _groupService.CreateEntity(model, cancellationToken);

		return Ok(result);
	}


	[HttpPut("{id}")]

	public async Task<IActionResult> UpdateAsync(UpdateGroupVm model, CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid) return BadRequest(ModelState);

		var completed = await _groupService.UpdateEntity(model.Id, model, cancellationToken);

		if (!completed) return NotFound();
		return Ok();
	}
	
	[HttpGet("{id}")]
	public async Task<IActionResult> DeleteAsync(int id, CancellationToken token = default)
	{
		var completed = await _groupService.DeleteEntity(id, token);

		if (!completed) return BadRequest();

		return Ok();
	}
}
