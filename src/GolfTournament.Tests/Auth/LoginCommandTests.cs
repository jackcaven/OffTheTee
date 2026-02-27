using FluentAssertions;
using GolfTournament.Application.Auth;
using GolfTournament.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace GolfTournament.Tests.Auth;

public class LoginCommandTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public LoginCommandTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _signInManager = Substitute.For<SignInManager<ApplicationUser>>(
            _userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Substitute.For<IOptions<IdentityOptions>>(),
            Substitute.For<ILogger<SignInManager<ApplicationUser>>>(),
            Substitute.For<IAuthenticationSchemeProvider>(),
            Substitute.For<IUserConfirmation<ApplicationUser>>());

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
    public async Task Handle_ValidCredentials_ReturnsAuthResult()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = command.Email,
            UserName = command.Email,
            DisplayName = "Test User"
        };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, false)
            .Returns(SignInResult.Success);

        var handler = new LoginCommandHandler(_userManager, _signInManager, _configuration);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(command.Email);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("nouser@example.com", "Password123!");
        _userManager.FindByEmailAsync(command.Email).ReturnsNull();

        var handler = new LoginCommandHandler(_userManager, _signInManager, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "WrongPassword!");
        var user = new ApplicationUser { Email = command.Email, UserName = command.Email, DisplayName = "User" };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, false)
            .Returns(SignInResult.Failed);

        var handler = new LoginCommandHandler(_userManager, _signInManager, _configuration);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}
