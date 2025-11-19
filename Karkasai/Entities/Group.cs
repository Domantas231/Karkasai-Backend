namespace Karkasai.Entities;

//int id, string title, string description, bool open, int currentMembers, int maxMembers, 
// DateTime dateCreated, int userId);

public class Group : BaseEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description  { get; set; }
    public bool Open { get; set; }
    public int CurrentMembers { get; set; }
    public int MaxMembers { get; set; }
    public DateTime DateCreated { get; set; }

    public int UserId { get; set; }
}

public class CreateGroupDTO : CreateBaseEntityDTO
{
    public string Title { get; set; }
    public string Description  { get; set; }
    public bool Open { get; set; }
    public int CurrentMembers { get; set; }
    public int MaxMembers { get; set; }

    public int UserId { get; set; }
}

public class UpdateGroupDTO : UpdateBaseEntityDTO
{
    public string Title { get; set; }
    public string Description  { get; set; }
    public bool Open { get; set; }
    public int CurrentMembers { get; set; }
    public int MaxMembers { get; set; }
}