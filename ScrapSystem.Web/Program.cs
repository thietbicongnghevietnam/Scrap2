using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Repositories;
using ScrapSystem.Web;
using ScrapSystem.Web.Service;
using ScrapSystem.Web.Service.Interface;
var builder = WebApplication.CreateBuilder(args);



// Add builder.Services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.Web.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IApiClientService, ApiClientService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(60);
});

//doan ket noi co so du lieu  05.08.2025
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Scrap/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
     pattern: "{controller=Scrap}/{action=Index}/{id?}");

app.Run();
