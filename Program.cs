using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CedarDelivery.Constants;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Implementations;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Implementations;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Database: SQL Server through EF Core. The connection string lives in appsettings.json / user-secrets.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity with roles. Same password and lockout rules as CedarPay.
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    // Turned on in Phase 6, once MailKit can send the confirmation emails.
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Google login. The client ID and secret come from user-secrets (see README / CLAUDE.md).
// Until real values are set, Google is simply not registered, so the app still runs
// and the "Sign in with Google" button is hidden.
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !googleClientId.StartsWith("YOUR_")
    && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

    // Web pages redirect to the login page, but an API client (JavaScript, Postman)
    // needs a status code it can read: 401 = not logged in, 403 = wrong role.
    options.Events.OnRedirectToLogin = context =>
        ApiAwareRedirect(context, StatusCodes.Status401Unauthorized, "You must be logged in.");
    options.Events.OnRedirectToAccessDenied = context =>
        ApiAwareRedirect(context, StatusCodes.Status403Forbidden, "You don't have permission to do this.");
});

// Role-based policies, used as [Authorize(Policy = "AdminOnly")] on controllers.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy("MerchantOrAdmin", policy => policy.RequireRole(Roles.Merchant, Roles.Admin));
    options.AddPolicy("DriverOrAdmin", policy => policy.RequireRole(Roles.Driver, Roles.Admin));
});

builder.Services.AddControllersWithViews()
    // Enums travel as text in JSON ("Restaurant" instead of 0), easier to read and to send.
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    // Invalid API input (e.g. missing Name) returns our ApiError format instead of the default one.
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value!.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(error =>
                    string.IsNullOrEmpty(entry.Key) ? error.ErrorMessage : $"{entry.Key}: {error.ErrorMessage}"))
                .ToList();

            return new BadRequestObjectResult(new ApiError("Some fields are invalid.", errors));
        };
    });
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

// Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Swagger (API docs at /swagger in development)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create the roles and demo accounts. If the database isn't created yet
// (dotnet ef database update not run), log the error and keep starting.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RoleSeeder.SeedRolesAsync(services);
        await UserSeeder.SeedUsersAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Used by the cookie events above: /api requests get a JSON error with the status code;
// normal pages keep the usual redirect to the login or access-denied page.
static Task ApiAwareRedirect(RedirectContext<CookieAuthenticationOptions> context, int statusCode, string message)
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new ApiError(message));
    }

    context.Response.Redirect(context.RedirectUri);
    return Task.CompletedTask;
}
