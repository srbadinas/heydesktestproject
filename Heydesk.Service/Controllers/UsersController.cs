using Heydesk.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Heydesk.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly HeydeskContext _context;

        public UsersController(HeydeskContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("EnterEmail")]
        public async Task<IActionResult> EnterEmail([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { Message = "Enter your email." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User()
                {
                    Email = email
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }

            return Ok(user);

        }
    }
}
