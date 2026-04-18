using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using toritanulo.Data;
using toritanulo.DTOs;
using toritanulo.Helpers;
using toritanulo.Models;

namespace toritanulo.Controllers;

[ApiController]
[Route("api/quiz")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public QuizController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [AllowAnonymous]
    [HttpGet("temakorok")]
    public async Task<ActionResult<List<QuizTemakorDto>>> GetTemakorok()
    {
        var temakorok = await _dbContext.TesztTemakorok
            .AsNoTracking()
            .Where(x => x.Aktiv)
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

    [AllowAnonymous]
    [HttpGet("tesztek")]
    [HttpGet("tests")]
    public async Task<ActionResult<List<QuizTesztListItemDto>>> GetTesztek([FromQuery] int? temakorId = null, [FromQuery] int? topicId = null, [FromQuery] string? tipus = null, [FromQuery] string? type = null)
    {
        var selectedTemakorId = temakorId ?? topicId;
        var selectedType = string.IsNullOrWhiteSpace(tipus) ? type : tipus;

        var query = _dbContext.Tesztek
            .AsNoTracking()
            .Include(x => x.Temakor)
            .Include(x => x.TesztKerdesek)
            .Where(x => x.Aktiv)
            .AsQueryable();

        if (selectedTemakorId.HasValue)
        {
            query = query.Where(x => x.TemakorId == selectedTemakorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(selectedType))
        {
            query = query.Where(x => x.TesztTipus == selectedType.Trim().ToLower());
        }

        var tesztek = await query
            .OrderBy(x => x.Temakor.Sorszam)
            .ThenBy(x => x.Id)
            .ToListAsync();

        return Ok(tesztek.Select(MapTesztListItem));
    }

    [AllowAnonymous]
    [HttpGet("tesztek/{id:int}")]
    public async Task<ActionResult<QuizTesztReszletekDto>> GetTesztById(int id)
    {
        var teszt = await LoadTesztForPlay()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Aktiv);

        if (teszt is null)
        {
            return NotFound(new { message = "A teszt nem található." });
        }

        return Ok(MapTesztReszletek(teszt));
    }

    [AllowAnonymous]
    [HttpGet("tesztek/slug/{slug}")]
    [HttpGet("tests/{slug}")]
    public async Task<ActionResult<QuizTesztReszletekDto>> GetTesztBySlug(string slug)
    {
        var teszt = await LoadTesztForPlay()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug && x.Aktiv);

        if (teszt is null)
        {
            return NotFound(new { message = "A teszt nem talalhato." });
        }

        return Ok(MapTesztReszletek(teszt));
    }

    [HttpPost("probalkozasok/start")]
    public async Task<ActionResult<QuizAttemptSummaryDto>> StartAttempt(StartQuizAttemptRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var teszt = await _dbContext.Tesztek
            .Include(x => x.TesztKerdesek)
            .FirstOrDefaultAsync(x => x.Id == request.TesztId && x.Aktiv);

        if (teszt is null)
        {
            return NotFound(new { message = "A teszt nem található." });
        }

        var activeQuestionIds = await _dbContext.Kerdesek
            .Where(x => teszt.TesztKerdesek.Select(tk => tk.KerdesId).Contains(x.Id) && x.Aktiv)
            .Select(x => x.Id)
            .ToListAsync();

        var activeTestQuestions = teszt.TesztKerdesek
            .Where(x => activeQuestionIds.Contains(x.KerdesId))
            .ToList();

        var probalkozas = new TesztProbalkozas
        {
            TesztId = teszt.Id,
            UserId = userId.Value,
            Statusz = "started",
            KezdveAt = DateTime.UtcNow,
            Pontszam = 0,
            MaxPontszam = activeTestQuestions.Sum(x => x.Pontszam),
            HelyesDb = 0,
            OsszesKerdesDb = activeTestQuestions.Count
        };

        _dbContext.TesztProbalkozasok.Add(probalkozas);
        await _dbContext.SaveChangesAsync();

        return Ok(new QuizAttemptSummaryDto
        {
            Id = probalkozas.Id,
            TesztId = probalkozas.TesztId,
            TesztCim = teszt.Cim,
            Statusz = probalkozas.Statusz,
            KezdveAt = probalkozas.KezdveAt,
            BekuldveAt = probalkozas.BekuldveAt,
            Pontszam = probalkozas.Pontszam,
            MaxPontszam = probalkozas.MaxPontszam,
            HelyesDb = probalkozas.HelyesDb,
            OsszesKerdesDb = probalkozas.OsszesKerdesDb,
            ElteltMs = probalkozas.ElteltMs
        });
    }

    [HttpGet("probalkozasok/{id:int}")]
    public async Task<ActionResult> GetAttempt(int id)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var probalkozas = await _dbContext.TesztProbalkozasok
            .AsNoTracking()
            .Include(x => x.Teszt)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (probalkozas is null)
        {
            return NotFound(new { message = "A próbálkozás nem található." });
        }

        if (!User.IsInRole("Admin") && probalkozas.UserId != userId.Value)
        {
            return Forbid();
        }

        var teszt = await LoadTesztForPlay()
            .AsNoTracking()
            .FirstAsync(x => x.Id == probalkozas.TesztId);

        return Ok(new
        {
            probalkozas = new QuizAttemptSummaryDto
            {
                Id = probalkozas.Id,
                TesztId = probalkozas.TesztId,
                TesztCim = probalkozas.Teszt.Cim,
                Statusz = probalkozas.Statusz,
                KezdveAt = probalkozas.KezdveAt,
                BekuldveAt = probalkozas.BekuldveAt,
                Pontszam = probalkozas.Pontszam,
                MaxPontszam = probalkozas.MaxPontszam,
                HelyesDb = probalkozas.HelyesDb,
                OsszesKerdesDb = probalkozas.OsszesKerdesDb,
                ElteltMs = probalkozas.ElteltMs
            },
            teszt = MapTesztReszletek(teszt)
        });
    }

    [HttpGet("probalkozasok/sajat")]
    public async Task<ActionResult<List<QuizAttemptSummaryDto>>> GetMyAttempts([FromQuery] int? tesztId = null)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var query = _dbContext.TesztProbalkozasok
            .AsNoTracking()
            .Include(x => x.Teszt)
            .Where(x => x.UserId == userId.Value)
            .AsQueryable();

        if (tesztId.HasValue)
        {
            query = query.Where(x => x.TesztId == tesztId.Value);
        }

        var attempts = await query
            .OrderByDescending(x => x.KezdveAt)
            .Take(25)
            .ToListAsync();

        return Ok(attempts.Select(x => new QuizAttemptSummaryDto
        {
            Id = x.Id,
            TesztId = x.TesztId,
            TesztCim = x.Teszt.Cim,
            Statusz = x.Statusz,
            KezdveAt = x.KezdveAt,
            BekuldveAt = x.BekuldveAt,
            Pontszam = x.Pontszam,
            MaxPontszam = x.MaxPontszam,
            HelyesDb = x.HelyesDb,
            OsszesKerdesDb = x.OsszesKerdesDb,
            ElteltMs = x.ElteltMs
        }));
    }

    [HttpPost("probalkozasok/{id:int}/submit")]
    public async Task<ActionResult<QuizSubmitResultDto>> SubmitAttempt(int id, SubmitQuizAttemptDto request)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var probalkozas = await _dbContext.TesztProbalkozasok
            .Include(x => x.Teszt)
            .Include(x => x.Valaszok)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (probalkozas is null)
        {
            return NotFound(new { message = "A próbálkozás nem található." });
        }

        if (probalkozas.UserId != userId.Value && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (probalkozas.Statusz == "submitted")
        {
            return BadRequest(new { message = "Ez a próbálkozás már be lett küldve." });
        }

        var teszt = await LoadTesztForPlay()
            .FirstAsync(x => x.Id == probalkozas.TesztId);

        _dbContext.TesztValaszok.RemoveRange(probalkozas.Valaszok);
        probalkozas.Valaszok.Clear();

        var requestLookup = request.Valaszok
            .GroupBy(x => x.KerdesId)
            .ToDictionary(x => x.Key, x => x.Last());

        var results = new List<QuizQuestionResultDto>();
        var maxPont = teszt.TesztKerdesek.Sum(x => x.Pontszam);
        var pontszam = 0;
        var helyesDb = 0;

        foreach (var tesztKerdes in teszt.TesztKerdesek.OrderBy(x => x.Sorszam))
        {
            requestLookup.TryGetValue(tesztKerdes.KerdesId, out var bekuldottValasz);

            JsonElement? valaszJson = bekuldottValasz?.ValaszJson;
            string? valaszSzoveg = bekuldottValasz?.ValaszSzoveg;

            var evaluation = QuizEvaluator.Evaluate(tesztKerdes.Kerdes, tesztKerdes.Pontszam, valaszSzoveg, valaszJson);

            var mentettValasz = new TesztValasz
            {
                ProbalkozasId = probalkozas.Id,
                KerdesId = tesztKerdes.KerdesId,
                ValaszSzoveg = valaszSzoveg,
                ValaszJson = valaszJson?.GetRawText(),
                Helyes = evaluation.Helyes,
                Pontszam = evaluation.Pontszam,
                AnsweredAt = DateTime.UtcNow
            };

            probalkozas.Valaszok.Add(mentettValasz);

            pontszam += evaluation.Pontszam;
            if (evaluation.Helyes)
            {
                helyesDb++;
            }

            results.Add(new QuizQuestionResultDto
            {
                KerdesId = tesztKerdes.KerdesId,
                Helyes = evaluation.Helyes,
                Pontszam = evaluation.Pontszam,
                MaxPontszam = tesztKerdes.Pontszam,
                HelyesValaszOsszegzes = evaluation.HelyesValaszOsszegzes,
                Magyarazat = tesztKerdes.Kerdes.Magyarazat
            });
        }

        probalkozas.Statusz = "submitted";
        probalkozas.BekuldveAt = DateTime.UtcNow;
        probalkozas.Pontszam = pontszam;
        probalkozas.MaxPontszam = maxPont;
        probalkozas.HelyesDb = helyesDb;
        probalkozas.OsszesKerdesDb = teszt.TesztKerdesek.Count;
        probalkozas.ElteltMs = request.ElteltMs;

        await _dbContext.SaveChangesAsync();

        return Ok(new QuizSubmitResultDto
        {
            Probalkozas = new QuizAttemptSummaryDto
            {
                Id = probalkozas.Id,
                TesztId = probalkozas.TesztId,
                TesztCim = probalkozas.Teszt.Cim,
                Statusz = probalkozas.Statusz,
                KezdveAt = probalkozas.KezdveAt,
                BekuldveAt = probalkozas.BekuldveAt,
                Pontszam = probalkozas.Pontszam,
                MaxPontszam = probalkozas.MaxPontszam,
                HelyesDb = probalkozas.HelyesDb,
                OsszesKerdesDb = probalkozas.OsszesKerdesDb,
                ElteltMs = probalkozas.ElteltMs
            },
            KerdesEredmenyek = results
        });
    }

    private IQueryable<Teszt> LoadTesztForPlay()
    {
        return _dbContext.Tesztek
            .Include(x => x.Temakor)
            .Include(x => x.TesztKerdesek.OrderBy(tk => tk.Sorszam))
                .ThenInclude(x => x.Kerdes)
                    .ThenInclude(x => x.KerdesTipus)
            .Include(x => x.TesztKerdesek)
                .ThenInclude(x => x.Kerdes)
                    .ThenInclude(x => x.ValaszOpcioK)
            .Include(x => x.TesztKerdesek)
                .ThenInclude(x => x.Kerdes)
                    .ThenInclude(x => x.Parok)
            .Include(x => x.TesztKerdesek)
                .ThenInclude(x => x.Kerdes)
                    .ThenInclude(x => x.HelyesValaszok)
            .Where(x => x.Aktiv);
    }

    private static QuizTesztListItemDto MapTesztListItem(Teszt teszt)
    {
        return new QuizTesztListItemDto
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
            KerdesDb = teszt.TesztKerdesek.Count,
            OsszPont = teszt.TesztKerdesek.Sum(x => x.Pontszam)
        };
    }

    private static QuizTesztReszletekDto MapTesztReszletek(Teszt teszt)
    {
        return new QuizTesztReszletekDto
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
            KerdesDb = teszt.TesztKerdesek.Count,
            OsszPont = teszt.TesztKerdesek.Sum(x => x.Pontszam),
            Kerdesek = teszt.TesztKerdesek
                .Where(x => x.Kerdes.Aktiv)
                .OrderBy(x => x.Sorszam)
                .Select(x => new QuizKerdesDto
                {
                    Id = x.KerdesId,
                    KerdesTipusKod = x.Kerdes.KerdesTipus.Kod,
                    KerdesTipusNev = x.Kerdes.KerdesTipus.Nev,
                    KerdesSzoveg = x.Kerdes.KerdesSzoveg,
                    Instrukcio = x.Kerdes.Instrukcio,
                    Magyarazat = x.Kerdes.Magyarazat,
                    Pontszam = x.Pontszam,
                    Nehezseg = x.Kerdes.Nehezseg,
                    ValaszOpcioK = x.Kerdes.ValaszOpcioK
                        .OrderBy(v => v.Sorszam)
                        .Select(v => new QuizValaszOpcioDto
                        {
                            Id = v.Id,
                            ValaszSzoveg = v.ValaszSzoveg,
                            Helyes = v.Helyes,
                            HelyesSorrend = v.HelyesSorrend,
                            Sorszam = v.Sorszam
                        })
                        .ToList(),
                    Parok = x.Kerdes.Parok
                        .OrderBy(p => p.Sorszam)
                        .Select(p => new QuizKerdesParDto
                        {
                            BalOldal = p.BalOldal,
                            JobbOldal = p.JobbOldal,
                            Sorszam = p.Sorszam
                        })
                        .ToList(),
                    HelyesValaszok = x.Kerdes.HelyesValaszok
                        .OrderBy(h => h.Id)
                        .Select(h => new QuizHelyesValaszDto
                        {
                            ValaszSzoveg = h.ValaszSzoveg,
                            ValaszSzam = h.ValaszSzam,
                            Era = h.Era,
                            NormalizaltValasz = h.NormalizaltValasz
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
