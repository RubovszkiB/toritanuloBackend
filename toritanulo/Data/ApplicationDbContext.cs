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

    public DbSet<TesztTemakor> TesztTemakorok => Set<TesztTemakor>();
    public DbSet<KerdesTipus> KerdesTipusok => Set<KerdesTipus>();
    public DbSet<KronologiaEsemeny> KronologiaEsemenyek => Set<KronologiaEsemeny>();
    public DbSet<Teszt> Tesztek => Set<Teszt>();
    public DbSet<Kerdes> Kerdesek => Set<Kerdes>();
    public DbSet<KerdesHelyesValasz> KerdesHelyesValaszok => Set<KerdesHelyesValasz>();
    public DbSet<KerdesValaszOpcio> KerdesValaszOpcioK => Set<KerdesValaszOpcio>();
    public DbSet<KerdesPar> KerdesParok => Set<KerdesPar>();
    public DbSet<TesztKerdes> TesztKerdesek => Set<TesztKerdes>();
    public DbSet<TesztProbalkozas> TesztProbalkozasok => Set<TesztProbalkozas>();
    public DbSet<TesztValasz> TesztValaszok => Set<TesztValasz>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureTetelek(modelBuilder);
        ConfigureTetelOlvasasiAllapotok(modelBuilder);

        ConfigureTesztTemakorok(modelBuilder);
        ConfigureKerdesTipusok(modelBuilder);
        ConfigureKronologiaEsemenyek(modelBuilder);
        ConfigureTesztek(modelBuilder);
        ConfigureKerdesek(modelBuilder);
        ConfigureKerdesHelyesValaszok(modelBuilder);
        ConfigureKerdesValaszOpcioK(modelBuilder);
        ConfigureKerdesParok(modelBuilder);
        ConfigureTesztKerdesek(modelBuilder);
        ConfigureTesztProbalkozasok(modelBuilder);
        ConfigureTesztValaszok(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestampUpdates();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTimestampUpdates();
        return base.SaveChanges();
    }

    private void ApplyTimestampUpdates()
    {
        var entries = ChangeTracker.Entries<IHasTimestamps>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }

                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IHasTimestamps.CreatedAt)).IsModified = false;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(100);
            entity.Property(x => x.Role).HasColumnName("role").HasColumnType("enum('Student','Admin')").HasDefaultValue("Student").IsRequired();
            entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate().IsRequired();

            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
        });
    }

    private static void ConfigureTetelek(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tetel>(entity =>
        {
            entity.ToTable("tetelek");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Sorszam).HasColumnName("sorszam").IsRequired();
            entity.Property(x => x.Cim).HasColumnName("cim").HasMaxLength(255).IsRequired();
            entity.Property(x => x.ForrasFajlnev).HasColumnName("forras_fajlnev").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Tartalom).HasColumnName("tartalom").HasColumnType("longtext").IsRequired();
            entity.Property(x => x.BekezdesDb).HasColumnName("bekezdes_db").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.Aktiv).HasColumnName("aktiv").HasDefaultValue(true).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate().IsRequired();

            entity.HasIndex(x => x.Sorszam).IsUnique();
            entity.HasIndex(x => x.Cim);
            entity.HasIndex(x => x.Aktiv);
        });
    }

    private static void ConfigureTetelOlvasasiAllapotok(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TetelOlvasasiAllapot>(entity =>
        {
            entity.ToTable("tetel_olvasasi_allapotok");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.TetelId).HasColumnName("tetel_id").IsRequired();
            entity.Property(x => x.HaladasSzazalek).HasColumnName("haladas_szazalek").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.LastOpenedAt).HasColumnName("last_opened_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate().IsRequired();

            entity.HasIndex(x => new { x.UserId, x.TetelId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.TetelOlvasasiAllapotok)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Tetel)
                .WithMany(x => x.TetelOlvasasiAllapotok)
                .HasForeignKey(x => x.TetelId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTesztTemakorok(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TesztTemakor>(entity =>
        {
            entity.ToTable("teszt_temakorok");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Kod).HasColumnName("kod").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Nev).HasColumnName("nev").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Leiras).HasColumnName("leiras").HasColumnType("text");
            entity.Property(x => x.Sorszam).HasColumnName("sorszam").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.Aktiv).HasColumnName("aktiv").HasDefaultValue(true).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate().IsRequired();

            entity.HasIndex(x => x.Kod).IsUnique();
            entity.HasIndex(x => x.Sorszam);
            entity.HasIndex(x => x.Aktiv);
        });
    }

    private static void ConfigureKerdesTipusok(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KerdesTipus>(entity =>
        {
            entity.ToTable("kerdes_tipusok");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.Kod).HasColumnName("kod").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Nev).HasColumnName("nev").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Leiras).HasColumnName("leiras").HasColumnType("text");
            entity.Property(x => x.Aktiv).HasColumnName("aktiv").HasDefaultValue(true).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate().IsRequired();

            entity.HasIndex(x => x.Kod).IsUnique();
            entity.HasIndex(x => x.Aktiv);
        });
    }

    private static void ConfigureKronologiaEsemenyek(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KronologiaEsemeny>(entity =>
        {
            entity.ToTable("kronologia_esemenyek");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.TemakorId).HasColumnName("temakor_id").IsRequired();
            entity.Property(x => x.TetelId).HasColumnName("tetel_id");
            entity.Property(x => x.Cim).HasColumnName("cim").HasMaxLength(255).IsRequired();
            entity.Property(x => x.RovidLeiras).HasColumnName("rovid_leiras").HasColumnType("text");
            entity.Property(x => x.EvKezd).HasColumnName("ev_kezd").IsRequired();
            entity.Property(x => x.EvVeg).HasColumnName("ev_veg");
            entity.Property(x => x.Idoszamitas).HasColumnName("idoszamitas").HasColumnType("enum('BCE','CE')").IsRequired();
            entity.Property(x => x.EvszamSzoveg).HasColumnName("evszam_szoveg").HasMaxLength(50).IsRequired();
            entity.Property(x => x.RendezesiEv).HasColumnName("rendezesi_ev").IsRequired();
            entity.Property(x => x.Fontossag).HasColumnName("fontossag").HasDefaultValue((byte)3).IsRequired();
            entity.Property(x => x.Aktiv).HasColumnName("aktiv").HasDefaultValue(true).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate().IsRequired();

            entity.HasIndex(x => x.TemakorId);
            entity.HasIndex(x => x.TetelId);
            entity.HasIndex(x => x.RendezesiEv);
            entity.HasIndex(x => x.Fontossag);

            entity.HasOne(x => x.Temakor)
                .WithMany(x => x.KronologiaEsemenyek)
                .HasForeignKey(x => x.TemakorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Tetel)
                .WithMany(x => x.KronologiaEsemenyek)
                .HasForeignKey(x => x.TetelId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureTesztek(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Teszt>(entity =>
        {
            entity.ToTable("tesztek");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.TemakorId).HasColumnName("temakor_id").IsRequired();
            entity.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Cim).HasColumnName("cim").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Leiras).HasColumnName("leiras").HasColumnType("text");
            entity.Property(x => x.TesztTipus).HasColumnName("teszt_tipus").HasColumnType("enum('evszam','szemely','vegyes')").HasDefaultValue("evszam").IsRequired();
            entity.Property(x => x.Nehezseg).HasColumnName("nehezseg").HasColumnType("enum('konnyu','kozepes','nehez')").HasDefaultValue("konnyu").IsRequired();
            entity.Property(x => x.IdokeretMp).HasColumnName("idokeret_mp");
            entity.Property(x => x.Aktiv).HasColumnName("aktiv").HasDefaultValue(true).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate().IsRequired();

            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.TemakorId);
            entity.HasIndex(x => x.Aktiv);

            entity.HasOne(x => x.Temakor)
                .WithMany(x => x.Tesztek)
                .HasForeignKey(x => x.TemakorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureKerdesek(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kerdes>(entity =>
        {
            entity.ToTable("kerdesek");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.TemakorId).HasColumnName("temakor_id").IsRequired();
            entity.Property(x => x.KerdesTipusId).HasColumnName("kerdes_tipus_id").IsRequired();
            entity.Property(x => x.KronologiaEsemenyId).HasColumnName("kronologia_esemeny_id");
            entity.Property(x => x.KerdesSzoveg).HasColumnName("kerdes_szoveg").HasColumnType("text").IsRequired();
            entity.Property(x => x.Instrukcio).HasColumnName("instrukcio").HasColumnType("text");
            entity.Property(x => x.Magyarazat).HasColumnName("magyarazat").HasColumnType("text");
            entity.Property(x => x.Nehezseg).HasColumnName("nehezseg").HasDefaultValue((byte)2).IsRequired();
            entity.Property(x => x.Aktiv).HasColumnName("aktiv").HasDefaultValue(true).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate().IsRequired();

            entity.HasIndex(x => x.TemakorId);
            entity.HasIndex(x => x.KerdesTipusId);
            entity.HasIndex(x => x.KronologiaEsemenyId);
            entity.HasIndex(x => x.Aktiv);

            entity.HasOne(x => x.Temakor)
                .WithMany(x => x.Kerdesek)
                .HasForeignKey(x => x.TemakorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.KerdesTipus)
                .WithMany(x => x.Kerdesek)
                .HasForeignKey(x => x.KerdesTipusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.KronologiaEsemeny)
                .WithMany(x => x.Kerdesek)
                .HasForeignKey(x => x.KronologiaEsemenyId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureKerdesHelyesValaszok(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KerdesHelyesValasz>(entity =>
        {
            entity.ToTable("kerdes_helyes_valaszok");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.KerdesId).HasColumnName("kerdes_id").IsRequired();
            entity.Property(x => x.ValaszSzoveg).HasColumnName("valasz_szoveg").HasMaxLength(255);
            entity.Property(x => x.ValaszSzam).HasColumnName("valasz_szam");
            entity.Property(x => x.Era).HasColumnName("era").HasColumnType("enum('BCE','CE','NONE')").HasDefaultValue("NONE").IsRequired();
            entity.Property(x => x.NormalizaltValasz).HasColumnName("normalizalt_valasz").HasMaxLength(255).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

            entity.HasIndex(x => x.KerdesId);
            entity.HasIndex(x => new { x.KerdesId, x.NormalizaltValasz }).IsUnique();

            entity.HasOne(x => x.Kerdes)
                .WithMany(x => x.HelyesValaszok)
                .HasForeignKey(x => x.KerdesId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureKerdesValaszOpcioK(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KerdesValaszOpcio>(entity =>
        {
            entity.ToTable("kerdes_valaszopciok");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.KerdesId).HasColumnName("kerdes_id").IsRequired();
            entity.Property(x => x.ValaszSzoveg).HasColumnName("valasz_szoveg").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Helyes).HasColumnName("helyes").HasDefaultValue(false).IsRequired();
            entity.Property(x => x.HelyesSorrend).HasColumnName("helyes_sorrend");
            entity.Property(x => x.Sorszam).HasColumnName("sorszam").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

            entity.HasIndex(x => x.KerdesId);
            entity.HasIndex(x => x.Sorszam);

            entity.HasOne(x => x.Kerdes)
                .WithMany(x => x.ValaszOpcioK)
                .HasForeignKey(x => x.KerdesId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureKerdesParok(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KerdesPar>(entity =>
        {
            entity.ToTable("kerdes_parok");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.KerdesId).HasColumnName("kerdes_id").IsRequired();
            entity.Property(x => x.BalOldal).HasColumnName("bal_oldal").HasMaxLength(255).IsRequired();
            entity.Property(x => x.JobbOldal).HasColumnName("jobb_oldal").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Sorszam).HasColumnName("sorszam").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

            entity.HasIndex(x => x.KerdesId);

            entity.HasOne(x => x.Kerdes)
                .WithMany(x => x.Parok)
                .HasForeignKey(x => x.KerdesId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTesztKerdesek(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TesztKerdes>(entity =>
        {
            entity.ToTable("teszt_kerdesek");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.TesztId).HasColumnName("teszt_id").IsRequired();
            entity.Property(x => x.KerdesId).HasColumnName("kerdes_id").IsRequired();
            entity.Property(x => x.Sorszam).HasColumnName("sorszam").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.Pontszam).HasColumnName("pontszam").HasDefaultValue(1).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

            entity.HasIndex(x => new { x.TesztId, x.KerdesId }).IsUnique();
            entity.HasIndex(x => x.KerdesId);
            entity.HasIndex(x => new { x.TesztId, x.Sorszam });

            entity.HasOne(x => x.Teszt)
                .WithMany(x => x.TesztKerdesek)
                .HasForeignKey(x => x.TesztId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Kerdes)
                .WithMany(x => x.TesztKerdesek)
                .HasForeignKey(x => x.KerdesId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTesztProbalkozasok(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TesztProbalkozas>(entity =>
        {
            entity.ToTable("teszt_probalkozasok");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.TesztId).HasColumnName("teszt_id").IsRequired();
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.Statusz).HasColumnName("statusz").HasColumnType("enum('started','submitted','abandoned')").HasDefaultValue("started").IsRequired();
            entity.Property(x => x.KezdveAt).HasColumnName("kezdve_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.BekuldveAt).HasColumnName("bekuldve_at").HasColumnType("datetime");
            entity.Property(x => x.Pontszam).HasColumnName("pontszam").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.MaxPontszam).HasColumnName("max_pontszam").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.HelyesDb).HasColumnName("helyes_db").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.OsszesKerdesDb).HasColumnName("osszes_kerdes_db").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.ElteltMs).HasColumnName("eltelt_ms");

            entity.HasIndex(x => x.TesztId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Statusz);
            entity.HasIndex(x => x.KezdveAt);

            entity.HasOne(x => x.Teszt)
                .WithMany(x => x.TesztProbalkozasok)
                .HasForeignKey(x => x.TesztId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany(x => x.TesztProbalkozasok)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTesztValaszok(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TesztValasz>(entity =>
        {
            entity.ToTable("teszt_valaszok");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(x => x.ProbalkozasId).HasColumnName("probalkozas_id").IsRequired();
            entity.Property(x => x.KerdesId).HasColumnName("kerdes_id").IsRequired();
            entity.Property(x => x.ValaszSzoveg).HasColumnName("valasz_szoveg").HasColumnType("text");
            entity.Property(x => x.ValaszJson).HasColumnName("valasz_json").HasColumnType("longtext");
            entity.Property(x => x.Helyes).HasColumnName("helyes").HasDefaultValue(false).IsRequired();
            entity.Property(x => x.Pontszam).HasColumnName("pontszam").HasDefaultValue(0).IsRequired();
            entity.Property(x => x.AnsweredAt).HasColumnName("answered_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

            entity.HasIndex(x => x.ProbalkozasId);
            entity.HasIndex(x => x.KerdesId);

            entity.HasOne(x => x.Probalkozas)
                .WithMany(x => x.Valaszok)
                .HasForeignKey(x => x.ProbalkozasId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Kerdes)
                .WithMany(x => x.TesztValaszok)
                .HasForeignKey(x => x.KerdesId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
