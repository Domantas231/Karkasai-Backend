namespace Karkasai.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
}

public abstract class CreateBaseEntityDTO
{
}

public abstract class UpdateBaseEntityDTO
{
    public int Id { get; set; }
}