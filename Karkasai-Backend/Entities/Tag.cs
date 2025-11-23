using System.Text.Json.Serialization;

namespace HabitTribe.Entities;

public class Tag : BaseEntity
{
    public required string Name { get; set; }
    
    [JsonIgnore]
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}