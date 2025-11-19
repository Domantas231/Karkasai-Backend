namespace Karkasai.Models;

public sealed record GroupVm(int id, string title, string description, bool open, int currentMembers, int maxMembers, 
    DateTime dateCreated, int userId);