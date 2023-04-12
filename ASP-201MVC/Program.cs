using ASP_201MVC.Data;
using ASP_201MVC.Services;
using ASP_201MVC.Services.Hash;
using ASP_201MVC.Services.KDF;
using ASP_201MVC.Services.Random;
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
