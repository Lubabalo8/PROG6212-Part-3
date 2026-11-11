using Microsoft.AspNetCore.Authentication.Cookies;
using RaceBay.MVC.Services;
using RaceBay.MVC.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC + Views
builder.Services.AddControllersWithViews();



// 1. MVC + Views
builder.Services.AddControllersWithViews();

// 2. Bind the ApiSettings
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

// 3. Register the HttpClient (PUT IT HERE)
builder.Services.AddHttpClient<IApiService, ApiService>();


// 2. Bind the ApiSettings from appsettings.json (or env vars)
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

// 3. Register a named HttpClient and the ApiService
builder.Services.AddHttpClient<IApiService, ApiService>();
// 4. Cookie Authentication (sessions across pages).
// Authentication and user persistence are handled by the API; this MVC
// application only stores the authenticated API response in a cookie.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// 5. Access to HttpContext (needed to read session claims)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Standard middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANT: Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();