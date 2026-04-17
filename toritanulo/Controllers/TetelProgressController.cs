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
    private const int MaxMentettTetelDb = 5;
    private readonly ApplicationDbContext _dbContext;

    public TetelProgressController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
                VanMentes = false,
                UtolsoMentesAt = null
            });
        }

        return Ok(new TetelProgressDto
        {
            TetelId = mentettAllapot.TetelId,
            HaladasSzazalek = mentettAllapot.HaladasSzazalek,
            VanMentes = true,
            UtolsoMentesAt = mentettAllapot.UpdatedAt
        });
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

        var meglevo = await _dbContext.TetelOlvasasiAllapotok
            .Where(x => x.UserId == userId.Value && x.TetelId == tetelId)
            .FirstOrDefaultAsync();

        if (meglevo is not null)
        {
            meglevo.HaladasSzazalek = request.HaladasSzazalek;
            meglevo.LastOpenedAt = now;
            meglevo.UpdatedAt = now;
        }
        else
        {
            var userMentesei = await _dbContext.TetelOlvasasiAllapotok
                .Where(x => x.UserId == userId.Value)
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            if (userMentesei.Count < MaxMentettTetelDb)
            {
                _dbContext.TetelOlvasasiAllapotok.Add(new TetelOlvasasiAllapot
                {
                    UserId = userId.Value,
                    TetelId = tetelId,
                    HaladasSzazalek = request.HaladasSzazalek,
                    LastOpenedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                var legregebbi = userMentesei
                    .OrderBy(x => x.UpdatedAt)
                    .ThenBy(x => x.Id)
                    .First();

                legregebbi.TetelId = tetelId;
                legregebbi.HaladasSzazalek = request.HaladasSzazalek;
                legregebbi.LastOpenedAt = now;
                legregebbi.UpdatedAt = now;
            }
        }

        await _dbContext.SaveChangesAsync();
        await TrimToLatestFiveAsync(userId.Value);

        return Ok(new TetelProgressDto
        {
            TetelId = tetelId,
            HaladasSzazalek = request.HaladasSzazalek,
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
            .Take(MaxMentettTetelDb)
            .Select(x => new TetelProgressDto
            {
                TetelId = x.TetelId,
                HaladasSzazalek = x.HaladasSzazalek,
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

    private async Task TrimToLatestFiveAsync(int userId)
    {
        var torlendoSorok = await _dbContext.TetelOlvasasiAllapotok
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(MaxMentettTetelDb)
            .ToListAsync();

        if (torlendoSorok.Count == 0)
        {
            return;
        }

        _dbContext.TetelOlvasasiAllapotok.RemoveRange(torlendoSorok);
        await _dbContext.SaveChangesAsync();
    }
}
