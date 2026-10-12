using System.Net;
using System.Net.Mail;
using Serilog;

namespace DbOperationsWithEfcoreApp.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            try
            {
                // Read from EmailSettings or Smtp section
                var smtpHost = _configuration["EmailSettings:SmtpServer"] 
                               ?? _configuration["Smtp:Host"] 
                               ?? "sandbox.smtp.mailtrap.io";

                var portString = _configuration["EmailSettings:SmtpPort"] 
                                 ?? _configuration["Smtp:Port"] 
                                 ?? "2525";

                int.TryParse(portString, out int smtpPort);
                if (smtpPort == 0) smtpPort = 2525;

                var smtpUser = _configuration["EmailSettings:SmtpUser"] 
                               ?? _configuration["Smtp:User"];

                var smtpPass = _configuration["EmailSettings:SmtpPass"] 
                               ?? _configuration["Smtp:Pass"];

                var fromEmail = _configuration["EmailSettings:From"] 
                                ?? _configuration["Smtp:From"] 
                                ?? "noreply@bookflowstudio.com";

                if (!string.IsNullOrEmpty(smtpHost) && !string.IsNullOrEmpty(smtpUser))
                {
                    Log.Information("🔌 Connecting to SMTP Host: {Host}:{Port} with User: {User}...", smtpHost, smtpPort, smtpUser);

                    using var client = new SmtpClient(smtpHost, smtpPort)
                    {
                        Credentials = new NetworkCredential(smtpUser, smtpPass),
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false
                    };

                    using var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "BookFlow Studio Security"),
                        Subject = subject,
                        Body = bodyHtml,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    Log.Information("✅ [MAILTRAP SENT] Email successfully dispatched to {Email} with subject '{Subject}'", toEmail, subject);
                }
                else
                {
                    Log.Warning("⚠️ SMTP configuration missing (Host or User is empty). Simulated email for {Email}", toEmail);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ SMTP dispatch failed while sending email to {Email}", toEmail);
                throw; // Rethrow or log so the issue is visible
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetLink)
        {
            var subject = "🔐 Password Reset Request - BookFlow Studio";
            var bodyHtml = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px; background: #ffffff;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h2 style='color: #4f46e5; margin: 0;'>BookFlow Studio</h2>
                        <span style='font-size: 12px; color: #64748b;'>Student Library & Reading Hub</span>
                    </div>
                    <p style='font-size: 15px; color: #1e293b;'>Hello,</p>
                    <p style='color: #475569; font-size: 14px; line-height: 1.5;'>
                        We received a request to reset your password for your BookFlow Studio account. Click the button below to securely choose a new password:
                    </p>
                    <div style='margin: 28px 0; text-align: center;'>
                        <a href='{resetLink}' style='background: linear-gradient(135deg, #4f46e5, #7c3aed); color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 10px; font-weight: bold; font-size: 15px; display: inline-block; box-shadow: 0 4px 12px rgba(79,70,229,0.3);'>Reset My Password</a>
                    </div>
                    <div style='background: #f8fafc; padding: 12px; border-radius: 8px; border: 1px solid #e2e8f0; margin: 20px 0;'>
                        <p style='margin: 0; font-size: 12px; color: #64748b;'>If the button above does not work, copy and paste this link into your browser:</p>
                        <p style='margin: 6px 0 0 0; font-size: 12px; word-break: break-all; color: #4f46e5;'>{resetLink}</p>
                    </div>
                    <p style='font-size: 12px; color: #94a3b8; border-top: 1px solid #f1f5f9; padding-top: 14px; margin-top: 24px;'>
                        ⏳ <strong>Note:</strong> This link is valid for 1 hour. If you did not request a password reset, you can safely ignore this email.
                    </p>
                </div>";

            await SendEmailAsync(toEmail, subject, bodyHtml);
        }
    }
}
