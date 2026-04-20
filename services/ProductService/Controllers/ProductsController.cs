using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ProductService.Data;
using ProductService.Models;
using System.Text.Json;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly IDistributedCache _cache;

        public ProductsController(ProductDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        private string GetLocalizedMessage(string ukMessage, string enMessage)
        {
            var language = Request.Headers["Accept-Language"].ToString().ToLower();
            return language.Contains("uk") ? ukMessage : enMessage;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            string cacheKey = "products_list";
            
            var cachedProducts = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedProducts))
            {
                var products = JsonSerializer.Deserialize<List<Product>>(cachedProducts);
                return Ok(products);
            }

            var dbProducts = await _context.Products.ToListAsync();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            };
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dbProducts), cacheOptions);

            return Ok(dbProducts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            string cacheKey = $"product_{id}";
            var cachedProduct = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedProduct))
            {
                return Ok(JsonSerializer.Deserialize<Product>(cachedProduct));
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                var errorMessage = GetLocalizedMessage("Товар не знайдено", "Product not found");
                return NotFound(new { message = errorMessage });
            }

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            });

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync("products_list");

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = GetLocalizedMessage("Товар не знайдено", "Product not found") });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync("products_list");
            await _cache.RemoveAsync($"product_{id}");

            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Category = request.Category;
            product.ImageUrl = request.ImageUrl;

            await _context.SaveChangesAsync();

            
            await _cache.RemoveAsync("products_all");
            await _cache.RemoveAsync($"product_{id}");

            return Ok(new { message = "Товар оновлено" });
        }
    }
}