using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using toritanulo.Data;
using toritanulo.DTOs;

namespace toritanulo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TetelekController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public TetelekController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<TetelListItemDto>>> GetAll([FromQuery] string? q)
    {
        var query = _dbContext.Tetelek
            .AsNoTracking()
            .Where(t => t.Aktiv);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var keresett = q.Trim();
            query = query.Where(t =>
                EF.Functions.Like(t.Cim, $"%{keresett}%") ||
                EF.Functions.Like(t.Tartalom, $"%{keresett}%"));
        }

        var tetelek = await query
            .OrderBy(t => t.Sorszam)
            .Select(t => new TetelListItemDto
            {
                Id = t.Id,
                Sorszam = t.Sorszam,
                Cim = t.Cim,
                BekezdesDb = t.BekezdesDb
            })
            .ToListAsync();

        return Ok(tetelek);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TetelDetailDto>> GetById(int id)
    {
        var tetel = await _dbContext.Tetelek
            .AsNoTracking()
            .Where(t => t.Aktiv && t.Id == id)
            .Select(t => new TetelDetailDto
            {
                Id = t.Id,
                Sorszam = t.Sorszam,
                Cim = t.Cim,
                ForrasFajlnev = t.ForrasFajlnev,
                Tartalom = t.Tartalom,
                BekezdesDb = t.BekezdesDb,
                Aktiv = t.Aktiv,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (tetel is null)
        {
            return NotFound(new { message = "A tétel nem található." });
        }

        return Ok(tetel);
    }

    [AllowAnonymous]
    [HttpGet("sorszam/{sorszam:int}")]
    public async Task<ActionResult<TetelDetailDto>> GetBySorszam(int sorszam)
    {
        var tetel = await _dbContext.Tetelek
            .AsNoTracking()
            .Where(t => t.Aktiv && t.Sorszam == sorszam)
            .Select(t => new TetelDetailDto
            {
                Id = t.Id,
                Sorszam = t.Sorszam,
                Cim = t.Cim,
                ForrasFajlnev = t.ForrasFajlnev,
                Tartalom = t.Tartalom,
                BekezdesDb = t.BekezdesDb,
                Aktiv = t.Aktiv,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (tetel is null)
        {
            return NotFound(new { message = "A tétel nem található." });
        }

        return Ok(tetel);
    }
}
