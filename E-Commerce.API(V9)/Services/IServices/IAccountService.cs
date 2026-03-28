using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API_V9_.Services.IServices
{
    public interface IAccountService
    {
        Task SendEmailAsync(EmailType emailType, string msg, ApplicationUser applicationUser);
    }
}
