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
        if (existingAdmin is null)
        {
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
        }

        if (!await dbContext.KerdesTipusok.AnyAsync())
        {
            dbContext.KerdesTipusok.AddRange(
                new KerdesTipus
                {
                    Kod = "single_choice",
                    Nev = "Egyválasztós",
                    Leiras = "Pontosan egy helyes válasz.",
                    Aktiv = true
                },
                new KerdesTipus
                {
                    Kod = "multi_choice",
                    Nev = "Többválasztós",
                    Leiras = "Több helyes válasz is lehet.",
                    Aktiv = true
                },
                new KerdesTipus
                {
                    Kod = "true_false",
                    Nev = "Igaz/Hamis",
                    Leiras = "Állítás eldöntése.",
                    Aktiv = true
                },
                new KerdesTipus
                {
                    Kod = "year_input",
                    Nev = "Évszám beírása",
                    Leiras = "A tanuló önállóan írja be az évszámot.",
                    Aktiv = true
                },
                new KerdesTipus
                {
                    Kod = "chronology_order",
                    Nev = "Időrendi sorrend",
                    Leiras = "Események helyes időrendbe rendezése.",
                    Aktiv = true
                },
                new KerdesTipus
                {
                    Kod = "matching",
                    Nev = "Párosítás",
                    Leiras = "Események és évszámok összepárosítása.",
                    Aktiv = true
                });
        }

        await dbContext.SaveChangesAsync();
    }
}
