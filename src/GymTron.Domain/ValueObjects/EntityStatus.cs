using GymTron.Domain.Enums;
using GymTron.Domain.Services;

namespace GymTron.Domain.ValueObjects;

public class EntityStatus : ValueObject<EntityStatus>
{


    public EntityStatusTypes Status { get; private set; }
    public DateTime CreatedOn { get; protected set; }
    public DateTime? ModifiedOn { get; protected set; } = null;
    public DateTime? DeletedOn { get; protected set; } = null;
    public bool IsActive { get => Status == EntityStatusTypes.ACTIVE; }
    public bool IsCompleted { get => Status == EntityStatusTypes.COMPLETED; }


    private EntityStatus(EntityStatusTypes status, DateTime createdOn)
    {
        Status = status;
        CreatedOn = createdOn;
    }


    protected override bool EqualsCore(EntityStatus? other)
    {
        if (other is null)
        {
            return false;
        }

        return Status == other.Status
            && CreatedOn == other.CreatedOn
            && ModifiedOn == other.ModifiedOn
            && DeletedOn == other.DeletedOn;
    }


    protected override int GetHashCodeCore()
    {
        return Status.GetHashCode()
            ^ CreatedOn.GetHashCode()
            ^ ModifiedOn.GetHashCode()
            ^ DeletedOn.GetHashCode();
    }


    internal void Create(IClock clock)
    {
        Create(clock.UtcNow);
    }


    internal void Create(DateTime createdOn)
    {
        Status = EntityStatusTypes.ACTIVE;
        CreatedOn = createdOn;
    }


    internal static EntityStatus New(IClock clock)
    {
        return new EntityStatus(EntityStatusTypes.ACTIVE, clock.UtcNow);
    }


    internal static EntityStatus FromDatabase(DateTime createdOn)
    {
        return new EntityStatus(EntityStatusTypes.ACTIVE, createdOn);
    }


    internal static EntityStatus FromDatabase(EntityStatusTypes status, DateTime createdOn)
    {
        return new EntityStatus(status, createdOn);
    }


    internal void Update(IClock clock)
    {
        Update(clock.UtcNow);
    }


    internal void Update(DateTime modifiedOn)
    {
        ModifiedOn = modifiedOn;
    }


    internal void Update(EntityStatusTypes statusType, IClock clock)
    {
        Update(statusType, clock.UtcNow);
    }


    internal void Update(EntityStatusTypes statusType, DateTime modifiedOn)
    {
        ModifiedOn = modifiedOn;
        Status = statusType;
    }


    internal void Delete(IClock clock)
    {
        Delete(clock.UtcNow);
    }


    internal void Delete(DateTime deletedOn)
    {
        Status = EntityStatusTypes.DELETED;
        DeletedOn = deletedOn;
    }
}
