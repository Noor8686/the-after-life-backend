using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheAfterLifeCMS.Models;
using Umbraco.Cms.Core.Security;

namespace TheAfterLifeCMS.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<MemberIdentityUser> _userManager;
    private readonly SignInManager<MemberIdentityUser> _signInManager;

    public AuthApiController(
        UserManager<MemberIdentityUser> userManager,
        SignInManager<MemberIdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // POST: /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { ok = false, error = "Bitte alle Felder korrekt ausfüllen." });

        if (model.Password != model.ConfirmPassword)
            return BadRequest(new { ok = false, error = "Passwörter stimmen nicht überein." });

        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing != null)
            return BadRequest(new { ok = false, error = "E-Mail ist bereits registriert." });

        var user = MemberIdentityUser.CreateNew(
            model.Email,
            model.Email,
            model.Name,
            true,
            "siteUser",   // ✅ Dein Alias aus Umbraco
            null
        );

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var msg = string.Join(" | ", result.Errors.Select(e => e.Description));
            return BadRequest(new { ok = false, error = msg });
        }

        // ✅ setzt Auth-Cookie (wichtig für "eingeloggt bleiben")
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok(new { ok = true });
    }

    // POST: /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { ok = false, error = "Ungültige Eingaben." });

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false
        );

        if (!result.Succeeded)
            return Unauthorized(new { ok = false, error = "Login fehlgeschlagen." });

        return Ok(new { ok = true });
    }

    // POST: /api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { ok = true });
    }

    // GET: /api/auth/me  (zum prüfen ob eingeloggt)
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var current = await _userManager.GetUserAsync(User);
        if (current == null) return Unauthorized(new { ok = false });

        return Ok(new
        {
            ok = true,
            name = current.Name,
            email = current.Email
        });
    }
}
