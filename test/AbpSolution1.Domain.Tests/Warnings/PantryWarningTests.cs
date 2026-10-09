using System;
using Shouldly;
using Xunit;

namespace AbpSolution1.Warnings;

public class PantryWarningTests
{
    private static readonly DateOnly Today = new(2026, 10, 9);

    [Fact]
    public void Should_Not_Change_When_Activated_With_Same_Date()
    {
        var warning = new PantryWarning(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), PantryWarningType.Expiration, Today);

        warning.ActivateForExpiration(Today).ShouldBeFalse();
        warning.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_Deactivate_And_Reactivate()
    {
        var warning = new PantryWarning(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), PantryWarningType.Expiration, Today);

        warning.Deactivate().ShouldBeTrue();
        warning.Deactivate().ShouldBeFalse();
        warning.IsActive.ShouldBeFalse();

        warning.ActivateForExpiration(Today.AddDays(1)).ShouldBeTrue();
        warning.IsActive.ShouldBeTrue();
        warning.ExpirationDate.ShouldBe(Today.AddDays(1));
    }
}
