using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Extensions;
using System.Reflection;
using WIPSystem.Web.Data;
using WIPSystem.Web.Services; // Assuming ViewRenderService is also part of this namespace
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<WIPDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging(true)
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

// Add the IConfiguration as a Singleton
builder.Services.AddSingleton(builder.Configuration);

// Configure logging here

builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Add other logging providers as needed

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<BarcodeService>();

// Register your View Rendering Service and HttpContextAccessor here
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<LinkGenerator>();

// Register session services
builder.Services.AddSession(options =>
{
    // Set a short timeout for easy testing.
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    // Make the session cookie essential
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton(typeof(IConverter),

new SynchronizedConverter(new PdfTools()));


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