using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserDbContext _context;
        public ProfileController(UserDbContext context) => _context = context;

        [HttpGet("{email}")]
        public async Task<IActionResult> GetProfile(string email)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Email == email);
            return Ok(profile ?? new UserProfile { Email = email }); 
        }

        [HttpPut("{email}")]
        public async Task<IActionResult> UpdateProfile(string email, [FromBody] UserProfile request)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Email == email);
            if (profile == null)
            {
                request.Email = email;
                _context.Profiles.Add(request);
            }
            else
            {
                profile.Phone = request.Phone;
                profile.Address = request.Address;
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Профіль оновлено" });
        }
    }
}