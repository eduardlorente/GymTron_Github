using FluentValidation;
using FluentValidation.Results;
using GymTron.Application.Users.DTO;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Web.Pages.Users;
using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GymTron.Web.Tests.Pages.Users;

public class UsersPageModelTests
{
    private readonly IGymTronWebApiClient _apiClient = Substitute.For<IGymTronWebApiClient>();

    [Fact]
    public async Task IndexModel_OnGetAsync_PopulatesUsers()
    {
        var users = new List<UserDto>
        {
            new() { Id = 1, Username = "user1", Email = "u1@gymtron.local", TypeId = UserTypes.Standard, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Username = "admin", Email = "a@gymtron.local", TypeId = UserTypes.Administrator, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        _apiClient.GetUsersAsync(Arg.Any<CancellationToken>()).Returns(users);

        var model = new IndexModel(_apiClient);
        await model.OnGetAsync();

        Assert.Equal(2, model.Users.Count);
        Assert.Equal("user1", model.Users[0].Username);
        Assert.Equal(UserTypes.Standard, model.Users[0].TypeId);
        Assert.Equal("admin", model.Users[1].Username);
        Assert.Equal(UserTypes.Administrator, model.Users[1].TypeId);
    }

    [Fact]
    public async Task IndexModel_OnPostDeleteAsync_CallsDeleteUserAsync()
    {
        var model = new IndexModel(_apiClient);
        var result = await model.OnPostDeleteAsync(5);

        await _apiClient.Received(1).DeleteUserAsync(5, Arg.Any<CancellationToken>());
        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task CreateModel_OnPostAsync_ValidModel_CallsCreateAndRedirects()
    {
        var model = new CreateModel(_apiClient)
        {
            UserInput = new UserCreateViewModel
            {
                Username = "newguy",
                Email = "new@gymtron.local",
                Password = "Password123",
                TypeId = UserTypes.Standard
            }
        };

        var result = await model.OnPostAsync();

        await _apiClient.Received(1).CreateUserAsync(
            Arg.Is<CreateUserRequest>(r => r.Username == "newguy" && r.Email == "new@gymtron.local" && r.TypeId == UserTypes.Standard),
            Arg.Any<CancellationToken>());

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Index", redirect.PageName);
    }

    [Fact]
    public async Task CreateModel_OnPostAsync_InvalidModelState_ReturnsPage()
    {
        var model = new CreateModel(_apiClient);
        model.ModelState.AddModelError("Username", "Required");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        await _apiClient.DidNotReceive().CreateUserAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditModel_OnGetAsync_ExistingUser_PopulatesModel()
    {
        var user = new UserDto
        {
            Id = 10,
            Username = "existing",
            Email = "exist@gymtron.local",
            TypeId = UserTypes.Administrator,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _apiClient.GetUserByIdAsync(10, Arg.Any<CancellationToken>()).Returns(user);

        var model = new EditModel(_apiClient);
        var result = await model.OnGetAsync(10);

        Assert.IsType<PageResult>(result);
        Assert.Equal(10, model.UserInput.Id);
        Assert.Equal("existing", model.UserInput.Username);
        Assert.Equal(UserTypes.Administrator, model.UserInput.TypeId);
    }

    [Fact]
    public async Task EditModel_OnGetAsync_UserNotFound_ReturnsNotFound()
    {
        _apiClient.GetUserByIdAsync(999, Arg.Any<CancellationToken>()).Returns((UserDto?)null);

        var model = new EditModel(_apiClient);
        var result = await model.OnGetAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditModel_OnPostAsync_ValidModel_CallsUpdateAndRedirects()
    {
        var model = new EditModel(_apiClient)
        {
            UserInput = new UserEditViewModel
            {
                Id = 10,
                Username = "updated",
                Email = "updated@gymtron.local",
                TypeId = UserTypes.Standard,
                IsActive = false,
                NewPassword = "NewPassword123"
            }
        };

        var result = await model.OnPostAsync(10);

        await _apiClient.Received(1).UpdateUserAsync(10,
            Arg.Is<UpdateUserRequest>(r => r.Username == "updated" && r.Email == "updated@gymtron.local" && r.IsActive == false && r.NewPassword == "NewPassword123"),
            Arg.Any<CancellationToken>());

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Index", redirect.PageName);
    }
}
