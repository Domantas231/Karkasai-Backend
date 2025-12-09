using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabitTribe.Auth.Model;

namespace HabitTribe.Entities;

public class Comment : BaseEntity
{
    public required string Content { get; set; }
    public DateTimeOffset DateCreated { get; set; }

    public string? ImageUrl { get; set; }
    
    [Required]
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
    
    [JsonIgnore]
    public Post Post { get; set; } = null!;
    public int PostId { get; set; }
}
