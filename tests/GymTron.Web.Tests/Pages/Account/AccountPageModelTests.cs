using GymTron.Web.Pages.Account;
using GymTron.Web.Services.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using NSubstitute;

namespace GymTron.Web.Tests.Pages.Account;

public class AccountPageModelTests
{
    private readonly IAuthApiClient _authApiClient = Substitute.For<IAuthApiClient>();
    private readonly IAuthenticationService _authService = Substitute.For<IAuthenticationService>();
    private readonly IUrlHelper _urlHelper = Substitute.For<IUrlHelper>();

    private const string ValidTestToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZWlkIjoiMSIsInVuaXF1ZV9uYW1lIjoidXNlciIsImVtYWlsIjoidXNlckB0ZXN0LmNvbSJ9.signature";

    private LoginModel CreateLoginModel()
    {
        var httpContext = new DefaultHttpContext();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(_authService);
        httpContext.RequestServices = serviceProvider;

        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());
        var pageContext = new PageContext(actionContext);

        return new LoginModel(_authApiClient)
        {
            PageContext = pageContext,
            Url = _urlHelper
        };
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_ReturnsPage_WhenModelStateIsInvalid()
    {
        var model = CreateLoginModel();
        model.ModelState.AddModelError("Identifier", "Required");

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_ReturnsPage_WhenLoginFails()
    {
        var model = CreateLoginModel();
        model.Input.Identifier = "unknown";
        model.Input.Password = "Secret123";
        _authApiClient.LoginAsync("unknown", "Secret123", Arg.Any<CancellationToken>())
            .Returns((AuthResultDto?)null);

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_ReturnsPage_WhenAccessTokenIsEmpty()
    {
        var model = CreateLoginModel();
        model.Input.Identifier = "user";
        model.Input.Password = "Secret123";
        _authApiClient.LoginAsync("user", "Secret123", Arg.Any<CancellationToken>())
            .Returns(new AuthResultDto(string.Empty, "refresh", 900, "Bearer"));

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_SignsInAndRedirectsToRoutines_WhenValid()
    {
        var model = CreateLoginModel();
        model.Input.Identifier = "user";
        model.Input.Password = "CorrectPass123";
        _authApiClient.LoginAsync("user", "CorrectPass123", Arg.Any<CancellationToken>())
            .Returns(new AuthResultDto(ValidTestToken, "refresh-123", 900, "Bearer"));

        var result = await model.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Routines/Index", redirectResult.PageName);
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_RedirectsToLocalReturnUrl_WhenValidAndLocal()
    {
        var model = CreateLoginModel();
        model.Input.Identifier = "user";
        model.Input.Password = "CorrectPass123";
        _authApiClient.LoginAsync("user", "CorrectPass123", Arg.Any<CancellationToken>())
            .Returns(new AuthResultDto(ValidTestToken, "refresh-123", 900, "Bearer"));
        _urlHelper.IsLocalUrl("/ExerciseParameters/Index").Returns(true);

        var result = await model.OnPostAsync("/ExerciseParameters/Index");

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/ExerciseParameters/Index", redirectResult.Url);
    }

    [Fact]
    public async Task LogoutModel_OnPostAsync_SignsOutAndRedirectsToLogin()
    {
        var httpContext = new DefaultHttpContext();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(_authService);
        httpContext.RequestServices = serviceProvider;

        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());
        var pageContext = new PageContext(actionContext);

        var model = new LogoutModel { PageContext = pageContext };

        var result = await model.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Account/Login", redirectResult.PageName);
    }

    [Fact]
    public void LogoutModel_OnGet_RedirectsToIndex()
    {
        var model = new LogoutModel();

        var result = model.OnGet();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirectResult.PageName);
    }
}
