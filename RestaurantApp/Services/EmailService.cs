using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestaurantApp.Interfaces;

namespace RestaurantApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _baseUrl;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            _smtpUsername = _configuration["Email:Username"];
            _smtpPassword = _configuration["Email:Password"];
            _senderEmail = _configuration["Email:SenderEmail"];
            _senderName = _configuration["Email:SenderName"];
            _baseUrl = _configuration["Email:BaseUrl"];
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var client = new SmtpClient(_smtpServer)
                {
                    Port = _smtpPort,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, _senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // In a production app, you would log this exception
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        public async Task SendAccountActivationEmailAsync(string email, string username, string userId, string token)
        {
            var activationLink = $"{_baseUrl}/api/Auth/confirm-email?userId={userId}&token={WebUtility.UrlEncode(token)}";
            
            var subject = "Activate Your Account - Delicious Restaurant";
            
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to Delicious Restaurant, {username}!</h2>
                    <p>Thank you for registering with us. Please click the link below to activate your account:</p>
                    <p><a href='{activationLink}'>Activate Account</a></p>
                    <p>Or copy and paste this URL into your browser:</p>
                    <p>{activationLink}</p>
                    <p>This link will expire in 24 hours.</p>
                    <p>Thanks,<br>The Delicious Restaurant Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var resetLink = $"{_baseUrl}/reset-password?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(email)}";
            
            var subject = "Reset Your Password - Delicious Restaurant";
            
            var body = $@"
                <html>
                <body>
                    <h2>Reset Your Password</h2>
                    <p>We received a request to reset your password. Please click the link below to create a new password:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>
                    <p>Or copy and paste this URL into your browser:</p>
                    <p>{resetLink}</p>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you didn't request a password reset, you can ignore this email.</p>
                    <p>Thanks,<br>The Delicious Restaurant Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }
    }
}