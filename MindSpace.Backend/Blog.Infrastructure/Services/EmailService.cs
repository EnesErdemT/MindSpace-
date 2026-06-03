using Blog.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Blog.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string toEmail, string toName, string verificationToken, CancellationToken cancellationToken = default)
    {
        var verificationUrl = $"{_settings.AppUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";

        if (string.IsNullOrWhiteSpace(_settings.SmtpUsername) || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            _logger.LogWarning("Setările SMTP nu sunt configurate. Email-ul nu a fost trimis. URL de verificare: {Url}", verificationUrl);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "MindSpace - Verificați adresa de email";
        message.Body = new TextPart("html") { Text = BuildVerificationEmailBody(toName, verificationUrl) };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email de verificare trimis: {Email}", toEmail);
    }

    private static string BuildVerificationEmailBody(string name, string verificationUrl)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="ro">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
            </head>
            <body style="margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,sans-serif;">
                <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f4;padding:40px 0;">
                    <tr>
                        <td align="center">
                            <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.1);">
                                <tr>
                                    <td style="background:linear-gradient(135deg,#7c3aed,#2563eb);padding:32px;text-align:center;">
                                        <h1 style="color:#ffffff;margin:0;font-size:28px;font-weight:bold;">MindSpace</h1>
                                        <p style="color:#e0e7ff;margin:8px 0 0;font-size:14px;">Împărtășește-ți gândurile cu lumea</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:40px 32px;">
                                        <h2 style="color:#1f2937;margin:0 0 16px;">Bună ziua, {name}!</h2>
                                        <p style="color:#4b5563;font-size:16px;line-height:1.6;margin:0 0 24px;">
                                            Bun venit la MindSpace. Vă rugăm să verificați adresa de email pentru a activa contul dvs.
                                        </p>
                                        <div style="text-align:center;margin:32px 0;">
                                            <a href="{verificationUrl}"
                                               style="background:linear-gradient(135deg,#7c3aed,#2563eb);color:#ffffff;text-decoration:none;padding:14px 32px;border-radius:8px;font-size:16px;font-weight:bold;display:inline-block;">
                                                Verifică adresa de email
                                            </a>
                                        </div>
                                        <p style="color:#6b7280;font-size:14px;line-height:1.6;margin:0;">
                                            Acest link este valabil <strong>24 de ore</strong>. Dacă nu ați creat acest cont, puteți ignora acest email.
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="background-color:#f9fafb;padding:24px 32px;text-align:center;">
                                        <p style="color:#9ca3af;font-size:12px;margin:0;">
                                            © {DateTime.UtcNow.Year} MindSpace. Toate drepturile rezervate.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }
}
