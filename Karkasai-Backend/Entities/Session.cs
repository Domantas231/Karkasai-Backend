using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HabitTribe.Auth.Model;

namespace HabitTribe.Entities;

public class Session
{
    public Guid Id { get; set; }
    public string LastRefreshToken { get; set; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    
    [Required]
    public required string UserId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; } = null!;
}
