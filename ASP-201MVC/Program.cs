using ASP_201MVC.Data;
using ASP_201MVC.Middleware;
using ASP_201MVC.Services;
using ASP_201MVC.Services.Email;
using ASP_201MVC.Services.Hash;
using ASP_201MVC.Services.KDF;
using ASP_201MVC.Services.Random;
using ASP_201MVC.Services.Validation;
using ASP_201MVC.Services.Email;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<DateService>();
builder.Services.AddScoped<TimeService>();
builder.Services.AddSingleton<StampService>();


// Add services to the container.

builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddSingleton<IRandomService, RandomServiceV1>();
builder.Services.AddSingleton<IKdfService, HashKdService>();
builder.Services.AddSingleton<IValidationService, ValidationServiceV1>();
builder.Services.AddSingleton<IEmailService, GmailService>();

//builder.Services.AddDbContext<DataContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("MsDb")
//        )
//);

//ServerVersion serverVersion = new MySqlServerVersion(new Version(8,0,23));
//Варіант 1 - обрати версію
//builder.Services.AddDbContext<DataContext>(options =>
//    options.UseMySql(builder.Configuration.GetConnectionString("MySqlDb"), serverVersion)
//);
//Варіант 2 - автоматично

String? connectionString = builder.Configuration.GetConnectionString("MySqlDb");
MySqlConnection connection = new(connectionString);
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(
        connection, ServerVersion.AutoDetect(
            connection)));

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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
app.UseSession();

app.UseSessionAuth(); //app.UseMiddleware<SessionAuthMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
