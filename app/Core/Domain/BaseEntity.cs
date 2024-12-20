namespace zora.Core.Domain;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public bool Deleted { get; set; }
}
