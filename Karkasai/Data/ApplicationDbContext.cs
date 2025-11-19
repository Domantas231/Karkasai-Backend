using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Karkasai.Entities;

namespace Karkasai.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Group> Groups { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {   
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Group>().ToTable("Groups");
        
        modelBuilder.Entity<Group>().HasData(
            new Group
            {
                Id = 1,
                Title = "Group1",
                Description = "Group1",
                CurrentMembers = 1,
                Open = true,
                MaxMembers = 3,
                UserId = 1
            },
            new Group
            {
                Id = 2,
                Title = "Group2",
                Description = "Group2",
                CurrentMembers = 1,
                Open = false,
                MaxMembers = 5,
                UserId = 2
            }
        );
    }
}