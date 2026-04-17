using Microsoft.EntityFrameworkCore;
using toritanulo.Models;

namespace toritanulo.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tetel> Tetelek => Set<Tetel>();
    public DbSet<TetelOlvasasiAllapot> TetelOlvasasiAllapotok => Set<TetelOlvasasiAllapot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .HasColumnName("id");

            entity.Property(u => u.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(100);

            entity.Property(u => u.Role)
                .HasColumnName("role")
                .HasMaxLength(20)
                .HasDefaultValue("Student")
                .IsRequired();

            entity.Property(u => u.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate()
                .IsRequired();

            entity.HasIndex(u => u.Username)
                .IsUnique();

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });

        modelBuilder.Entity<Tetel>(entity =>
        {
            entity.ToTable("tetelek");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .HasColumnName("id");

            entity.Property(t => t.Sorszam)
                .HasColumnName("sorszam")
                .IsRequired();

            entity.Property(t => t.Cim)
                .HasColumnName("cim")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(t => t.ForrasFajlnev)
                .HasColumnName("forras_fajlnev")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(t => t.Tartalom)
                .HasColumnName("tartalom")
                .HasColumnType("longtext")
                .IsRequired();

            entity.Property(t => t.BekezdesDb)
                .HasColumnName("bekezdes_db")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(t => t.Aktiv)
                .HasColumnName("aktiv")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(t => t.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate()
                .IsRequired();

            entity.HasIndex(t => t.Sorszam)
                .IsUnique();

            entity.HasIndex(t => t.Cim);

            entity.HasIndex(t => t.Aktiv);
        });

        modelBuilder.Entity<TetelOlvasasiAllapot>(entity =>
        {
            entity.ToTable("tetel_olvasasi_allapotok");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(x => x.TetelId)
                .HasColumnName("tetel_id")
                .IsRequired();

            entity.Property(x => x.HaladasSzazalek)
                .HasColumnName("haladas_szazalek")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.LastOpenedAt)
                .HasColumnName("last_opened_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate()
                .IsRequired();

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.TetelId);
            entity.HasIndex(x => new { x.UserId, x.UpdatedAt });
            entity.HasIndex(x => new { x.UserId, x.TetelId }).IsUnique();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Tetel>()
                .WithMany()
                .HasForeignKey(x => x.TetelId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userEntries = ChangeTracker
            .Entries<User>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in userEntries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        var tetelEntries = ChangeTracker
            .Entries<Tetel>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in tetelEntries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        var progressEntries = ChangeTracker
            .Entries<TetelOlvasasiAllapot>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in progressEntries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
