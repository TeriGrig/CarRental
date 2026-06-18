using CarRental.Data;
using CarRental.Middleware;
using CarRental.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using NuGet.ContentModel;
using Stripe;
using System.Globalization;

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

    string[] roles = { "Admin", "Owner", "Renter" };

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

    async Task<User> CreateUser(string email, string password, string firstName, string lastName, string role, string phone, bool isSuspended, DateOnly date)
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
            LastName = lastName,
            PhoneNumber = phone,
            IsSuspended = isSuspended,
            SuspensionEnd = date
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, role);

        return user;
    }


    //  =================================== ADMINS ===================================
    var admin1 = await CreateUser("admin1@site.com", "Admin123!", "Admin", "Ad1", "Admin", "6912345678", false, new DateOnly(0001, 1, 1));
    var admin2 = await CreateUser("admin2@site.com", "Admin123!", "Admin", "Ad2", "Admin", "6923456789", false, new DateOnly(0001, 1, 1));

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
    var owner1 = await CreateUser("tereza@gmail.com", "Owner123!", "Tereza", "Grig", "Owner", "6934567890", true, new DateOnly(2026, 8, 19));
    var owner2 = await CreateUser("panos@gmail.com", "Owner123!", "Panos", "Kele", "Owner", "6945678901", false, new DateOnly(0001, 1, 1));

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
    var renter1 = await CreateUser("terimicha@gmail.com", "Renter123!", "Terez", "Micha", "Renter", "6956789012", false, new DateOnly(0001, 1, 1));
    var renter2 = await CreateUser("natali@gmail.com", "Renter123!", "Natalia", "Samal", "Renter", "6967890123", true, new DateOnly(2026, 8, 19));

    if (!context.Renters.Any(r => r.UserId == renter1.Id))
    {
        context.Renters.Add(new Renter
        {
            UserId = renter1.Id,
            BirthYear = 2002,
            LicenceYear = 2020
        });
    }

    if (!context.Renters.Any(r => r.UserId == renter2.Id))
    {
        context.Renters.Add(new Renter
        {
            UserId = renter2.Id,
            BirthYear = 1998,
            LicenceYear = 2017
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
                Latitude = 37.9755,
                Longitude = 23.7348,
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
                Latitude = 40.6401,
                Longitude = 22.9444,
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
                Latitude = 38.2466,
                Longitude = 21.7346,
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
                Latitude = 35.3387,
                Longitude = 25.1442,
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
                Latitude = 39.6390,
                Longitude = 22.4191,
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
                Latitude = 39.3610,
                Longitude = 22.9429,
                Comments = "Hatch back",
                Image = "/images/2007_Suzuki_Swift.jpg",
                OwnerID = existingOwner2.Id
            }
        );

        await context.SaveChangesAsync();
    }


    // =================================== REVIEWS ===================================

    if (!context.Reviews.Any())
    {
        context.Reviews.AddRange(

            new CarRental.Models.Review
            {
                Rating = 5,
                Comment = "Excellent renter. Returned the car in perfect condition.",
                CommenterId = owner1.Id,
                RecipientId = renter1.Id
            },

            new CarRental.Models.Review
            {
                Rating = 4,
                Comment = "Very friendly owner and smooth pickup process.",
                CommenterId = renter1.Id,
                RecipientId = owner1.Id
            },

            new CarRental.Models.Review
            {
                Rating = 5,
                Comment = "Highly recommended. Great communication.",
                CommenterId = owner2.Id,
                RecipientId = renter2.Id
            },

            new CarRental.Models.Review
            {
                Rating = 3,
                Comment = "Car was fine but pickup was delayed.",
                CommenterId = renter2.Id,
                RecipientId = owner2.Id
            },

            new CarRental.Models.Review
            {
                Rating = 4,
                Comment = "Vehicle returned clean and on time.",
                CommenterId = owner2.Id,
                RecipientId = renter1.Id
            },

            new CarRental.Models.Review
            {
                Rating = 5,
                Comment = "Fantastic experience. Would rent again.",
                CommenterId = renter1.Id,
                RecipientId = owner2.Id
            }
        );

        await context.SaveChangesAsync();
    }


    // =================================== REPORTS ===================================

    if (!context.Reports.Any())
    {
        context.Reports.AddRange(

            new Report
            {
                ReporterId = renter2.Id,
                ReportRecipientId = owner2.Id,
                DateTime = DateTime.Now.AddDays(-10),
                Description = "Owner arrived 40 minutes late for vehicle pickup.",
                Seen = false
            },

            new Report
            {
                ReporterId = owner1.Id,
                ReportRecipientId = renter2.Id,
                DateTime = DateTime.Now.AddDays(-6),
                Description = "Vehicle was returned with an empty fuel tank.",
                Seen = true
            },

            new Report
            {
                ReporterId = renter1.Id,
                ReportRecipientId = owner1.Id,
                DateTime = DateTime.Now.AddDays(-3),
                Description = "Vehicle was not cleaned before pickup.",
                Seen = false
            },

            new Report
            {
                ReporterId = owner2.Id,
                ReportRecipientId = renter1.Id,
                DateTime = DateTime.Now.AddDays(-1),
                Description = "Renter exceeded the agreed return time by several hours.",
                Seen = false
            }
        );

        await context.SaveChangesAsync();
    }


    // =================================== BOOKINGS ===================================
    var existingRenter1 = context.Renters.First(r => r.UserId == renter1.Id);
    var existingRenter2 = context.Renters.First(r => r.UserId == renter2.Id);

    var mercedes = context.Vehicles.First(v => v.Make == "Mercedes");
    var bmw = context.Vehicles.First(v => v.Make == "BMW");
    var audi = context.Vehicles.First(v => v.Make == "Audi");
    var passat = context.Vehicles.First(v => v.Make == "Volkswagen");

    if (!context.Bookings.Any())
    {
        context.Bookings.AddRange(

            // Active booking
            new Booking
            {
                VehicleId = mercedes.VehicleId,
                RenterId = existingRenter1.Id,

                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(5),

                Status = "Accepted",
                IsPaid = true
            },

            // Archived booking
            new Booking
            {
                VehicleId = bmw.VehicleId,
                RenterId = existingRenter1.Id,

                StartDate = DateTime.Now.AddDays(-15),
                EndDate = DateTime.Now.AddDays(-10),

                Status = "Accepted",
                IsPaid = true
            },

            // Active booking
            new Booking
            {
                VehicleId = audi.VehicleId,
                RenterId = existingRenter2.Id,

                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(4),

                Status = "Requested",
                IsPaid = false
            },

            // Archived booking
            new Booking
            {
                VehicleId = passat.VehicleId,
                RenterId = existingRenter2.Id,

                StartDate = DateTime.Now.AddDays(-8),
                EndDate = DateTime.Now.AddDays(-2),

                Status = "Cancelled",
                IsPaid = false
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
