using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using GramKar.Data;
using GramKar.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Register application services
builder.Services.AddScoped<DuesCalculator>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<DemandGenerator>();
builder.Services.AddScoped<LocalizerService>();

// Configure Database (MySQL with auto-fallback to SQLite if MySQL is unreachable)
var provider = builder.Configuration["DatabaseProvider"] ?? "Auto";
var mysqlConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=gramkosh.db";

bool useMySql = false;
if (provider.Equals("MySQL", StringComparison.OrdinalIgnoreCase) || provider.Equals("Auto", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        using var testConn = new MySqlConnector.MySqlConnection(mysqlConn);
        testConn.Open();
        useMySql = true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GramKar] MySQL connection failed ({ex.Message}). Falling back to SQLite for zero-config operation.");
        useMySql = false;
    }
}

if (useMySql)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(mysqlConn, ServerVersion.AutoDetect(mysqlConn)));
    Console.WriteLine("[GramKar] Running with MySQL provider.");
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(sqliteConn));
    Console.WriteLine("[GramKar] Running with SQLite provider (gramkosh.db).");
}

// Authentication configuration (20 min idle timeout per FR-1)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "GramKar.Auth";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SarpanchOrAdmin", policy => policy.RequireRole("Admin", "Sarpanch"));
});

var app = builder.Build();

// Seed initial database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await DbInitializer.InitializeAsync(db);
        Console.WriteLine("[GramKar] Database initialized & seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GramKar] Database seeding error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
