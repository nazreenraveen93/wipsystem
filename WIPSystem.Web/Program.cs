using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Extensions;
using System.Reflection;
using WIPSystem.Web.Data;
using WIPSystem.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<WIPDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging(true)
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

// Add the IConfiguration as a Singleton
builder.Services.AddSingleton(builder.Configuration);


// Configure logging here

builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Add other logging providers as needed

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<BarcodeService>();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
