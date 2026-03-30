using E_Commerce.API_V9_.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API_V9_.Areas.Admin
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]

    [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN} , {SD.ROLE_EMPLOYEE}")]
    public class CategoriesController : ControllerBase
    {
        private IRepository<Catgeory> _repository;
        public CategoriesController(IRepository<Catgeory> repository)
        {
            _repository = repository;
        }
        [HttpGet]
        public async Task<IActionResult> Get(string? name, int page = 1)
        {
            var categories = await _repository.GetAsync(tracking: false);
            //Add new filter
            if (name is not null)
                categories = categories.Where(e => e.Name.Contains(name)).ToList();

            // Pagination
            if (page < 1)
                page = 1;
            int pageSize = 5;
            int currentPage = page;
            double totalCount = Math.Ceiling(categories.Count() / (double)pageSize);
            categories = categories.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new CategoriesResponse
            {
                Categories = categories.AsEnumerable(),
                TotalPages = totalCount,
                CurrentPage = currentPage
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var category = await _repository.GetOneAsync(e => e.Id == id);
            if (category is null)
                return NotFound();
            return Ok(category);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Catgeory catgeory)
        {
            

            await _repository.CreateAsync(catgeory);
            await _repository.CommitAsync();
            return Ok(new SuccessResponse()
            { 
            Msg = "Category created successfully"
            });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
        public async Task<IActionResult> Update([FromRoute] int id, Catgeory catgeory)
        {

           var categoryInDb = await _repository.GetOneAsync(e=>e.Id == id);
            if (categoryInDb is null)
                return NotFound();
            categoryInDb.Name = catgeory.Name;
            categoryInDb.Description = catgeory.Description;
            categoryInDb.Status = catgeory.Status;
            await _repository.CommitAsync();
            return Ok(new SuccessResponse()
            {
                Msg = "Category updated successfully"
            });
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var category = await _repository.GetOneAsync(e => e.Id == id);
            if (category is null)
                return NotFound();
            _repository.Delete(category);
            await _repository.CommitAsync();
            return Ok(new SuccessResponse()
            {
                Msg = "Category deleted successfully"
            });
        }
    }
}

