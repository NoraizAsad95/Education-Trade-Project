using System;
using System.Collections.Generic;
using System.Linq;
using EducationTrade.Core.Helpers;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string userName, string verificationLink);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);

    }

}
