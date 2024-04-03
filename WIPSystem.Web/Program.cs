using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DinkToPdf.Contracts;
using DinkToPdf;
using WIPSystem.Web.Data;
using WIPSystem.Web.Services;
using System;
using SmartBreadcrumbs.Extensions;
using WIPSystem.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Serialization;
using WIPSystem.Web.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<WIPDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>().AddDefaultTokenProviders()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<WIPDbContext>();

// You might want to configure Identity options here as well
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings, lockout settings, user settings, etc.
    // For example:
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
});

// Configure the application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Account/Login"; // Your LoginPath here
    options.LogoutPath = "/Account/Logout"; // Your LogoutPath here
    options.AccessDeniedPath = "/Account/AccessDenied"; // Your AccessDeniedPath here
    options.SlidingExpiration = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

// Add the IConfiguration as a Singleton
builder.Services.AddSingleton(builder.Configuration);

// Register your custom service here


// Configure logging here
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Add other logging providers as needed

builder.Services.AddScoped<BarcodeService>();

// Register your View Rendering Service and HttpContextAccessor here
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddHttpContextAccessor();

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

// Register the Email Service
builder.Services.AddSingleton<IEmailService, EmailService>(); // <-- Add this line

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
app.UseAuthentication(); // Add this to enable authentication
app.UseAuthorization();  // This was already here, ensure it is below UseAuthentication

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CurrentStatus}/{action=Index}/{id?}");

app.Run();
