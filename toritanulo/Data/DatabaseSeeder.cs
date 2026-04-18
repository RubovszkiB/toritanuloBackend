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

        await SeedSzemelyQuizAsync(dbContext);

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedSzemelyQuizAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Tesztek.AnyAsync(x => x.TesztTipus == "szemely"))
        {
            return;
        }

        var tipusok = await dbContext.KerdesTipusok.ToDictionaryAsync(x => x.Kod);

        var temakor = await dbContext.TesztTemakorok.FirstOrDefaultAsync(x => x.Kod == "szemely-emelt");
        if (temakor is null)
        {
            temakor = new TesztTemakor
            {
                Kod = "szemely-emelt",
                Nev = "Szemely kviz - emelt tortenelem",
                Leiras = "Szemelyekhez, fogalmakhoz es szerepekhez kapcsolodo emelt szintu gyakorlas a tetelek es a vizsgakovetelmeny alapjan.",
                Sorszam = 90,
                Aktiv = true
            };

            dbContext.TesztTemakorok.Add(temakor);
            await dbContext.SaveChangesAsync();
        }

        var reform = new Teszt
        {
            TemakorId = temakor.Id,
            Slug = "szemely-kviz-reformkor-1848",
            Cim = "Szemely kviz: reformkor es 1848-49",
            Leiras = "Szereplok, programok, miniszterek es katonai vezetok az emelt tortenelem szobeli tetelekbol.",
            TesztTipus = "szemely",
            Nehezseg = "kozepes",
            IdokeretMp = 900,
            Aktiv = true
        };

        var magyar = new Teszt
        {
            TemakorId = temakor.Id,
            Slug = "szemely-kviz-magyar-allam-es-habsburgok",
            Cim = "Szemely kviz: magyar allam, Erdely, Habsburgok",
            Leiras = "Kozepkori es kora ujkori magyar uralkodok, erdelyi fejedelmek, rendi szereplok.",
            TesztTipus = "szemely",
            Nehezseg = "kozepes",
            IdokeretMp = 900,
            Aktiv = true
        };

        var egyetemes = new Teszt
        {
            TemakorId = temakor.Id,
            Slug = "szemely-kviz-egyetemes-eszmek-vilaghaboruk",
            Cim = "Szemely kviz: egyetemes tortenelem es vilaghaboruk",
            Leiras = "Reformacio, felvilagosodas, forradalmak, diktaturak es hideghaboru fontos szemelyei.",
            TesztTipus = "szemely",
            Nehezseg = "nehez",
            IdokeretMp = 1020,
            Aktiv = true
        };

        dbContext.Tesztek.AddRange(reform, magyar, egyetemes);
        await dbContext.SaveChangesAsync();

        AddQuestion(dbContext, reform, temakor.Id, tipusok["single_choice"].Id, 1, 1,
            "Kihez kotheto a Hitel cimu mu es a mersekelt, arisztokraciara epito reformprogram?",
            "Valaszd ki a reformkori politikust.",
            "Szechenyi Istvan a Hitel, a Stadium es a Vilag szerzoje; programja fokozatos, a birodalmon beluli reformokra epult.",
            new[] { ("Szechenyi Istvan", true), ("Kossuth Lajos", false), ("Deak Ferenc", false), ("Wesselenyi Miklos", false) });

        AddQuestion(dbContext, reform, temakor.Id, tipusok["single_choice"].Id, 2, 1,
            "Ki kepviselte legerosebben a vedegyleten es iparfejlesztesen alapulo reformprogramot?",
            null,
            "Kossuth Lajos a kozepnemessegre tamaszkodva gyorsabb polgari atalakulast es vedovamos iparfejlesztest szorgalmazott.",
            new[] { ("Kossuth Lajos", true), ("Szechenyi Istvan", false), ("Batthyany Lajos", false), ("Eotvos Jozsef", false) });

        AddQuestion(dbContext, reform, temakor.Id, tipusok["matching"].Id, 3, 2,
            "Parositsd az 1848-as Batthyany-kormany tagjait a teruletukkel.",
            "Minden bal oldali nevhez valaszd ki a megfelelo szerepet.",
            "Az elso felelos magyar kormany szemelyi osszetetele alapveto emelt szintu azonositas.",
            pairs: new[] { ("Batthyany Lajos", "miniszterelnok"), ("Kossuth Lajos", "penzugyminiszter"), ("Szechenyi Istvan", "kozlekedesugyi miniszter"), ("Deak Ferenc", "igazsagugyi miniszter") });

        AddQuestion(dbContext, reform, temakor.Id, tipusok["true_false"].Id, 4, 1,
            "Deak Ferencet a kiegyzes politikai elokeszitese miatt gyakran a haza bolcsekent emlegetik.",
            null,
            "Deak a passziv ellenallas es a kiegyzes egyik kulcsszereploje volt.",
            new[] { ("Igaz", true), ("Hamis", false) });

        AddQuestion(dbContext, reform, temakor.Id, tipusok["multi_choice"].Id, 5, 2,
            "Mely szemelyek kothetok kozvetlenul az 1848-49-es szabadsagharchoz?",
            "Tobb helyes valasz is lehet.",
            "A felsoroltak kozul Jellasics, Gorgei es Szemere kozvetlenul szerepeltek az 1848-49-es esemenyekben.",
            new[] { ("Jellasics ban", true), ("Gorgei Artur", true), ("Szemere Bertalan", true), ("Bethlen Gabor", false) });

        AddQuestion(dbContext, reform, temakor.Id, tipusok["chronology_order"].Id, 6, 2,
            "Allitsd idorendbe a reformkor es szabadsagharc szemelyekhez kotheto fordulopontjait.",
            null,
            "A sorrend: Szechenyi felajanlasa, Kossuth Pesti Hirlapja, Batthyany-kormany, Gorgei vilagosi fegyverletetele.",
            optionsWithOrder: new[] { ("Szechenyi felajanlasa az Akademia javara", 1), ("Kossuth a Pesti Hirlap szerkesztoje", 2), ("Batthyany Lajos kormanyalakitasa", 3), ("Gorgei Artur vilagosi fegyverletetele", 4) });

        AddQuestion(dbContext, magyar, temakor.Id, tipusok["single_choice"].Id, 1, 1,
            "Melyik uralkodo vezette be az aranyforintot es erositette meg a kiralyi jovedelemeket a 14. szazadban?",
            null,
            "Karoly Robert banyareformjai es kapuadoja a 14. szazadi magyar gazdasag fontos elemei.",
            new[] { ("Karoly Robert", true), ("I. Nagy Lajos", false), ("Hunyadi Matyas", false), ("IV. Bela", false) });

        AddQuestion(dbContext, magyar, temakor.Id, tipusok["single_choice"].Id, 2, 1,
            "Kihez kotheto a fekete sereg es a reneszansz kiralyi udvar megerositese Magyarorszagon?",
            null,
            "Hunyadi Matyas kozpontosito politikaja, zsoldosserege es udvari kulturaja kiemelt tetelanyag.",
            new[] { ("Hunyadi Matyas", true), ("Hunyadi Janos", false), ("Zsigmond kiraly", false), ("II. Lajos", false) });

        AddQuestion(dbContext, magyar, temakor.Id, tipusok["matching"].Id, 3, 2,
            "Parositsd a magyar es erdelyi szereploket a legjellemzobb szerepukkel.",
            null,
            "A teteleitekben Bethlen, Zrinyi es Rakoczi kulon kiemelt szemelykent jelennek meg.",
            pairs: new[] { ("Bethlen Gabor", "erdelyi fejedelem, gazdasagi es kulturpolitikai erosites"), ("Zrinyi Miklos", "torokellenes hadvezer es politikai iro"), ("II. Rakoczi Ferenc", "a szabadsagharc vezerlo fejedelme"), ("Maria Terezia", "uralkodo, vamrendelet es urbarium") });

        AddQuestion(dbContext, magyar, temakor.Id, tipusok["true_false"].Id, 4, 1,
            "Bethlen Gabor Erdely fejedelmekent a harminceves haboru idejen is aktiv kulpolitikat folytatott.",
            null,
            "Bethlen kulpolitikaja a Habsburgokkal es a protestans rendekkel valo viszony miatt emelt szinten is fontos.",
            new[] { ("Igaz", true), ("Hamis", false) });

        AddQuestion(dbContext, magyar, temakor.Id, tipusok["multi_choice"].Id, 5, 2,
            "Kik kothetok a torokellenes vagy Habsburg-ellenes magyar kuzdelmekhez?",
            "Tobb helyes valasz is lehet.",
            "Zrinyi, Bocskai es Rakoczi mas-mas korszakban, de magyar rendi/torokellenes/Habsburg-ellenes kuzdelmek szereploi.",
            new[] { ("Zrinyi Miklos", true), ("Bocskai Istvan", true), ("II. Rakoczi Ferenc", true), ("Oliver Cromwell", false) });

        AddQuestion(dbContext, magyar, temakor.Id, tipusok["chronology_order"].Id, 6, 2,
            "Allitsd idorendbe a magyar tortenelem szemelyeit korszakuk szerint.",
            null,
            "A szemelyek idorendje: Szent Istvan, Karoly Robert, Hunyadi Matyas, Bethlen Gabor.",
            optionsWithOrder: new[] { ("Szent Istvan", 1), ("Karoly Robert", 2), ("Hunyadi Matyas", 3), ("Bethlen Gabor", 4) });

        AddQuestion(dbContext, egyetemes, temakor.Id, tipusok["single_choice"].Id, 1, 1,
            "Ki inditotta el a reformaciot a 95 tetel kozzetetelevel?",
            null,
            "Luther Marton fellepese a nyugati keresztenyseg tortenetenek egyik fordulopontja.",
            new[] { ("Luther Marton", true), ("Kalvin Janos", false), ("Loyolai Ignac", false), ("Erasmus", false) });

        AddQuestion(dbContext, egyetemes, temakor.Id, tipusok["single_choice"].Id, 2, 1,
            "Kihez kotheto a predestinacio tana es a genfi reformacio megszervezese?",
            null,
            "Kalvin Janos tanitasa es egyhazszervezete a reformacio masodik nagy iranyat jelentette.",
            new[] { ("Kalvin Janos", true), ("Luther Marton", false), ("XIV. Lajos", false), ("Robespierre", false) });

        AddQuestion(dbContext, egyetemes, temakor.Id, tipusok["matching"].Id, 3, 2,
            "Parositsd a szemelyeket a tortenelmi fogalommal vagy jelenseggel.",
            null,
            "A feladat az emelt kovetelmeny szerinti szemely-fogalom kapcsolatot gyakoroltatja.",
            pairs: new[] { ("Montesquieu", "hatalmi agak megosztasa"), ("Rousseau", "nepfelseg elve"), ("Robespierre", "jakobinus diktatura"), ("Napoleon", "polgari torvenykonyv es csaszarsag") });

        AddQuestion(dbContext, egyetemes, temakor.Id, tipusok["multi_choice"].Id, 4, 2,
            "Kik voltak a masodik vilaghaboru es a haboru utani rendezodes meghatarozo vezeto szemelyei?",
            "Tobb helyes valasz is lehet.",
            "Churchill, Roosevelt es Sztalin a szovetseges nagyhatalmak meghatarozo vezetoikent jelentek meg.",
            new[] { ("Winston Churchill", true), ("Franklin D. Roosevelt", true), ("Sztalin", true), ("Luther Marton", false) });

        AddQuestion(dbContext, egyetemes, temakor.Id, tipusok["true_false"].Id, 5, 1,
            "Hitler hatalomra jutasa utan Nemetorszagban totalitarius diktatura epult ki.",
            null,
            "A nemzetiszocialista diktatura mukodese es ideologiaja alapkovetelmeny a 20. szazadi temaknal.",
            new[] { ("Igaz", true), ("Hamis", false) });

        AddQuestion(dbContext, egyetemes, temakor.Id, tipusok["chronology_order"].Id, 6, 2,
            "Allitsd idorendbe a szemelyeket a hozzajuk kotheto korszak szerint.",
            null,
            "A sorrend: Luther, XIV. Lajos, Napoleon, Sztalin.",
            optionsWithOrder: new[] { ("Luther Marton", 1), ("XIV. Lajos", 2), ("Napoleon", 3), ("Sztalin", 4) });
    }

    private static void AddQuestion(
        ApplicationDbContext dbContext,
        Teszt teszt,
        int temakorId,
        int kerdesTipusId,
        int sorszam,
        int pontszam,
        string kerdesSzoveg,
        string? instrukcio,
        string magyarazat,
        IEnumerable<(string Text, bool Correct)>? options = null,
        IEnumerable<(string Left, string Right)>? pairs = null,
        IEnumerable<(string Text, int Order)>? optionsWithOrder = null)
    {
        var kerdes = new Kerdes
        {
            TemakorId = temakorId,
            KerdesTipusId = kerdesTipusId,
            KerdesSzoveg = kerdesSzoveg,
            Instrukcio = instrukcio,
            Magyarazat = magyarazat,
            Nehezseg = (byte)Math.Clamp(pontszam, 1, 3),
            Aktiv = true
        };

        var optionIndex = 1;
        foreach (var option in options ?? Array.Empty<(string Text, bool Correct)>())
        {
            kerdes.ValaszOpcioK.Add(new KerdesValaszOpcio
            {
                ValaszSzoveg = option.Text,
                Helyes = option.Correct,
                Sorszam = optionIndex++
            });
        }

        optionIndex = 1;
        foreach (var option in optionsWithOrder ?? Array.Empty<(string Text, int Order)>())
        {
            kerdes.ValaszOpcioK.Add(new KerdesValaszOpcio
            {
                ValaszSzoveg = option.Text,
                Helyes = true,
                HelyesSorrend = option.Order,
                Sorszam = optionIndex++
            });
        }

        var pairIndex = 1;
        foreach (var pair in pairs ?? Array.Empty<(string Left, string Right)>())
        {
            kerdes.Parok.Add(new KerdesPar
            {
                BalOldal = pair.Left,
                JobbOldal = pair.Right,
                Sorszam = pairIndex++
            });
        }

        teszt.TesztKerdesek.Add(new TesztKerdes
        {
            Kerdes = kerdes,
            Sorszam = sorszam,
            Pontszam = pontszam
        });
    }
}
