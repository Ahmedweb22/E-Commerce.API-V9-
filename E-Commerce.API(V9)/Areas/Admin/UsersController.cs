using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.API_V9_.Areas.Admin
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [HttpGet]
        public async Task<IActionResult> Get(string? name, int page = 1)
        {
            var users = _userManager.Users.AsNoTracking();

            if (name is not null)
                users = users.Where(e => e.UserName.Contains(name));

            if (page < 1)
                page = 1;
            int pageSize = 10;
            int currentPage = page;
            double totalCount = Math.Ceiling(users.Count() / (double)pageSize);
            users = users.Skip((page - 1) * pageSize).Take(pageSize);
            var usersList = await users.ToListAsync();
            var model = new List<UserWithRoleResponse>();
            foreach (var user in usersList)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserWithRoleResponse
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    RoleName = roles.FirstOrDefault()
                });
            }




            return Ok(new UsersResponse
            {
                Users = model,
                TotalPages = totalCount,
                CurrentPage = currentPage
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = _roleManager.Roles.AsNoTracking().AsQueryable();
            var userRoles = await _userManager.GetRolesAsync(user);
            return Ok(new GetOneUserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RoleName = userRoles.FirstOrDefault(),
                Roles = roles.AsEnumerable()
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequest model)
        {
           
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            ModelStateDictionary keyValuePairs = new();
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    keyValuePairs.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(keyValuePairs);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, model.RoleName);
              
                return Ok(new SuccessResponse()
                { 
                Msg = "Save Successful"
                });
            }


          
        }
       
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id , UpdateUserRequest model)
        {
      
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }
            user.UserName = model.UserName;
            user.Email = model.Email;
            var result = await _userManager.UpdateAsync(user);
            ModelStateDictionary keyValuePairs = new();
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    keyValuePairs.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(keyValuePairs);
            }
            else
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.AddToRoleAsync(user, model.RoleName);
                return Ok(new SuccessResponse()
                {
                    Msg = "Update Successful"
                });
            }
         

        }
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponse()
                {
                    ErrorMsg = "Delete Failed"
                });
            }
            else
            {
                return Ok(new SuccessResponse()
                {
                    Msg = "Delete Successful"
                });
            }
        }
    
}
}
