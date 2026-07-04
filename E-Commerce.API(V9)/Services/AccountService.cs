using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QVeEgVkjRtK2RznXiRuLbCeJlDWp11MG57ktMvt7/dE="));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                  issuer: "https://localhost:7284",
                  audience: "https://localhost:7284",
                  claims: claims,
                  expires: DateTime.Now.AddMinutes(50),
                  signingCredentials: signingCredentials
                  );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }

        public string GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            throw new NotImplementedException();
        }
    }
}
