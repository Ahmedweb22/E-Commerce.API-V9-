using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API_V9_.Areas.Admin
{
    [Route("[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN} , {SD.ROLE_EMPLOYEE}")]
    public class ProductsController : ControllerBase
    {
        private IRepository<Product> _productRepository;
        private IProductSubImgRepository _productSubImgRepository;
        private IRepository<Catgeory> _categoryRepository;
        private IRepository<Brand> _brandRepository;
        public ProductsController(IRepository<Product> productRepository, IProductSubImgRepository productSubImgRepository, IRepository<Catgeory> categoryRepository, IRepository<Brand> brandRepository)
        {
            _productRepository = productRepository;
            _productSubImgRepository = productSubImgRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Get(ProductFilterResponse filter, int page = 1)
        {
            var products = await _productRepository.GetAsync(includes: [p => p.Catgeory, p => p.Brand], tracking: false);


            //Add new filter
            ProductFilterResponse filterVMResponse = new();

            var categories = await _categoryRepository.GetAsync(tracking: false);
            var brands = await _brandRepository.GetAsync(tracking: false);
            if (filter.Name is not null)
            {
                products = products.Where(e => e.Name.Contains(filter.Name)).ToList();
                filterVMResponse.Name = filter.Name;
            }
            if (filter.MinPrice is not null)
            {
                products = products.Where(e => e.Price >= filter.MinPrice).ToList();
                filterVMResponse.MinPrice = filter.MinPrice;
            }
            if (filter.MaxPrice is not null)
            {
                products = products.Where(e => e.Price <= filter.MaxPrice).ToList();
                filterVMResponse.MaxPrice = filter.MaxPrice;
            }
            if (filter.CategoryId is not null)
            {
                products = products.Where(e => e.CatgeoryId == filter.CategoryId).ToList();
                filterVMResponse.CategoryId = filter.CategoryId;
            }
            if (filter.BrandId is not null)
            {
                products = products.Where(e => e.BrandId == filter.BrandId).ToList();
                filterVMResponse.BrandId = filter.BrandId;
            }
            if (filter.LessQuantity)
            {
                products = products.Where(e => e.Quantity < 50).ToList();
                filterVMResponse.LessQuantity = filter.LessQuantity;
            }
            // Pagination
            if (page < 1)
                page = 1;
            int pageSize = 5;
            int currentPage = page;
            double totalCount = Math.Ceiling(products.Count() / (double)pageSize);
            products = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new ProductsResponse
            {
                Products = products.AsEnumerable(),
                Categories = categories.AsEnumerable(),
                Brands = brands.AsEnumerable(),
                TotalPages = totalCount,
                CurrentPage = currentPage,
                Filter = filterVMResponse
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
        {
            var product = await _productRepository.GetOneAsync(e => e.Id == id);
            if (product is null)
                return NotFound();
            var productSubImgs = await _productSubImgRepository.GetAsync(p => p.ProductId == id);
            var categories = await _categoryRepository.GetAsync(tracking: false);
            var brands = await _brandRepository.GetAsync(tracking: false);
            return Ok(new ProductGetOneResponse
            {
                Product = product,
                SubImg = productSubImgs,
                Categories = categories,
                Brands = brands
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile mainImg, List<IFormFile> subImgs)
        {
            if (mainImg is not null && mainImg.Length > 0)
            {
                var newFileName = Guid.NewGuid().ToString() + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(mainImg.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\product_images", newFileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    mainImg.CopyTo(stream);
                }
                product.MainImg = newFileName;
            }

            await _productRepository.CreateAsync(product);
            await _productRepository.CommitAsync();
            if (subImgs.Any())
            {
                foreach (var img in subImgs)
                {
                    if (img is not null && img.Length > 0)
                    {
                        var newFileName = Guid.NewGuid().ToString() + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(img.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\product_images\\sub_images", newFileName);
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            img.CopyTo(stream);
                        }

                        await _productSubImgRepository.CreateAsync(new()
                        {
                            ProductId = product.Id,
                            SubImg = newFileName
                        });
                    }
                }
                await _productSubImgRepository.CommitAsync();
            }
            return Ok(new SuccessResponse()
            {
                Msg = "Product created successfully"
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
        public async Task<IActionResult> Edit(Product product, IFormFile? mainImg, List<IFormFile>? subImgs)
        {
            var PtoductInDB = await _productRepository.GetOneAsync(p => p.Id == product.Id, tracking: false);
            if (PtoductInDB is null)
                return NotFound();
            // 1.Update main image if exist
            if (mainImg is not null && mainImg.Length > 0)
            {
                //Step1: Create NewImg in wwwroot
                var newFileName = Guid.NewGuid().ToString() + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(mainImg.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\product_images", newFileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    mainImg.CopyTo(stream);
                }
                product.MainImg = newFileName;
                //Step2: Delete old image from wwwroot
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\product_images", PtoductInDB.MainImg);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
                //step3: Update Img in DB
                product.MainImg = newFileName;
            }
            else
            {
                product.MainImg = PtoductInDB.MainImg;
            }
            //2.Update Product Info
            _productRepository.Update(product);
            await _productRepository.CommitAsync();

            //3. Update Sub Images if exist
            if (subImgs.Any())
            {
                var oldSubImgs = await _productSubImgRepository.GetAsync(p => p.ProductId == product.Id);
                //Step1: Create New Imgs in wwwroot & Save to DB
                foreach (var img in subImgs)
                {
                    var newFileName = Guid.NewGuid().ToString() + DateTime.UtcNow.ToString("yyyy-MM-dd") + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\product_images\\sub_images", newFileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }
                    //Step2: Save to DB
                    await _productSubImgRepository.CreateAsync(new()
                    {
                        ProductId = product.Id,
                        SubImg = newFileName
                    });
                }
                //step3: Delete old Imgs from wwwroot
                foreach (var oldImg in oldSubImgs)
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\product_images\\sub_images", oldImg.SubImg);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                //step4: Delete old Imgs from DB
                _productSubImgRepository.DeleteRange(oldSubImgs);
                await _productSubImgRepository.CommitAsync();
            }
            return Ok(new SuccessResponse()
            {
                Msg = "Product updated successfully"
            });
        }
        [HttpPatch("{id}")]
        [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
        public async Task<IActionResult> DeleteImg([FromRoute] int id, [FromQuery] int productImgId)
        {
            var subImg = await _productSubImgRepository.GetOneAsync(p => p.Id == productImgId);
            if (subImg is null)
                return NotFound();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\product_images\\sub_images", subImg.SubImg);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            _productSubImgRepository.Delete(subImg);
            await _productSubImgRepository.CommitAsync();
      
            return Ok();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.ROLE_ADMIN} , {SD.ROLE_SUPER_ADMIN}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetOneAsync(p => p.Id == id);
            if (product is null)
                return NotFound();
            //Step1: Delete main image from wwwroot
            var mainImgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\product_images", product.MainImg);
            if (System.IO.File.Exists(mainImgPath))
            {
                System.IO.File.Delete(mainImgPath);
            }
            //Step2: Delete sub images from wwwroot
            var subImgs = await _productSubImgRepository.GetAsync(p => p.ProductId == id);
            foreach (var subImg in subImgs)
            {
                var subImgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img\\product_images\\sub_images", subImg.SubImg);
                if (System.IO.File.Exists(subImgPath))
                {
                    System.IO.File.Delete(subImgPath);
                }
            }
            //Step3: Delete sub images from DB
            _productSubImgRepository.DeleteRange(subImgs);
            //Step4: Delete product from DB
            _productRepository.Delete(product);
            await _productRepository.CommitAsync();
            return NoContent();
        }
    }
}
