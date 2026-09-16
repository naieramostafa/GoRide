using FluentAssertions;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using Xunit;

namespace RideSharing.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void CreateUser_ShouldSetProperties()
    {
        var user = new User("John", "Doe", "john@test.com", "1234567890", UserRole.Passenger, "password123");

        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Email.Should().Be("john@test.com");
        user.Phone.Should().Be("1234567890");
        user.Role.Should().Be(UserRole.Passenger);
        user.IsActive.Should().BeTrue();
        user.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "correct-password");

        var result = user.VerifyPassword("correct-password");

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ShouldReturnFalse()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "correct-password");

        var result = user.VerifyPassword("wrong-password");

        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateProfile_ShouldChangeFields()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");

        user.UpdateProfile("Jane", "Smith", "456");

        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.Phone.Should().Be("456");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        user.Deactivate();

        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void MarkLogin_ShouldSetLastLoginAt()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");

        user.MarkLogin();

        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RecordFailedLogin_ShouldIncrementAttempts()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");

        user.RecordFailedLogin();

        user.FailedLoginAttempts.Should().Be(1);
        user.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public void RecordFailedLogin_ExceedsMaxAttempts_ShouldLockOut()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");

        for (int i = 0; i < 5; i++)
            user.RecordFailedLogin();

        user.IsLockedOut.Should().BeTrue();
    }

    [Fact]
    public void MarkLogin_ShouldResetFailedAttempts()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        user.MarkLogin();

        user.FailedLoginAttempts.Should().Be(0);
        user.IsLockedOut.Should().BeFalse();
    }
}
