using Microsoft.EntityFrameworkCore;
using HabitTribe.Data;
using HabitTribe.Entities;

namespace HabitTribe.Repositories;

public interface ITagRepository : IRepository<Tag>
{
}

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context) : base(context)
    {
    }
}
