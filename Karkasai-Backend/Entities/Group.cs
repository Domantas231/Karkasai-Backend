using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabitTribe.Auth.Model;

namespace HabitTribe.Entities;

public class Group : BaseEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int CurrentMembers { get; set; }
    public int MaxMembers { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    
    [Required]
    public required string OwnerUserId { get; set; }
    
    [JsonIgnore]
    public User OwnerUser { get; set; } = null!;
    
    [JsonIgnore]
    public ICollection<User> Members { get; set; } = new List<User>();
    
    [JsonIgnore]
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    
    [JsonIgnore]
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
