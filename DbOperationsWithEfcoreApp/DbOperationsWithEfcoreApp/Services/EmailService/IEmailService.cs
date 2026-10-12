namespace DbOperationsWithEfcoreApp.Services.EmailService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string bodyHtml);
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetLink);
    }
}
