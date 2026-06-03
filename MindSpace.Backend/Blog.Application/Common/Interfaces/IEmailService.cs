namespace Blog.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string toName, string verificationToken, CancellationToken cancellationToken = default);
}
