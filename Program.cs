using Microsoft.EntityFrameworkCore;
using Donatello.Data;
using Donatello.Data.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

string connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string 'DefaultConnection' not found.");

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();

builder.Services.AddDbContext<DonatelloDbContext>(options =>
    options.UseSqlServer(connStr));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<DonatelloDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
