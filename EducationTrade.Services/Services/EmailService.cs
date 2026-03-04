using EducationTrade.Core.Helpers;
using EducationTrade.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendVerificationEmailAsync(string toEmail, string userName, string link)
        {
            var subject = "Verify Your Education Trade Account";
            var body = $@"
                <h2>Welcome {userName}!</h2>
                <p>Click the link below to verify your email:</p>
                <a href='{link}' style='background:#667eea; color:white; padding:10px 20px; text-decoration:none;'>
                    Verify Email
                </a>
                <p>Or copy this link: {link}</p>
                <p>Link expires in 24 hours.</p>";

            await SendEmail(toEmail, subject, body);
        }
        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to Education Trade!";
            var body = $@"
                <h2>Welcome {userName}!</h2>
                <p>Your email is verified! You received 200 coins.</p>
                <p>Start browsing tasks now!</p>";

            await SendEmail(toEmail, subject, body);
        }
        private async Task SendEmail(string toEmail, string subject, string body)
        {
            var host = _config["SmtpSettings:Host"];
            var port = int.Parse(_config["SmtpSettings:Port"]);
            var email = _config["SmtpSettings:Email"];
            var password = _config["SmtpSettings:Password"];

            using var smtp = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(email, password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(email, "Education Trade"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await smtp.SendMailAsync(message); 
        }
    }
}
