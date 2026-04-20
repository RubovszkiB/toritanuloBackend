using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using toritanulo.Data;
using toritanulo.DTOs;
using toritanulo.Helpers;
using toritanulo.Models;

namespace toritanulo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(ApplicationDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(new { message = "A felhasználó nem található." });
        }

        return Ok(ToUserResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create(CreateUserRequestDto request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var fullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim();

        if (await _dbContext.Users.AnyAsync(u => u.Username == username))
        {
            return BadRequest(new { message = "Ez a felhasználónév már foglalt." });
        }

        if (await _dbContext.Users.AnyAsync(u => u.Email == email))
        {
            return BadRequest(new { message = "Ez az email cím már használatban van." });
        }

        var user = new User
        {
            Username = username,
            Email = email,
            FullName = fullName,
            Role = RoleHelper.Student,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToUserResponse(user));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> Update(int id, UpdateUserRequestDto request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(new { message = "A felhasználó nem található." });
        }

        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var fullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim();

        if (IsCurrentUser(id) && !request.IsActive)
        {
            return BadRequest(new { message = "A saját admin fiókodat nem inaktiválhatod." });
        }

        if (await _dbContext.Users.AnyAsync(u => u.Id != id && u.Username == username))
        {
            return BadRequest(new { message = "Ez a felhasználónév már foglalt." });
        }

        if (await _dbContext.Users.AnyAsync(u => u.Id != id && u.Email == email))
        {
            return BadRequest(new { message = "Ez az email cím már használatban van." });
        }

        user.Username = username;
        user.Email = email;
        user.FullName = fullName;
        user.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync();
        return Ok(ToUserResponse(user));
    }

    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> SetPassword(int id, SetPasswordRequestDto request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(new { message = "A felhasználó nem található." });
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "A jelszó sikeresen módosítva lett." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(new { message = "A felhasználó nem található." });
        }

        if (IsCurrentUser(id))
        {
            return BadRequest(new { message = "A saját admin fiókodat nem törölheted." });
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "A felhasználó törölve lett." });
    }

    private static UserResponseDto ToUserResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private bool IsCurrentUser(int userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserId, out var parsedUserId) && parsedUserId == userId;
    }
}
