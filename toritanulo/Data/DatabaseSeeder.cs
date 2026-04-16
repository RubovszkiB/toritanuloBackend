using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using toritanulo.Models;

namespace toritanulo.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<User>>();

        var existingAdmin = await dbContext.Users.FirstOrDefaultAsync(x => x.Username == "balazs");
        if (existingAdmin is not null)
        {
            return;
        }

        var adminUser = new User
        {
            Username = "balazs",
            Email = "balazs@toritanulo.local",
            FullName = "Rubovszki Balázs",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "balazs123");

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync();
    }
}
