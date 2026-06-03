using Blog.Application.Common.Interfaces;
using Blog.Application.Features.Authentication.DTOs;
using Blog.Application.Features.Authentication.Interfaces;
using Blog.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace Blog.Application.Features.Authentication.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenBlacklistService _blacklist;
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ITokenBlacklistService blacklist,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _blacklist = blacklist;
        _emailService = emailService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _unitOfWork.Users.IsEmailExistsAsync(request.Email))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Această adresă de email este deja utilizată",
                Errors = ["Adresa de email există deja"]
            };
        }

        if (await _unitOfWork.Users.IsUserNameExistsAsync(request.UserName))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Acest nume de utilizator este deja folosit",
                Errors = ["Numele de utilizator există deja"]
            };
        }

        var verificationToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            JoinDate = DateTime.UtcNow,
            EmailConfirmed = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Utilizatorul nu a putut fi creat",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        await _userManager.AddToRoleAsync(user, "Author");

        try
        {
            var displayName = $"{request.FirstName} {request.LastName}".Trim();
            if (string.IsNullOrEmpty(displayName)) displayName = request.UserName;
            await _emailService.SendEmailVerificationAsync(user.Email!, displayName, verificationToken);
        }
        catch (Exception)
        {
            // Chiar dacă emailul nu poate fi trimis, înregistrarea este considerată reușită
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Contul dvs. a fost creat. Vă rugăm să vă verificați adresa de email."
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        User? user = null;

        if (request.EmailOrUserName.Contains("@"))
        {
            user = await _userManager.FindByEmailAsync(request.EmailOrUserName);
        }
        else
        {
            user = await _userManager.FindByNameAsync(request.EmailOrUserName);
        }

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Utilizator negăsit",
                Errors = ["Informații de utilizator invalide"]
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Autentificare eșuată",
                Errors = ["Parolă invalidă"]
            };
        }

        if (!user.EmailConfirmed)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "EMAIL_NOT_VERIFIED",
                Errors = ["Adresa dvs. de email nu a fost încă verificată. Vă rugăm să verificați inbox-ul."]
            };
        }

        var token = await _jwtTokenService.GenerateTokenAsync(user);
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Autentificare reușită",
            Token = token,
            RefreshToken = refreshToken,
            TokenExpiry = DateTime.UtcNow.AddDays(1),
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Token invalid",
                Errors = ["Token-ul nu poate fi validat"]
            };
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Token invalid",
                Errors = ["ID-ul utilizatorului nu a fost găsit"]
            };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Utilizator negăsit",
                Errors = ["Utilizatorul nu există"]
            };
        }

        var newToken = await _jwtTokenService.GenerateTokenAsync(user);
        var newRefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Token reînnoit",
            Token = newToken,
            RefreshToken = newRefreshToken,
            TokenExpiry = DateTime.UtcNow.AddDays(1),
            User = MapToUserDto(user)
        };
    }

    public Task<bool> LogoutAsync(string userId)
    {
        return Task.FromResult(true);
    }

    public Task<bool> RevokeTokenAsync(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var jti = jwt.Id;
            if (!string.IsNullOrEmpty(jti))
                _blacklist.RevokeToken(jti, jwt.ValidTo);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        // Returnează întotdeauna succes pentru a evita enumerarea emailurilor
        if (user == null)
        {
            return new ForgotPasswordResponse
            {
                Success = true,
                Message = "Link-ul de resetare a parolei a fost trimis la adresa dvs. de email"
            };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return new ForgotPasswordResponse
        {
            Success = true,
            Message = "Link-ul de resetare a parolei a fost trimis la adresa dvs. de email",
            ResetToken = token
        };
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Cerere invalidă",
                Errors = ["Utilizator negăsit"]
            };
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Parola nu a putut fi resetată",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Parola a fost resetată cu succes"
        };
    }

    public async Task<AuthResponse> VerifyEmailAsync(string token)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Link de verificare invalid",
                Errors = ["Token-ul nu a fost găsit"]
            };
        }

        if (user.EmailConfirmed)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Această adresă de email este deja verificată",
                Errors = ["Email-ul este deja verificat"]
            };
        }

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Link-ul de verificare a expirat. Vă rugăm să solicitați un link nou.",
                Errors = ["Token-ul a expirat"]
            };
        }

        user.EmailConfirmed = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        var jwtToken = await _jwtTokenService.GenerateTokenAsync(user);
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Adresa dvs. de email a fost verificată cu succes. Bun venit!",
            Token = jwtToken,
            RefreshToken = refreshToken,
            TokenExpiry = DateTime.UtcNow.AddDays(1),
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponse> ResendVerificationEmailAsync(string email)
    {
        // Securitate: returnează același mesaj chiar dacă utilizatorul nu există
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.EmailConfirmed)
        {
            return new AuthResponse
            {
                Success = true,
                Message = "Email-ul de verificare a fost trimis. Vă rugăm să verificați inbox-ul."
            };
        }

        user.EmailVerificationToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        await _userManager.UpdateAsync(user);

        try
        {
            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrEmpty(displayName)) displayName = user.UserName ?? email;
            await _emailService.SendEmailVerificationAsync(user.Email!, displayName, user.EmailVerificationToken);
        }
        catch (Exception)
        {
            // trece în tăcere
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Email-ul de verificare a fost trimis. Vă rugăm să verificați inbox-ul."
        };
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }
        return MapToUserDto(user);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl,
            IsVerified = user.IsVerified,
            FollowerCount = user.FollowerCount,
            FollowingCount = user.FollowingCount,
            JoinDate = user.JoinDate
        };
    }
} 