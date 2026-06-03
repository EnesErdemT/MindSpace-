using Blog.Application.Features.Authentication.DTOs;
using Blog.Application.Features.Authentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IHostEnvironment _env;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, IHostEnvironment env)
    {
        _authService = authService;
        _logger = logger;
        _env = env;
    }
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(AuthResponse), 400)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Eroare de validare",
                    Errors = errors
                });
            }

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Utilizator nou înregistrat: {UserName}", request.UserName);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la înregistrare: {Message}", ex.Message);
            
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "A apărut o eroare",
                Errors = ["Eroare de server"]
            });
        }
    }
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(AuthResponse), 400)]
    [ProducesResponseType(typeof(AuthResponse), 401)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Eroare de validare",
                    Errors = errors
                });
            }

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            _logger.LogInformation("Utilizator autentificat: {EmailOrUserName}", request.EmailOrUserName);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la autentificare: {Message}", ex.Message);
            
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "A apărut o eroare",
                Errors = ["Eroare de server"]
            });
        }
    }
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(AuthResponse), 400)]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromQuery] string token, [FromQuery] string refreshToken)
    {
        try
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Token-ul și token-ul de reîmprospătare sunt necesare",
                    Errors = ["Parametri lipsă"]
                });
            }

            var result = await _authService.RefreshTokenAsync(token, refreshToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Token reînnoit");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la reînnoirea token-ului: {Message}", ex.Message);
            
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "A apărut o eroare",
                Errors = ["Eroare de server"]
            });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
                await _authService.RevokeTokenAsync(token);

            if (!string.IsNullOrEmpty(userId))
                _logger.LogInformation("Utilizator deconectat: {UserId}", userId);

            return Ok(new { success = true, message = "Deconectare reușită" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la deconectare");
            return BadRequest(new { success = false, message = "A apărut o eroare" });
        }
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { Message = "Adresa de email este obligatorie" });

            var result = await _authService.ForgotPasswordAsync(request.Email);

            // Only return token in Development to avoid leaking it in production
            var response = _env.IsDevelopment()
                ? new { result.Success, result.Message, result.ResetToken }
                : (object)new { result.Success, result.Message };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la ForgotPassword");
            return StatusCode(500, new { Message = "A apărut o eroare" });
        }
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(AuthResponse), 400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(request);
            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Parola a fost resetată: {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la ResetPassword");
            return StatusCode(500, new { Message = "A apărut o eroare" });
        }
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return Content(BuildVerificationHtml(false, "Link de verificare invalid."), "text/html");

            var result = await _authService.VerifyEmailAsync(token);

            return Content(BuildVerificationHtml(result.Success, result.Message ?? "A apărut o eroare."), "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la VerifyEmail");
            return Content(BuildVerificationHtml(false, "A apărut o eroare, vă rugăm să încercați din nou."), "text/html");
        }
    }

    private static string BuildVerificationHtml(bool success, string message) => $$"""
        <!DOCTYPE html>
        <html lang="ro">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Verificare Email — MindSpace</title>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #f5f5f5; display: flex; align-items: center; justify-content: center; min-height: 100vh; }
                .card { background: white; border-radius: 16px; padding: 48px 40px; max-width: 420px; width: 90%; text-align: center; box-shadow: 0 4px 24px rgba(0,0,0,0.08); }
                .icon { font-size: 56px; margin-bottom: 24px; }
                h1 { font-size: 22px; font-weight: 700; color: #111; margin-bottom: 12px; }
                p { font-size: 15px; color: #555; line-height: 1.6; margin-bottom: 32px; }
                a { display: inline-block; padding: 12px 32px; border-radius: 8px; font-size: 15px; font-weight: 600; text-decoration: none; background: {{(success ? "#111" : "#e53e3e")}}; color: white; }
                a:hover { opacity: 0.85; }
            </style>
        </head>
        <body>
            <div class="card">
                <div class="icon">{{(success ? "✅" : "❌")}}</div>
                <h1>{{(success ? "Email Verificat!" : "Verificarea a eșuat")}}</h1>
                <p>{{message}}</p>
                <a href="http://localhost:3000">{{(success ? "Autentifică-te" : "Înapoi la pagina principală")}}</a>
            </div>
        </body>
        </html>
        """;

    [HttpPost("resend-verification")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { Message = "Adresa de email este obligatorie" });

            var result = await _authService.ResendVerificationEmailAsync(request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la ResendVerification");
            return StatusCode(500, new { Message = "A apărut o eroare" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var user = await _authService.GetCurrentUserAsync(userId);
            
            if (user == null)
            {
                return NotFound("Utilizator negăsit");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la GetCurrentUser: {Message}", ex.Message);
            return BadRequest(new { success = false, message = "A apărut o eroare" });
        }
    }
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResendVerificationRequest
{
    public string Email { get; set; } = string.Empty;
}