using Microsoft.AspNetCore.Identity;
using HabitTribe.Entities;

namespace HabitTribe.Auth.Model;

public class User : IdentityUser
{
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}
    