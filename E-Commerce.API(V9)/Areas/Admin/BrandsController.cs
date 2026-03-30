using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API_V9_.Areas.Admin
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN} , {SD.ROLE_EMPLOYEE}")]
    public class BrandsController : ControllerBase
    {
        private IRepository<Brand> _brandRepository;
        public BrandsController(IRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Get(string? name, int page = 1)
        {

            var brands = await _brandRepository.GetAsync(tracking: false);
            //Add new filter
            if (name is not null)
                brands = brands.Where(e => e.Name.Contains(name)).ToList();

            // Pagination
            if (page < 1)
                page = 1;
            int pageSize = 5;
            int currentPage = page;
            double totalCount = Math.Ceiling(brands.Count() / (double)pageSize);
            brands = brands.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new BrandsResponse
            {
                Brands = brands.AsEnumerable(),
                CurrentPage = currentPage,
                TotalPages = totalCount
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id);
            if (brand is null)
                return NotFound();
            return Ok(brand);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] BrandCreateRequest model)
        {
            var brand = model.Adapt<Brand>();
            if (model.Logo is not null && model.Logo.Length > 0)
            {
                var newFileName = Guid.NewGuid().ToString() + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(model.Logo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\brand_logos", newFileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    model.Logo.CopyTo(stream);
                }
                brand.Logo = newFileName;
            }

            await _brandRepository.CreateAsync(brand);
            await _brandRepository.CommitAsync();
            return Created();
        }
        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
        public async Task<IActionResult> Update([FromRoute] int id ,[FromForm] BrandUpdateRequest model)
        {
            Brand? existingBrand = await _brandRepository.GetOneAsync(e => e.Id == id, tracking: false);
            if (existingBrand is null)
                return NotFound();
            if (model.Logo is not null && model.Logo.Length > 0)
            {
                var newFileName = Guid.NewGuid().ToString() + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(model.Logo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\brand_logos", newFileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    model.Logo.CopyTo(stream);
                }
                // Optionally delete the old logo file
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\brand_logos", existingBrand.Logo);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
                existingBrand.Logo = newFileName;
            }
                          existingBrand.Name = model.Name;
                existingBrand.Status = model.Status;
            await _brandRepository.CommitAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id);
            if (brand is null)
                return NotFound();
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\brand_logos", brand.Logo);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
            _brandRepository.Delete(brand);
            await _brandRepository.CommitAsync();
            return NoContent();
        }

    }
}
