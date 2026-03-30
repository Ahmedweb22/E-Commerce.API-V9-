using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using E_Commerce.API_V9_.DTOs.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;


namespace E_Commerce.API_V9_.Areas.Identity
{
    [Microsoft.AspNetCore.Mvc.Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.IDENTITY_AREA)]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IAccountService _accountService;
        private readonly IRepository<ApplicationUserOTP> _otpRepository;
        public AccountsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IAccountService accountService, IRepository<ApplicationUserOTP> otpRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _accountService = accountService;
            _otpRepository = otpRepository;
        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Created($"{Request.Scheme}://{Request.Host}/Customer/Home/Index",
               new SuccessResponse()
               {
                   Msg = "\"Logged out successfully."
               });
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest model)
        {

            ApplicationUser applicationUser = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                FName = model.FName,
                LName = model.LName,
                Address = model.Address

            };
            var result = await _userManager.CreateAsync(applicationUser, model.Password);
            if (!result.Succeeded)
            {
                ModelStateDictionary keyValuePairs = new();
                foreach (var error in result.Errors)
                {
                    keyValuePairs.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(keyValuePairs);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = applicationUser.Id, token = token }, Request.Scheme);

            await _accountService.SendEmailAsync(EmailType.ConfirmEmail, $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>Click here to confirm your account</a>", applicationUser);

            await _userManager.AddToRoleAsync(applicationUser, SD.ROLE_CUSTOMER);
            
        
            return Ok(new SuccessResponse ()
            {
                Msg = "User created successfully" 
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest model)
        {

            var user = await _userManager.FindByEmailAsync(model.EmailOrUserName) ??
               await _userManager.FindByNameAsync(model.EmailOrUserName);
            ModelStateDictionary keyValuePairs = new();
            if (user == null)
            {
                keyValuePairs.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(keyValuePairs);
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

          
            if (!result.Succeeded)
            {
                if (result.IsNotAllowed)
                {
                    keyValuePairs.AddModelError("EmailOrUserName", "Confirm your email before logging in.");
                    return BadRequest(keyValuePairs);
                }
                if (result.IsLockedOut)
                {
                    keyValuePairs.AddModelError(string.Empty, "Your account is locked out. Please try again later.");
                    return BadRequest(keyValuePairs);
                }
                keyValuePairs.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(keyValuePairs);
            }
           
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim (JwtRegisteredClaimNames.Jti, DateTime.Now.ToString("dd-MM-yyyy")));
            
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QVeEgVkjRtK2RznXiRuLbCeJlDWp11MG57ktMvt7/dE="));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                  issuer: "https://localhost:7284",
                  audience: "https://localhost:7284",
                  claims: claims,
                  expires: DateTime.Now.AddMinutes(50),
                  signingCredentials: signingCredentials
                  );
            return Ok(new
            {
                Msg = $"Welcome back {user.UserName}!",
            token = new JwtSecurityTokenHandler().WriteToken(token),
            AccesstokenExpireIn = "33 min"

        });
        }
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest(new
                {
                    message = "UserId and token are required"
                });
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    message = "Email confirmed successfully. You can now log in."
                });
            }
            else
            {
                ModelStateDictionary keyValuePairs = new();
                foreach (var error in result.Errors)
                {
                    keyValuePairs.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(keyValuePairs);
            }
        }
        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest model)
        {

            var user = await _userManager.FindByEmailAsync(model.EmailOrUserName) ??
                       await _userManager.FindByNameAsync(model.EmailOrUserName);
            if (user is not null && !user.EmailConfirmed)
            {

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ResendConfirmationEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
                await _accountService.SendEmailAsync(EmailType.ResendConfirmationEmail, $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>Click here to confirm your account</a>", user);
            }
            return Ok(new SuccessResponse()
            {
                Msg = "If an account with that email or username exists and is not confirmed, a confirmation email has been resent."
            });
        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest model)
        {
        

            // Logic to resend email confirmation
            var user = await _userManager.FindByEmailAsync(model.EmailOrUserName) ??
                       await _userManager.FindByNameAsync(model.EmailOrUserName);
            var userOtpsCount = (await _otpRepository.GetAsync(e => user.Id == e.UserId && e.CreateAt >= DateTime.UtcNow.AddHours(-24))).Count();
            if (!user.EmailConfirmed)
            {
             return BadRequest(new ErrorResponce()
             {
                    ErrorMsg = "Please confirm your email before resetting your password."
                });
            }
            if (user is not null && userOtpsCount < 5)
            {
                string otp = new Random().Next(1000, 9999).ToString();
                string msg = $"<h1>Your OTP for password reset is: {otp}. Don't share it</h1>";
                await _accountService.SendEmailAsync(EmailType.ForgetPassword, msg, user);
                await _otpRepository.CreateAsync(new()
                {
                    UserId = user.Id,
                    OTP = otp,
                });
                await _otpRepository.CommitAsync();
                return Ok(new SuccessResponse()
                {
                    Msg = "Send OTP to your email Successfully."
                });
            }
            else if (userOtpsCount >= 5)
            {
                return BadRequest(new ErrorResponce()
                {
                    ErrorMsg = "You have exceeded the maximum number of OTP requests. Please try again later."
                });
            }
                return Ok(new SuccessResponse()
                {
                        Msg = "If an account with that email or username exists, an OTP has been sent to the registered email address."
                    });

        }
        [HttpPost("ValidateOTP")]
        public async Task<IActionResult> ValidateOTP(ValidateOTPRequest model)
        {

            var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
            ModelStateDictionary keyValuePairs = new ModelStateDictionary();
            if (user is null)
            {
                keyValuePairs.AddModelError(string.Empty, "Invalid OTP.");
                return BadRequest(keyValuePairs);
            }

            var otp = (await _otpRepository.GetAsync()).Where(e => e.UserId == user.Id && e.IsValid).OrderBy(e => e.Id).LastOrDefault();
            if (otp == null)
            {
                keyValuePairs.AddModelError(string.Empty, "Invalid OTP.");
                return BadRequest(keyValuePairs);
            }
            otp.IsUsed = true;
            return Ok(new SuccessResponse()
            {
                Msg = "OTP validated successfully. You can now reset your password."
            });
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
          
            var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
            ModelStateDictionary keyValuePairs = new ModelStateDictionary();

            if (user is null)
            {
                keyValuePairs.AddModelError(string.Empty, "User not found.");
                return BadRequest(keyValuePairs);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    keyValuePairs.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(keyValuePairs);
            }
            return Ok(new SuccessResponse()
            {
                Msg = "Your password has been reset successfully. You can now log in with your new password."
            });
        }
    }
}
