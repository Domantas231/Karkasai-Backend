using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HabitTribe.Auth.Model;
using HabitTribe.Entities;

namespace HabitTribe.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    private readonly IConfiguration _configuration;
    
    public ApplicationDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public DbSet<Group> Groups { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Tag> Tags { get; set; }    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(_configuration.GetConnectionString("DefaultConnection"), 
            ServerVersion.AutoDetect(_configuration.GetConnectionString("DefaultConnection")));
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Group>()
            .HasMany(g => g.Members)
            .WithMany(u => u.Groups);
        
        modelBuilder.Entity<Group>()
            .HasMany(g => g.Tags)
            .WithMany(t => t.Groups);
        
        modelBuilder.Entity<Group>()
            .HasOne(g => g.OwnerUser)
            .WithMany()
            .HasForeignKey(g => g.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
