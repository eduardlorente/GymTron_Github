using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using GymTron.Web.Services.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace GymTron.Web.Tests.Services.Api;

public class WebAuthHttpMessageHandlerTests
{
    private sealed class InnerTestHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    [Fact]
    public async Task SendAsync_AttachesBearerToken_WhenTokenPresentInContext()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var authApiClient = Substitute.For<IAuthApiClient>();
        var authService = Substitute.For<IAuthenticationService>();

        var httpContext = new DefaultHttpContext();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authService);
        httpContext.RequestServices = serviceProvider;

        var authProperties = new AuthenticationProperties();
        authProperties.StoreTokens([
            new AuthenticationToken { Name = "access_token", Value = "my-secret-token" }
        ]);

        var authResult = AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity()), authProperties, CookieAuthenticationDefaults.AuthenticationScheme));

        authService.AuthenticateAsync(httpContext, CookieAuthenticationDefaults.AuthenticationScheme)
            .Returns(Task.FromResult(authResult));

        httpContextAccessor.HttpContext.Returns(httpContext);

        AuthenticationHeaderValue? capturedAuth = null;
        var innerHandler = new InnerTestHandler(req =>
        {
            capturedAuth = req.Headers.Authorization;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var handler = new WebAuthHttpMessageHandler(httpContextAccessor, authApiClient)
        {
            InnerHandler = innerHandler
        };

        var invoker = new HttpMessageInvoker(handler);

        // Act
        var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://localhost/test"), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(capturedAuth);
        Assert.Equal("Bearer", capturedAuth.Scheme);
        Assert.Equal("my-secret-token", capturedAuth.Parameter);
    }

    [Fact]
    public async Task SendAsync_DoesNotAttachAuthHeader_WhenNoTokenPresent()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var authApiClient = Substitute.For<IAuthApiClient>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        AuthenticationHeaderValue? capturedAuth = null;
        var innerHandler = new InnerTestHandler(req =>
        {
            capturedAuth = req.Headers.Authorization;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var handler = new WebAuthHttpMessageHandler(httpContextAccessor, authApiClient)
        {
            InnerHandler = innerHandler
        };

        var invoker = new HttpMessageInvoker(handler);

        // Act
        var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://localhost/test"), CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(capturedAuth);
    }
}
