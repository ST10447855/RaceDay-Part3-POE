using System.Globalization;
using Microsoft.AspNetCore.Localization;
using RaceDay.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC services
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Register session storage
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // works over http inside Docker too
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Connect MVC to the RaceDay API (all data goes through ApiService)
builder.Services.AddHttpClient("RaceDayAPI", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"]
        ?? "http://localhost:5276/");
});
builder.Services.AddScoped<ApiService>();

var app = builder.Build();

// Always parse numbers/dates the same way (e.g. 10.5 km) regardless of server culture
var culture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new List<CultureInfo> { culture },
    SupportedUICultures = new List<CultureInfo> { culture }
});

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Configuration["DisableHttpsRedirect"] != "true")
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

// Session must run before authorization checks
app.UseSession();

app.UseAuthorization();

// Default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
