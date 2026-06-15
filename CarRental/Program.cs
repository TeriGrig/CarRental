using CarRental.Data;
using CarRental.Middleware;
using CarRental.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
var culture = new CultureInfo("en-US");
var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];

StripeConfiguration.ApiKey = stripeSecretKey;

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Applies any pending migrations and creates the database if it doesn't exist
    db.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    string[] roles = { "Admin","Owner", "Renter" };

    foreach (var role in roles)
    {
        try
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Role seeding failed for {role}: {ex.Message}");
        }
    }

    //string email = "admin@site.com";
    //string password = "Admin123!";

    //var user = await userManager.FindByEmailAsync(email);

    //if (user == null)
    //{
    //    user = new User
    //    {
    //        UserName = email,
    //        Email = email,
    //        EmailConfirmed = true,
    //        FirstName = "Admin",
    //        LastName = "User"
    //    };

    //    await userManager.CreateAsync(user, password);

    //    await userManager.AddToRoleAsync(user, "Admin");

    //    var admin = new Admin
    //    {
    //        UserId = user.Id
    //    };

    //    context.Admins.Add(admin);
    //    await context.SaveChangesAsync();
    //}

    async Task<User> CreateUser(string email, string password, string firstName, string lastName, string role)
    {
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser != null)
            return existingUser;

        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, role);

        return user;
    }


    //  =================================== ADMINS ===================================
    var admin1 = await CreateUser("admin1@site.com", "Admin123!", "Admin", "Ad1", "Admin");
    var admin2 = await CreateUser("admin2@site.com", "Admin123!", "Admin", "Ad2", "Admin");

    if (!context.Admins.Any(a => a.UserId == admin1.Id))
    {
        context.Admins.Add(new Admin
        {
            UserId = admin1.Id
        });
    }

    if (!context.Admins.Any(a => a.UserId == admin2.Id))
    {
        context.Admins.Add(new Admin
        {
            UserId = admin2.Id
        });
    }


    //  =================================== OWNERS ===================================
    var owner1 = await CreateUser("tereza@gmail.com", "Owner123!", "Tereza", "Grig", "Owner");
    var owner2 = await CreateUser("panos@gmail.com", "Owner123!", "Panos", "Kele", "Owner");

    if (!context.Owners.Any(o => o.UserId == owner1.Id))
    {
        context.Owners.Add(new Owner
        {
            UserId = owner1.Id
        });
    }

    if (!context.Owners.Any(o => o.UserId == owner2.Id))
    {
        context.Owners.Add(new Owner
        {
            UserId = owner2.Id
        });
    }


    //  =================================== RENTERS ===================================
    var renter1 = await CreateUser("terimicha@gmail.com", "Renter123!", "Terez", "Micha", "Renter");
    var renter2 = await CreateUser("natali@gmail.com", "Renter123!", "Natalia", "Samal", "Renter");

    if (!context.Renters.Any(r => r.UserId == renter1.Id))
    {
        context.Renters.Add(new Renter
        {
            UserId = renter1.Id
        });
    }

    if (!context.Renters.Any(r => r.UserId == renter2.Id))
    {
        context.Renters.Add(new Renter
        {
            UserId = renter2.Id
        });
    }

    await context.SaveChangesAsync();

    //  =================================== VEHICLES ===================================
    var existingOwner1 = context.Owners.First(o => o.UserId == owner1.Id);
    var existingOwner2 = context.Owners.First(o => o.UserId == owner2.Id);

    if (!context.Vehicles.Any())
    {
        context.Vehicles.AddRange(
            new Vehicle
            {
                Make = "Mercedes",
                Model = "SLS AMG",
                Cubic = 6200,
                Year = 2014,
                PricePerDay = 1500,
                Availability = true,
                Comments = "High performance supercar",
                Image = "/images/Mercedes-Benz_SLS_AMG.jpg",
                OwnerID = existingOwner1.Id
            },

            new Vehicle
            {
                Make = "BMW",
                Model = "320i",
                Cubic = 1995,
                Year = 2007,
                PricePerDay = 70,
                Availability = true,
                Comments = "Luxury sedan",
                Image = "/images/BMW_E90.jpg",
                OwnerID = existingOwner1.Id
            },

            new Vehicle
            {
                Make = "Peugot",
                Model = "208",
                Cubic = 1600,
                Year = 2017,
                PricePerDay = 50,
                Availability = true,
                Comments = "Hatch back",
                Image = "/images/2017_Peugeot_208.jpg",
                OwnerID = existingOwner2.Id
            },

            new Vehicle
            {
                Make = "Audi",
                Model = "RS6",
                Cubic = 4000,
                Year = 2025,
                PricePerDay = 500,
                Availability = true,
                Comments = "High performance station wagon",
                Image = "/images/Audi_RS6_Avant.jpg",
                OwnerID = existingOwner2.Id
            },

            new Vehicle
            {
                Make = "Volkswagen",
                Model = "Passat B6",
                Cubic = 1800,
                Year = 2009,
                PricePerDay = 150,
                Availability = true,
                Comments = "Family station wagon",
                Image = "/images/VW_Passat_B6.jpg",
                OwnerID = existingOwner2.Id
            },

            new Vehicle
            {
                Make = "Suzuki",
                Model = "Swift",
                Cubic = 1400,
                Year = 2007,
                PricePerDay = 55,
                Availability = true,
                Comments = "Hatch back",
                Image = "/images/2007_Suzuki_Swift.jpg",
                OwnerID = existingOwner2.Id
            }
        );

        await context.SaveChangesAsync();
    }

}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<SuspensionMiddleware>();
app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
