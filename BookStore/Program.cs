using BookStore.DataAccess;
using BookStore.DataAccess.Repository;
using BookStore.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using BookStore.Utilities;
using Stripe;
using BookStore.Models;
using BookStore.Models.DbInitializer;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DbContextApp>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

builder.Services.Configure<ApplicationUser>(builder.Configuration.GetSection("Admin"));

builder.Services.Configure<EmailSenderSettings>(builder.Configuration.GetSection("EmailSenderSettings"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddDefaultTokenProviders().AddEntityFrameworkStores<DbContextApp>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services.AddSingleton<IEmailSender, EmailSender>();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = builder.Configuration.GetValue<string>("FaceBookSettings:AppId");
    options.AppSecret = builder.Configuration.GetValue<string>("FaceBookSettings:AppSecret");
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("~/"));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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

StripeConfiguration.ApiKey = app.Configuration.GetSection("StripeSettings:SecretKey").Get<string>();

SeedDatabase();

app.UseAuthentication();

app.UseAuthorization();
app.UseSession();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();


void SeedDatabase()
{
    using(var scope = app.Services.CreateScope())
    {
        var dbInit = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInit.Initialize();
    }
}