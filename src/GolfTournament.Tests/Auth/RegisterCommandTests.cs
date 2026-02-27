using FluentAssertions;
using FluentValidation;
using GolfTournament.Application.Auth;
using GolfTournament.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace GolfTournament.Tests.Auth;

public class RegisterCommandTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public RegisterCommandTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-at-least-32-characters-long!",
                ["Jwt:Issuer"] = "OffTheTee",
                ["Jwt:Audience"] = "OffTheTee",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsAuthResult()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123!", "Test User");

        _userManager.FindByEmailAsync(command.Email).ReturnsNull();
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(IdentityResult.Success);

        var handler = new RegisterCommandHandler(_userManager, _configuration);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(command.Email);
        result.DisplayName.Should().Be(command.DisplayName);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterCommand("existing@example.com", "Password123!", "Existing User");
        var existingUser = new ApplicationUser { Email = command.Email };

        _userManager.FindByEmailAsync(command.Email).Returns(existingUser);

        var handler = new RegisterCommandHandler(_userManager, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Theory]
    [InlineData("", "Password123!", "Name")]       // empty email
    [InlineData("notanemail", "Password123!", "Name")]  // invalid email
    [InlineData("a@b.com", "short", "Name")]        // short password
    [InlineData("a@b.com", "Password123!", "")]     // empty display name
    public async Task Validate_InvalidInputs_ReturnsValidationErrors(
        string email, string password, string displayName)
    {
        var command = new RegisterCommand(email, password, displayName);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var command = new RegisterCommand("test@example.com", "Password123!", "Test User");
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }
}
