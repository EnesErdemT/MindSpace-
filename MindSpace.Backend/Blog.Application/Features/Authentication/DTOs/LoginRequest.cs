using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Features.Authentication.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Email-ul sau numele de utilizator este obligatoriu")]
    public string EmailOrUserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
} 