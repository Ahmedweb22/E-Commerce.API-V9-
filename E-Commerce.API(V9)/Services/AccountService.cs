using System.Security.Policy;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API_V9_.Services
{
    public enum EmailType
    {
        ConfirmEmail,
        ResendConfirmationEmail,
        ForgetPassword
    }
    public class AccountService : IAccountService
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(IEmailSender emailSender, UserManager<ApplicationUser> userManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
        }
        public async Task SendEmailAsync(EmailType emailType, string msg, ApplicationUser applicationUser)
        {


            if (emailType == EmailType.ConfirmEmail)
            {
                await _emailSender.SendEmailAsync(applicationUser.Email!, "Confirm your email", msg);
            }
            else if (emailType == EmailType.ResendConfirmationEmail)
            {
                await _emailSender.SendEmailAsync(applicationUser.Email!, "Resend Confirmation Email", msg);
            }
            else if (emailType == EmailType.ForgetPassword)
            {
                await _emailSender.SendEmailAsync(applicationUser.Email!, "Forget Password", msg);
            }
        }
    }
}
