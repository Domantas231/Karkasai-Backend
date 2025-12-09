using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabitTribe.Auth.Model;

namespace HabitTribe.Entities;

public class Post : BaseEntity
{
    public required string Title { get; set; }
    public required DateTimeOffset DateCreated { get; set; }
    
    public string? ImageUrl { get; set; }
    
    [Required]
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    [JsonIgnore]
    public Group Group { get; set; } = null!;
    public int GroupId { get; set; }
    
    [JsonIgnore]
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
