using GymTron.Domain.Enums;
using GymTron.Domain.ValueObjects;
using GymTron.UnitTests.Helpers;

namespace GymTron.UnitTests.Domain;

public class ValueObjectTests
{
    private static readonly DateTime CreatedOn = new(2026, 1, 2, 3, 4, 5);

    [Fact]
    public void Equality_WithEquivalentTrainingDates_IsConsistent()
    {
        TrainingDate left = new(CreatedOn);
        TrainingDate right = new(CreatedOn);

        Assert.True(left.Equals(right));
        Assert.True(left.Equals((object)right));
        Assert.True(left == right);
        Assert.False(left != right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentOrInvalidTrainingDates_IsFalse()
    {
        TrainingDate value = new(CreatedOn);
        TrainingDate different = new(CreatedOn.AddSeconds(1));

        Assert.False(value.Equals(different));
        Assert.False(value.Equals((TrainingDate?)null));
        Assert.False(value.Equals((object?)null));
        Assert.False(value.Equals(CreatedOn));
        Assert.False(value == different);
        Assert.True(value != different);
        Assert.True(Equals((TrainingDate?)null, null));
    }

    [Fact]
    public void Equality_WithObservations_UsesCommentValue()
    {
        Observation left = new("Controlled tempo");
        Observation equal = new("Controlled tempo");
        Observation different = new("Pause at bottom");

        Assert.Equal("Controlled tempo", left.Comment);
        Assert.Equal(left, equal);
        Assert.NotEqual(left, different);
        Assert.False(left.Equals((Observation?)null));
        Assert.Equal(left.GetHashCode(), equal.GetHashCode());
    }

    [Fact]
    public void Lifecycle_WithDeterministicTimes_TracksEveryTransition()
    {
        DateTime modifiedOn = CreatedOn.AddHours(1);
        DateTime deletedOn = CreatedOn.AddHours(2);
        EntityStatus status = EntityStatus.FromDatabase(EntityStatusTypes.UNDEFINED, DateTime.MinValue);

        status.Create(CreatedOn);
        Assert.True(status.IsActive);
        Assert.False(status.IsCompleted);

        status.Update(modifiedOn);
        Assert.Equal(modifiedOn, status.ModifiedOn);

        status.Update(EntityStatusTypes.COMPLETED, modifiedOn);
        Assert.True(status.IsCompleted);

        status.Delete(deletedOn);
        Assert.Equal(EntityStatusTypes.DELETED, status.Status);
        Assert.Equal(deletedOn, status.DeletedOn);

        FakeClock clock = new(deletedOn.AddHours(1));
        EntityStatus newStatus = EntityStatus.New(clock);
        Assert.Equal(clock.UtcNow, newStatus.CreatedOn);
        Assert.True(newStatus.IsActive);

        status.Delete(clock);
        Assert.Equal(clock.UtcNow, status.DeletedOn);
    }

    [Fact]
    public void Equality_WithEntityStatuses_UsesAllLifecycleValues()
    {
        DateTime modifiedOn = CreatedOn.AddHours(1);
        DateTime deletedOn = CreatedOn.AddHours(2);
        EntityStatus left = CreateDeletedStatus(CreatedOn, modifiedOn, deletedOn);
        EntityStatus equal = CreateDeletedStatus(CreatedOn, modifiedOn, deletedOn);

        Assert.Equal(left, equal);
        Assert.Equal(left.GetHashCode(), equal.GetHashCode());
        Assert.NotEqual(left, EntityStatus.FromDatabase(EntityStatusTypes.ACTIVE, CreatedOn));
        Assert.NotEqual(left, CreateDeletedStatus(CreatedOn.AddDays(1), modifiedOn, deletedOn));
        Assert.NotEqual(left, CreateDeletedStatus(CreatedOn, modifiedOn.AddMinutes(1), deletedOn));
        Assert.NotEqual(left, CreateDeletedStatus(CreatedOn, modifiedOn, deletedOn.AddMinutes(1)));
        EntityStatus withoutModified = EntityStatus.FromDatabase(EntityStatusTypes.DELETED, CreatedOn);
        withoutModified.Delete(deletedOn);
        EntityStatus withoutDeleted = EntityStatus.FromDatabase(EntityStatusTypes.ACTIVE, CreatedOn);
        withoutDeleted.Update(EntityStatusTypes.DELETED, modifiedOn);
        Assert.NotEqual(left, withoutModified);
        Assert.NotEqual(withoutModified, left);
        Assert.NotEqual(left, withoutDeleted);
        Assert.NotEqual(withoutDeleted, left);
        Assert.NotEqual(withoutModified, EntityStatus.FromDatabase(EntityStatusTypes.DELETED, CreatedOn));
        Assert.NotEqual(withoutDeleted, EntityStatus.FromDatabase(EntityStatusTypes.DELETED, CreatedOn));
        Assert.Equal(
            EntityStatus.FromDatabase(EntityStatusTypes.DELETED, CreatedOn),
            EntityStatus.FromDatabase(EntityStatusTypes.DELETED, CreatedOn));
        Assert.False(left.Equals((EntityStatus?)null));
    }

    private static EntityStatus CreateDeletedStatus(DateTime createdOn, DateTime modifiedOn, DateTime deletedOn)
    {
        EntityStatus status = EntityStatus.FromDatabase(EntityStatusTypes.ACTIVE, createdOn);
        status.Update(modifiedOn);
        status.Delete(deletedOn);
        return status;
    }
}
