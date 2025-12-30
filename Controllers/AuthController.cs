using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheAfterLifeCMS.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace TheAfterLifeCMS.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly UserManager<MemberIdentityUser> _userManager;
    private readonly SignInManager<MemberIdentityUser> _signInManager;
    private readonly IMemberService _memberService;

    public AuthController(
        UserManager<MemberIdentityUser> userManager,
        SignInManager<MemberIdentityUser> signInManager,
        IMemberService memberService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _memberService = memberService;
    }

    // POST: /auth/register
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["auth_error"] = "Bitte alle Felder korrekt ausfüllen.";
            return Redirect("/registrieren");
        }

        if (model.Password != model.ConfirmPassword)
        {
            TempData["auth_error"] = "Passwörter stimmen nicht überein.";
            return Redirect("/registrieren");
        }

        var existing = await _userManager.FindByEmailAsync(model.Email);
        if (existing != null)
        {
            TempData["auth_error"] = "E-Mail ist bereits registriert.";
            return Redirect("/registrieren");
        }

        // ✅ Alias aus deinem Umbraco: WebsiteUser => siteUser
        var user = MemberIdentityUser.CreateNew(
            model.Email,   // username
            model.Email,   // email
            model.Name,    // name
            true,          // approved
            "siteUser",    // ✅ MemberTypeAlias
            null
        );

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            TempData["auth_error"] = string.Join(" | ", result.Errors.Select(e => e.Description));
            return Redirect("/registrieren");
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return Redirect("/profil");
    }

    // POST: /auth/login
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["auth_error"] = "Ungültige Eingaben.";
            return Redirect("/login");
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            TempData["auth_error"] = "Login fehlgeschlagen (E-Mail/Passwort falsch?).";
            return Redirect("/login");
        }

        return Redirect("/profil");
    }

    // POST: /auth/logout
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/");
    }

    // POST: /profil/save
    [HttpPost("/profil/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProfile(ProfileViewModel model)
    {
        var current = await _userManager.GetUserAsync(User);
        if (current == null)
            return Redirect("/login");

        var member = _memberService.GetByEmail(current.Email!);
        if (member == null)
            return Redirect("/profil");

        member.Name = model.Name ?? "";
        member.Email = model.Email ?? current.Email ?? "";

        member.SetValue("phone", model.Phone);
        member.SetValue("bio", model.Bio);

        _memberService.Save(member);

        TempData["success"] = "Profil gespeichert!";
        return Redirect("/profil");
    }
}
