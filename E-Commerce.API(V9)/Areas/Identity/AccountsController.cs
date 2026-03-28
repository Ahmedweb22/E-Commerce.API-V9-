using E_Commerce.API_V9_.DTOs.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;


namespace E_Commerce.API_V9_.Areas.Identity
{
    [Route("[area]/[controller]")]
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

            return Created($"{Request.Scheme}://{Request.Host}/Customer/Home/Index", new SuccessResponse(){ Msg = $"Welcome back {user.UserName}!" });
        }
    }
}
