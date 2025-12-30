using System.ComponentModel.DataAnnotations;

namespace TheAfterLifeCMS.Models;

public class RegisterModel
{
    [Required]
    public string Name { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";
}
