namespace CCTV.Shared.Entities;

public abstract class BaseEntity
{
    public Guid id { get; set; }

    public DateTime createdAt { get; set; }

    public DateTime updatedAt { get; set; }

    public Guid? createdBy { get; set; }

    public Guid? updatedBy { get; set; }

    public bool isDeleted { get; set; }
}