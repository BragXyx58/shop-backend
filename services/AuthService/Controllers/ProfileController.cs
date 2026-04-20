using AuthService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public ProfileController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetProfile(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound(new { message = "Користувача не знайдено" });

            return Ok(new { user.Username, user.Email, user.Phone, user.Address, user.Role });
        }

        [HttpPut("{email}")]
        public async Task<IActionResult> UpdateProfile(string email, [FromBody] UpdateProfileDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound();

            user.Phone = request.Phone;
            user.Address = request.Address;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Профіль оновлено" });
        }
    }

    public class UpdateProfileDto
    {
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}