using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using toritanulo.Data;
using toritanulo.DTOs;
using toritanulo.Helpers;
using toritanulo.Models;

namespace toritanulo.Controllers;

[ApiController]
[Route("api/admin/quiz")]
[Authorize(Roles = "Admin")]
public class QuizAdminController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public QuizAdminController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("temakorok")]
    public async Task<ActionResult<List<QuizTemakorDto>>> GetTemakorok()
    {
        var temakorok = await _dbContext.TesztTemakorok
            .AsNoTracking()
            .OrderBy(x => x.Sorszam)
            .ThenBy(x => x.Nev)
            .Select(x => new QuizTemakorDto
            {
                Id = x.Id,
                Kod = x.Kod,
                Nev = x.Nev,
                Leiras = x.Leiras,
                TesztDb = x.Tesztek.Count(t => t.Aktiv)
            })
            .ToListAsync();

        return Ok(temakorok);
    }

    [HttpGet("kerdes-tipusok")]
    public async Task<ActionResult> GetKerdesTipusok()
    {
        var tipusok = await _dbContext.KerdesTipusok
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Kod,
                x.Nev,
                x.Leiras,
                x.Aktiv
            })
            .ToListAsync();

        return Ok(tipusok);
    }

    [HttpGet("kronologia-esemenyek")]
    public async Task<ActionResult> GetKronologiaEsemenyek([FromQuery] int? temakorId = null)
    {
        var query = _dbContext.KronologiaEsemenyek
            .AsNoTracking()
            .Include(x => x.Temakor)
            .AsQueryable();

        if (temakorId.HasValue)
        {
            query = query.Where(x => x.TemakorId == temakorId.Value);
        }

        var esemenyek = await query
            .OrderBy(x => x.RendezesiEv)
            .Select(x => new
            {
                x.Id,
                x.TemakorId,
                TemakorNev = x.Temakor.Nev,
                x.TetelId,
                x.Cim,
                x.EvszamSzoveg,
                x.Idoszamitas,
                x.EvKezd,
                x.EvVeg,
                x.RendezesiEv,
                x.Fontossag,
                x.Aktiv
            })
            .ToListAsync();

        return Ok(esemenyek);
    }

    [HttpGet("kerdesek")]
    public async Task<ActionResult<List<AdminKerdesDto>>> GetKerdesek([FromQuery] int? temakorId = null, [FromQuery] int? tesztId = null)
    {
        var query = _dbContext.Kerdesek
            .AsNoTracking()
            .Include(x => x.Temakor)
            .Include(x => x.KerdesTipus)
            .Include(x => x.KronologiaEsemeny)
            .Include(x => x.HelyesValaszok)
            .Include(x => x.ValaszOpcioK)
            .Include(x => x.Parok)
            .AsQueryable();

        if (temakorId.HasValue)
        {
            query = query.Where(x => x.TemakorId == temakorId.Value);
        }

        if (tesztId.HasValue)
        {
            var kerdesIds = await _dbContext.TesztKerdesek
                .Where(x => x.TesztId == tesztId.Value)
                .Select(x => x.KerdesId)
                .ToListAsync();

            query = query.Where(x => kerdesIds.Contains(x.Id));
        }

        var kerdesek = await query
            .OrderBy(x => x.Id)
            .ToListAsync();

        return Ok(kerdesek.Select(MapKerdes));
    }

    [HttpGet("kerdesek/{id:int}")]
    public async Task<ActionResult<AdminKerdesDto>> GetKerdesById(int id)
    {
        var kerdes = await LoadKerdesGraph()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (kerdes is null)
        {
            return NotFound(new { message = "A kérdés nem található." });
        }

        return Ok(MapKerdes(kerdes));
    }

    [HttpPost("kerdesek")]
    public async Task<ActionResult<AdminKerdesDto>> CreateKerdes(AdminKerdesUpsertDto request)
    {
        var kerdesTipus = await _dbContext.KerdesTipusok
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.KerdesTipusId);

        if (kerdesTipus is null)
        {
            return BadRequest(new { message = "A megadott kérdéstípus nem létezik." });
        }

        var temakorExists = await _dbContext.TesztTemakorok.AnyAsync(x => x.Id == request.TemakorId);
        if (!temakorExists)
        {
            return BadRequest(new { message = "A megadott témakör nem létezik." });
        }

        if (request.KronologiaEsemenyId.HasValue)
        {
            var esemenyExists = await _dbContext.KronologiaEsemenyek.AnyAsync(x => x.Id == request.KronologiaEsemenyId.Value);
            if (!esemenyExists)
            {
                return BadRequest(new { message = "A megadott kronológiai esemény nem létezik." });
            }
        }

        var validationError = ValidateKerdesPayload(kerdesTipus.Kod, request);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var kerdes = new Kerdes();
        ApplyKerdesValues(kerdes, request);
        ReplaceKerdesChildren(kerdes, request, kerdesTipus.Kod);

        _dbContext.Kerdesek.Add(kerdes);
        await _dbContext.SaveChangesAsync();

        var saved = await LoadKerdesGraph()
            .AsNoTracking()
            .FirstAsync(x => x.Id == kerdes.Id);

        return CreatedAtAction(nameof(GetKerdesById), new { id = saved.Id }, MapKerdes(saved));
    }

    [HttpPut("kerdesek/{id:int}")]
    public async Task<ActionResult<AdminKerdesDto>> UpdateKerdes(int id, AdminKerdesUpsertDto request)
    {
        var kerdes = await LoadKerdesGraph().FirstOrDefaultAsync(x => x.Id == id);
        if (kerdes is null)
        {
            return NotFound(new { message = "A kérdés nem található." });
        }

        var kerdesTipus = await _dbContext.KerdesTipusok
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.KerdesTipusId);

        if (kerdesTipus is null)
        {
            return BadRequest(new { message = "A megadott kérdéstípus nem létezik." });
        }

        var temakorExists = await _dbContext.TesztTemakorok.AnyAsync(x => x.Id == request.TemakorId);
        if (!temakorExists)
        {
            return BadRequest(new { message = "A megadott témakör nem létezik." });
        }

        if (request.KronologiaEsemenyId.HasValue)
        {
            var esemenyExists = await _dbContext.KronologiaEsemenyek.AnyAsync(x => x.Id == request.KronologiaEsemenyId.Value);
            if (!esemenyExists)
            {
                return BadRequest(new { message = "A megadott kronológiai esemény nem létezik." });
            }
        }

        var validationError = ValidateKerdesPayload(kerdesTipus.Kod, request);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        ApplyKerdesValues(kerdes, request);

        _dbContext.KerdesHelyesValaszok.RemoveRange(kerdes.HelyesValaszok);
        _dbContext.KerdesValaszOpcioK.RemoveRange(kerdes.ValaszOpcioK);
        _dbContext.KerdesParok.RemoveRange(kerdes.Parok);

        kerdes.HelyesValaszok.Clear();
        kerdes.ValaszOpcioK.Clear();
        kerdes.Parok.Clear();

        ReplaceKerdesChildren(kerdes, request, kerdesTipus.Kod);

        await _dbContext.SaveChangesAsync();

        var saved = await LoadKerdesGraph()
            .AsNoTracking()
            .FirstAsync(x => x.Id == kerdes.Id);

        return Ok(MapKerdes(saved));
    }

    [HttpDelete("kerdesek/{id:int}")]
    public async Task<ActionResult> DeleteKerdes(int id)
    {
        var kerdes = await _dbContext.Kerdesek.FirstOrDefaultAsync(x => x.Id == id);
        if (kerdes is null)
        {
            return NotFound(new { message = "A kérdés nem található." });
        }

        _dbContext.Kerdesek.Remove(kerdes);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "A kérdés törölve lett." });
    }

    [HttpGet("tesztek")]
    public async Task<ActionResult<List<AdminTesztDto>>> GetTesztek([FromQuery] int? temakorId = null)
    {
        var query = _dbContext.Tesztek
            .AsNoTracking()
            .Include(x => x.Temakor)
            .Include(x => x.TesztKerdesek)
                .ThenInclude(x => x.Kerdes)
                    .ThenInclude(x => x.KerdesTipus)
            .AsQueryable();

        if (temakorId.HasValue)
        {
            query = query.Where(x => x.TemakorId == temakorId.Value);
        }

        var tesztek = await query
            .OrderBy(x => x.Id)
            .ToListAsync();

        return Ok(tesztek.Select(MapTeszt));
    }

    [HttpGet("tesztek/{id:int}")]
    public async Task<ActionResult<AdminTesztDto>> GetTesztById(int id)
    {
        var teszt = await LoadTesztGraph()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (teszt is null)
        {
            return NotFound(new { message = "A teszt nem található." });
        }

        return Ok(MapTeszt(teszt));
    }

    [HttpPost("tesztek")]
    public async Task<ActionResult<AdminTesztDto>> CreateTeszt(AdminTesztUpsertDto request)
    {
        var validationError = await ValidateTesztPayload(request);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        if (await _dbContext.Tesztek.AnyAsync(x => x.Slug == request.Slug.Trim()))
        {
            return BadRequest(new { message = "Ez a slug már foglalt." });
        }

        var teszt = new Teszt();
        ApplyTesztValues(teszt, request);
        ReplaceTesztKerdesek(teszt, request);

        _dbContext.Tesztek.Add(teszt);
        await _dbContext.SaveChangesAsync();

        var saved = await LoadTesztGraph()
            .AsNoTracking()
            .FirstAsync(x => x.Id == teszt.Id);

        return CreatedAtAction(nameof(GetTesztById), new { id = saved.Id }, MapTeszt(saved));
    }

    [HttpPut("tesztek/{id:int}")]
    public async Task<ActionResult<AdminTesztDto>> UpdateTeszt(int id, AdminTesztUpsertDto request)
    {
        var teszt = await LoadTesztGraph().FirstOrDefaultAsync(x => x.Id == id);
        if (teszt is null)
        {
            return NotFound(new { message = "A teszt nem található." });
        }

        var validationError = await ValidateTesztPayload(request, id);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        if (await _dbContext.Tesztek.AnyAsync(x => x.Id != id && x.Slug == request.Slug.Trim()))
        {
            return BadRequest(new { message = "Ez a slug már foglalt." });
        }

        ApplyTesztValues(teszt, request);
        _dbContext.TesztKerdesek.RemoveRange(teszt.TesztKerdesek);
        teszt.TesztKerdesek.Clear();
        ReplaceTesztKerdesek(teszt, request);

        await _dbContext.SaveChangesAsync();

        var saved = await LoadTesztGraph()
            .AsNoTracking()
            .FirstAsync(x => x.Id == teszt.Id);

        return Ok(MapTeszt(saved));
    }

    [HttpDelete("tesztek/{id:int}")]
    public async Task<ActionResult> DeleteTeszt(int id)
    {
        var teszt = await _dbContext.Tesztek.FirstOrDefaultAsync(x => x.Id == id);
        if (teszt is null)
        {
            return NotFound(new { message = "A teszt nem található." });
        }

        _dbContext.Tesztek.Remove(teszt);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "A teszt törölve lett." });
    }

    private IQueryable<Kerdes> LoadKerdesGraph()
    {
        return _dbContext.Kerdesek
            .Include(x => x.Temakor)
            .Include(x => x.KerdesTipus)
            .Include(x => x.KronologiaEsemeny)
            .Include(x => x.HelyesValaszok)
            .Include(x => x.ValaszOpcioK)
            .Include(x => x.Parok);
    }

    private IQueryable<Teszt> LoadTesztGraph()
    {
        return _dbContext.Tesztek
            .Include(x => x.Temakor)
            .Include(x => x.TesztKerdesek)
                .ThenInclude(x => x.Kerdes)
                    .ThenInclude(x => x.KerdesTipus);
    }

    private static void ApplyKerdesValues(Kerdes kerdes, AdminKerdesUpsertDto request)
    {
        kerdes.TemakorId = request.TemakorId;
        kerdes.KerdesTipusId = request.KerdesTipusId;
        kerdes.KronologiaEsemenyId = request.KronologiaEsemenyId;
        kerdes.KerdesSzoveg = request.KerdesSzoveg.Trim();
        kerdes.Instrukcio = string.IsNullOrWhiteSpace(request.Instrukcio) ? null : request.Instrukcio.Trim();
        kerdes.Magyarazat = string.IsNullOrWhiteSpace(request.Magyarazat) ? null : request.Magyarazat.Trim();
        kerdes.Nehezseg = (byte)request.Nehezseg;
        kerdes.Aktiv = request.Aktiv;
    }

    private static void ReplaceKerdesChildren(Kerdes kerdes, AdminKerdesUpsertDto request, string kerdesTipusKod)
    {
        foreach (var item in request.HelyesValaszok)
        {
            var valaszSzoveg = string.IsNullOrWhiteSpace(item.ValaszSzoveg) ? null : item.ValaszSzoveg.Trim();
            var normalizalt = string.IsNullOrWhiteSpace(item.NormalizaltValasz)
                ? ResolveNormalizaltValasz(kerdesTipusKod, valaszSzoveg, item.ValaszSzam)
                : item.NormalizaltValasz.Trim();

            kerdes.HelyesValaszok.Add(new KerdesHelyesValasz
            {
                ValaszSzoveg = valaszSzoveg,
                ValaszSzam = item.ValaszSzam,
                Era = string.IsNullOrWhiteSpace(item.Era) ? "NONE" : item.Era.Trim().ToUpperInvariant(),
                NormalizaltValasz = normalizalt
            });
        }

        foreach (var item in request.ValaszOpcioK.OrderBy(x => x.Sorszam))
        {
            kerdes.ValaszOpcioK.Add(new KerdesValaszOpcio
            {
                ValaszSzoveg = item.ValaszSzoveg.Trim(),
                Helyes = item.Helyes,
                HelyesSorrend = item.HelyesSorrend,
                Sorszam = item.Sorszam
            });
        }

        foreach (var item in request.Parok.OrderBy(x => x.Sorszam))
        {
            kerdes.Parok.Add(new KerdesPar
            {
                BalOldal = item.BalOldal.Trim(),
                JobbOldal = item.JobbOldal.Trim(),
                Sorszam = item.Sorszam
            });
        }
    }

    private static string ResolveNormalizaltValasz(string kerdesTipusKod, string? valaszSzoveg, int? valaszSzam)
    {
        if (kerdesTipusKod == "year_input")
        {
            return valaszSzam?.ToString() ?? QuizAnswerNormalizer.NormalizeYear(valaszSzoveg);
        }

        return QuizAnswerNormalizer.NormalizeLooseText(valaszSzoveg ?? valaszSzam?.ToString());
    }

    private static string? ValidateKerdesPayload(string kerdesTipusKod, AdminKerdesUpsertDto request)
    {
        if (string.IsNullOrWhiteSpace(request.KerdesSzoveg))
        {
            return "A kérdés szövege kötelező.";
        }

        if ((kerdesTipusKod == "single_choice" || kerdesTipusKod == "true_false") &&
            (request.ValaszOpcioK.Count < 2 || request.ValaszOpcioK.Count(x => x.Helyes) != 1))
        {
            return "Egyválasztós vagy igaz/hamis kérdésnél legalább 2 opció kell, és pontosan 1 helyes.";
        }

        if (kerdesTipusKod == "multi_choice" &&
            (request.ValaszOpcioK.Count < 2 || request.ValaszOpcioK.Count(x => x.Helyes) < 1))
        {
            return "Többválasztós kérdésnél legalább 2 opció kell, és legalább 1 helyes.";
        }

        if (kerdesTipusKod == "chronology_order")
        {
            var sorrendek = request.ValaszOpcioK.Where(x => x.HelyesSorrend.HasValue).Select(x => x.HelyesSorrend!.Value).ToList();
            if (request.ValaszOpcioK.Count < 2 || sorrendek.Count != request.ValaszOpcioK.Count || sorrendek.Distinct().Count() != request.ValaszOpcioK.Count)
            {
                return "Időrendi sorrend kérdésnél minden opciónak egyedi helyes sorrendet kell adni.";
            }
        }

        if (kerdesTipusKod == "matching" && request.Parok.Count < 2)
        {
            return "Párosítás kérdésnél legalább 2 pár szükséges.";
        }

        if (kerdesTipusKod == "year_input" && request.HelyesValaszok.Count < 1)
        {
            return "Évszám beírása kérdésnél legalább 1 helyes válasz szükséges.";
        }

        return null;
    }

    private async Task<string?> ValidateTesztPayload(AdminTesztUpsertDto request, int? currentTesztId = null)
    {
        if (string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.Cim))
        {
            return "A teszt slug és cím mezője kötelező.";
        }

        var temakorExists = await _dbContext.TesztTemakorok.AnyAsync(x => x.Id == request.TemakorId);
        if (!temakorExists)
        {
            return "A megadott témakör nem létezik.";
        }

        var distinctKerdesIds = request.Kerdesek.Select(x => x.KerdesId).Distinct().ToList();
        if (distinctKerdesIds.Count != request.Kerdesek.Count)
        {
            return "Ugyanaz a kérdés egy tesztben csak egyszer szerepelhet.";
        }

        var kerdesek = await _dbContext.Kerdesek
            .Where(x => distinctKerdesIds.Contains(x.Id))
            .Select(x => new { x.Id, x.TemakorId })
            .ToListAsync();

        if (kerdesek.Count != distinctKerdesIds.Count)
        {
            return "A tesztben szereplő egyik kérdés nem található.";
        }

        if (kerdesek.Any(x => x.TemakorId != request.TemakorId))
        {
            return "A tesztbe csak azonos témakörhöz tartozó kérdések rakhatók.";
        }

        return null;
    }

    private static void ApplyTesztValues(Teszt teszt, AdminTesztUpsertDto request)
    {
        teszt.TemakorId = request.TemakorId;
        teszt.Slug = request.Slug.Trim();
        teszt.Cim = request.Cim.Trim();
        teszt.Leiras = string.IsNullOrWhiteSpace(request.Leiras) ? null : request.Leiras.Trim();
        teszt.TesztTipus = request.TesztTipus.Trim().ToLowerInvariant();
        teszt.Nehezseg = request.Nehezseg.Trim().ToLowerInvariant();
        teszt.IdokeretMp = request.IdokeretMp;
        teszt.Aktiv = request.Aktiv;
    }

    private static void ReplaceTesztKerdesek(Teszt teszt, AdminTesztUpsertDto request)
    {
        foreach (var item in request.Kerdesek.OrderBy(x => x.Sorszam))
        {
            teszt.TesztKerdesek.Add(new TesztKerdes
            {
                KerdesId = item.KerdesId,
                Sorszam = item.Sorszam,
                Pontszam = item.Pontszam < 1 ? 1 : item.Pontszam
            });
        }
    }

    private static AdminKerdesDto MapKerdes(Kerdes kerdes)
    {
        return new AdminKerdesDto
        {
            Id = kerdes.Id,
            TemakorId = kerdes.TemakorId,
            TemakorNev = kerdes.Temakor.Nev,
            KerdesTipusId = kerdes.KerdesTipusId,
            KerdesTipusKod = kerdes.KerdesTipus.Kod,
            KerdesTipusNev = kerdes.KerdesTipus.Nev,
            KronologiaEsemenyId = kerdes.KronologiaEsemenyId,
            KronologiaEsemenyCim = kerdes.KronologiaEsemeny?.Cim,
            KerdesSzoveg = kerdes.KerdesSzoveg,
            Instrukcio = kerdes.Instrukcio,
            Magyarazat = kerdes.Magyarazat,
            Nehezseg = kerdes.Nehezseg,
            Aktiv = kerdes.Aktiv,
            CreatedAt = kerdes.CreatedAt,
            UpdatedAt = kerdes.UpdatedAt,
            HelyesValaszok = kerdes.HelyesValaszok
                .OrderBy(x => x.Id)
                .Select(x => new AdminHelyesValaszDto
                {
                    Id = x.Id,
                    ValaszSzoveg = x.ValaszSzoveg,
                    ValaszSzam = x.ValaszSzam,
                    Era = x.Era,
                    NormalizaltValasz = x.NormalizaltValasz
                })
                .ToList(),
            ValaszOpcioK = kerdes.ValaszOpcioK
                .OrderBy(x => x.Sorszam)
                .ThenBy(x => x.Id)
                .Select(x => new AdminValaszOpcioDto
                {
                    Id = x.Id,
                    ValaszSzoveg = x.ValaszSzoveg,
                    Helyes = x.Helyes,
                    HelyesSorrend = x.HelyesSorrend,
                    Sorszam = x.Sorszam
                })
                .ToList(),
            Parok = kerdes.Parok
                .OrderBy(x => x.Sorszam)
                .ThenBy(x => x.Id)
                .Select(x => new AdminKerdesParDto
                {
                    Id = x.Id,
                    BalOldal = x.BalOldal,
                    JobbOldal = x.JobbOldal,
                    Sorszam = x.Sorszam
                })
                .ToList()
        };
    }

    private static AdminTesztDto MapTeszt(Teszt teszt)
    {
        return new AdminTesztDto
        {
            Id = teszt.Id,
            TemakorId = teszt.TemakorId,
            TemakorNev = teszt.Temakor.Nev,
            Slug = teszt.Slug,
            Cim = teszt.Cim,
            Leiras = teszt.Leiras,
            TesztTipus = teszt.TesztTipus,
            Nehezseg = teszt.Nehezseg,
            IdokeretMp = teszt.IdokeretMp,
            Aktiv = teszt.Aktiv,
            CreatedAt = teszt.CreatedAt,
            UpdatedAt = teszt.UpdatedAt,
            KerdesDb = teszt.TesztKerdesek.Count,
            OsszPont = teszt.TesztKerdesek.Sum(x => x.Pontszam),
            Kerdesek = teszt.TesztKerdesek
                .OrderBy(x => x.Sorszam)
                .Select(x => new AdminTesztKerdesDto
                {
                    Id = x.Id,
                    KerdesId = x.KerdesId,
                    Sorszam = x.Sorszam,
                    Pontszam = x.Pontszam,
                    KerdesSzoveg = x.Kerdes.KerdesSzoveg,
                    KerdesTipusKod = x.Kerdes.KerdesTipus.Kod
                })
                .ToList()
        };
    }
}
