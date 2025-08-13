using AutoMapper;
using ElectronicsStore.Models;
using ElectronicsStore.Models.DTO;
using ElectronicsStore.RepositoryAndUnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ElectronicsStore.Controllers
{
    [Authorize(Roles ="admin")]
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class ProductController : Controller
    {
        private readonly IRepository<Product> _repository;
        private readonly IWebHostEnvironment webHost;
        private readonly IMapper mapper;
        public ProductController(IRepository<Product> repository, IWebHostEnvironment webHost,IMapper mapperu)
        {
            _repository = repository;
            this.webHost = webHost;
            this.mapper = mapper;
        }
        public async Task<IActionResult> Index(int pageIndex, int pageSize = 5)
        {
            var (products, totalPages) = await _repository.GetPage(pageSize, pageIndex);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageIndex;

            return View(products);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductDTO productDto)
        {
            if(productDto.ImageFile !=null)
            {
                if(productDto.ImageFile.Length > 1024*1024*5)
                {
                    ModelState.AddModelError("ImageFile", "Image size cannot be greater than 5 MB.");
                }
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
                if (!allowedExtensions.Contains(Path.GetExtension(productDto.ImageFile.FileName).ToLower()))
                    ModelState.AddModelError("ImageFile", "Only .png, .jpg, and .jpeg images are allowed.");

            }
            if(!ModelState.IsValid)
            {
                return View(productDto);
            }
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff")+Path.GetExtension(productDto.ImageFile.FileName);

            var imageDirectory = Path.Combine(webHost.WebRootPath, "products");
            if(!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }
            var filePath = Path.Combine(imageDirectory, fileName);
            using (var stream = new FileStream(filePath,FileMode.Create))
            {
                productDto.ImageFile.CopyTo(stream);
            }
            var product = new Product
            {
                Name = productDto.Name,
                Description=productDto.Description,
                Brand=productDto.Brand,
                Category=productDto.Category,
                Price=productDto.Price,
                ImageFile = fileName,
                CreatedAt = DateTime.Now
            };
            await _repository.AddAsync(product);
            await _repository.SaveChangeAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var productDTO = new ProductDTO
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
                // ImageFile stays null — handled via ViewData["ImageFile"]
            };

            ViewData["ProductId"] = id;
            ViewData["ImageFile"] = product.ImageFile ?? "";
            ViewData["CreatedAt"] = product.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss");

            return View(productDTO);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductDTO productDTO)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Keep same ViewData keys as GET so the view doesn't break
                ViewData["ProductId"] = id;
                ViewData["ImageFile"] = product.ImageFile ?? "";
                ViewData["CreatedAt"] = product.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss");
                return View(productDTO);
            }

            // Save new file only if one was uploaded
            if (productDTO.ImageFile != null && productDTO.ImageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/products");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var fileName = Path.GetFileName(productDTO.ImageFile.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productDTO.ImageFile.CopyToAsync(stream);
                }

                product.ImageFile = fileName;
            }

            // Update other fields
            product.Name = productDTO.Name;
            product.Brand = productDTO.Brand;
            product.Category = productDTO.Category;
            product.Price = productDTO.Price;
            product.Description = productDTO.Description;

            await _repository.UpdateAsync(product);
            await _repository.SaveChangeAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if(product == null)
            {
                return NotFound();
            }
            string imageName = Path.Combine(webHost.WebRootPath,"products",product.ImageFile);
            System.IO.File.Delete(imageName);
            await _repository.DeleteAsync(product.Id);
            await _repository.SaveChangeAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}