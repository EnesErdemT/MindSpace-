using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Features.Authentication.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Câmpul email este obligatoriu")]
    [EmailAddress(ErrorMessage = "Introduceți o adresă de email validă")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numele de utilizator este obligatoriu")]
    [MinLength(3, ErrorMessage = "Numele de utilizator trebuie să aibă cel puțin 3 caractere")]
    [MaxLength(50, ErrorMessage = "Numele de utilizator poate avea maximum 50 de caractere")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie")]
    [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmarea parolei este obligatorie")]
    [Compare("Password", ErrorMessage = "Parolele nu se potrivesc")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Prenumele poate avea maximum 50 de caractere")]
    public string? FirstName { get; set; }

    [MaxLength(50, ErrorMessage = "Numele poate avea maximum 50 de caractere")]
    public string? LastName { get; set; }
} 