using System.Globalization;
using GymTron.Web.Services.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

string apiUrl = builder.Configuration["ApiUrl"]
    ?? throw new InvalidOperationException("Required configuration 'ApiUrl' is missing or blank.");

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Routines");
    options.Conventions.AuthorizeFolder("/ExerciseParameters");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/SetLanguage");
    options.Conventions.AllowAnonymousToFolder("/Account");
})
.AddViewLocalization()
.AddDataAnnotationsLocalization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("ca"),
        new CultureInfo("es"),
        new CultureInfo("en")
    };

    options.DefaultRequestCulture = new RequestCulture("ca");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// HTTP Context Accessor and API Clients
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<WebAuthHttpMessageHandler>();

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient<IGymTronWebApiClient, GymTronWebApiClient>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
})
.AddHttpMessageHandler<WebAuthHttpMessageHandler>();

var app = builder.Build();

// Configure request localization
app.UseRequestLocalization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
