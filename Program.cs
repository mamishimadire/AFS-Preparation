using AfsWebApp.Data;
using AfsWebApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Change Kestrel to listen on port 5050
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5050);
});

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AfsDbContext>(opt =>
    opt.UseSqlite("Data Source=afs.db"));
builder.Services.AddScoped<AutoMappingService>();
builder.Services.AddScoped<PolicyService>();
builder.Services.AddHttpClient("PolicySearch");

var app = builder.Build();

// Migrate DB on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AfsDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
