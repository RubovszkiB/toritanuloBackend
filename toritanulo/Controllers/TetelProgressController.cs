using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using toritanulo.Data;
using toritanulo.DTOs;
using toritanulo.Models;

namespace toritanulo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TetelProgressController : ControllerBase
{
    private const int RecentTetelDb = 5;
    private readonly ApplicationDbContext _dbContext;

    public TetelProgressController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<TetelProgressDto>>> GetAll()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "A felhasználó azonosítása nem sikerült." });
        }

        var mentettLista = await _dbContext.TetelOlvasasiAllapotok
            .AsNoTracking()
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new TetelProgressDto
            {
                TetelId = x.TetelId,
                HaladasSzazalek = x.HaladasSzazalek,
                LastPage = x.LastPage < 1 ? 1 : x.LastPage,
                ScrollProgress = x.ScrollProgress,
                PageCount = x.PageCount,
                Completed = x.Completed,
                VanMentes = true,
                UtolsoMentesAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(mentettLista);
    }

    [HttpGet("{tetelId:int}")]
    public async Task<ActionResult<TetelProgressDto>> GetByTetelId(int tetelId)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "A felhasználó azonosítása nem sikerült." });
        }

        var mentettAllapot = await _dbContext.TetelOlvasasiAllapotok
            .AsNoTracking()
            .Where(x => x.UserId == userId.Value && x.TetelId == tetelId)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync();

        if (mentettAllapot is null)
        {
            return Ok(new TetelProgressDto
            {
                TetelId = tetelId,
                HaladasSzazalek = 0,
                LastPage = 1,
                ScrollProgress = 0,
                PageCount = 0,
                Completed = false,
                VanMentes = false,
                UtolsoMentesAt = null
            });
        }

        return Ok(ToDto(mentettAllapot, true));
    }

    [HttpPut("{tetelId:int}")]
    public async Task<ActionResult<TetelProgressDto>> Save(int tetelId, [FromBody] SaveTetelProgressRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "A felhasználó azonosítása nem sikerült." });
        }

        var tetelLetezik = await _dbContext.Tetelek
            .AsNoTracking()
            .AnyAsync(t => t.Id == tetelId && t.Aktiv);

        if (!tetelLetezik)
        {
            return NotFound(new { message = "A tétel nem található." });
        }

        var now = DateTime.UtcNow;
        var lastPage = Math.Max(1, request.LastPage);
        var pageCount = Math.Max(0, request.PageCount);
        var scrollProgress = Math.Clamp(request.ScrollProgress, 0, 1);
        var haladasSzazalek = request.Completed ? 100 : Math.Clamp(request.HaladasSzazalek, 0, 100);
        var completed = request.Completed || haladasSzazalek >= 100;

        var meglevo = await _dbContext.TetelOlvasasiAllapotok
            .Where(x => x.UserId == userId.Value && x.TetelId == tetelId)
            .FirstOrDefaultAsync();

        if (meglevo is not null)
        {
            meglevo.HaladasSzazalek = haladasSzazalek;
            meglevo.LastPage = lastPage;
            meglevo.ScrollProgress = scrollProgress;
            meglevo.PageCount = pageCount;
            meglevo.Completed = completed;
            meglevo.LastOpenedAt = now;
            meglevo.UpdatedAt = now;
        }
        else
        {
            _dbContext.TetelOlvasasiAllapotok.Add(new TetelOlvasasiAllapot
            {
                UserId = userId.Value,
                TetelId = tetelId,
                HaladasSzazalek = haladasSzazalek,
                LastPage = lastPage,
                ScrollProgress = scrollProgress,
                PageCount = pageCount,
                Completed = completed,
                LastOpenedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new TetelProgressDto
        {
            TetelId = tetelId,
            HaladasSzazalek = haladasSzazalek,
            LastPage = lastPage,
            ScrollProgress = scrollProgress,
            PageCount = pageCount,
            Completed = completed,
            VanMentes = true,
            UtolsoMentesAt = now
        });
    }

    [HttpGet("recent")]
    public async Task<ActionResult<List<TetelProgressDto>>> GetRecent()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "A felhasználó azonosítása nem sikerült." });
        }

        var mentettLista = await _dbContext.TetelOlvasasiAllapotok
            .AsNoTracking()
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.Id)
            .Take(RecentTetelDb)
            .Select(x => new TetelProgressDto
            {
                TetelId = x.TetelId,
                HaladasSzazalek = x.HaladasSzazalek,
                LastPage = x.LastPage < 1 ? 1 : x.LastPage,
                ScrollProgress = x.ScrollProgress,
                PageCount = x.PageCount,
                Completed = x.Completed,
                VanMentes = true,
                UtolsoMentesAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(mentettLista);
    }

    private int? GetUserId()
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid")
            ?? User.FindFirstValue("sub");

        return int.TryParse(rawUserId, out var userId) ? userId : null;
    }

    private static TetelProgressDto ToDto(TetelOlvasasiAllapot allapot, bool vanMentes)
    {
        return new TetelProgressDto
        {
            TetelId = allapot.TetelId,
            HaladasSzazalek = allapot.HaladasSzazalek,
            LastPage = Math.Max(1, allapot.LastPage),
            ScrollProgress = allapot.ScrollProgress,
            PageCount = allapot.PageCount,
            Completed = allapot.Completed,
            VanMentes = vanMentes,
            UtolsoMentesAt = allapot.UpdatedAt
        };
    }
}
