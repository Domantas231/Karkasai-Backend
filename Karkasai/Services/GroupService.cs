using Karkasai.Entities;
using Karkasai.Models;
using Karkasai.Repositories;

namespace Karkasai.Services;

public class GroupService
{
    private readonly IRepository<Group> _repository;

	public GroupService(IRepository<Group> repository)
	{
		_repository = repository;
	}
	
	public async Task<int> CreateEntity(CreateGroupVm model, CancellationToken token = default)
	{
		var newGroup = new Group
		{
			Title = model.Title,
			Description = model.Description,
			Open = model.Open,
			CurrentMembers = 1,
			MaxMembers = model.MaxMembers,
			DateCreated = DateTime.Now,
			UserId = model.UserId,
		};

		await _repository.AddAsync(newGroup, token);
		await _repository.SaveChangesAsync(token);

		return newGroup.Id;
	}
	
	public async Task<bool> UpdateEntity(int id, UpdateGroupVm model, CancellationToken token = default)
	{
		var entity = await _repository.FindAsync(id, token);

		if (entity is null) return false;
		
		Map(entity, model);
		
		await _repository.SaveChangesAsync(token);
		return true;
	}
	
	public async Task<bool> DeleteEntity(int id, CancellationToken token = default)
	{
		var entity = await _repository.FindAsync(id, token);
		if (entity is null)
		{
			return false;
		}
		
		
		await _repository.RemoveAsync(entity, token);
		await _repository.SaveChangesAsync(token);

		return true;
	}

	public async Task<IEnumerable<GroupVm>> LoadEntities(CancellationToken token = default)
	{
		var entities = await _repository.GetAllAsync(token);
		return entities.Select(Map);
	}

	public async Task<GroupVm?> LoadEntity(int id, CancellationToken token = default)
	{
		var group = await _repository.FindAsync(id, token);
		if (group is null) return null;
		
		return Map(group);
	}
    
	private static GroupVm Map(Group group)
	{
		return new GroupVm(
			group.Id, 
			group.Title, 
			group.Description,
			group.Open,
			group.CurrentMembers,
			group.MaxMembers,
			group.DateCreated,
			group.UserId
		);
	}
	
	private static void Map(Group entity, UpdateGroupVm model)
	{
		entity.Title = model.Title;
		entity.Description = model.Description;
		entity.Open = model.Open;
		entity.CurrentMembers = model.CurrentMembers;
		entity.MaxMembers = model.MaxMembers;
	}
}