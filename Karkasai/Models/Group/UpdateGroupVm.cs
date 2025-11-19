using System.ComponentModel.DataAnnotations;

namespace Karkasai.Models;

public class UpdateGroupVm
{
    public int Id { get; set; }
    [Range(5, 100, ErrorMessage = "Value should be between 5 and 100")]
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; }
    
    [Range(10, 200, ErrorMessage = "Value should be between 10 and 200")]
    [Required(ErrorMessage = "Description is required")]
    public string Description  { get; set; }

    public bool Open { get; set; }
    public int CurrentMembers { get; set; }
    public int MaxMembers { get; set; }
}