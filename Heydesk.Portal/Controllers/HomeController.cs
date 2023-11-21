using Heydesk.Commons.Helpers;
using Heydesk.Entities.Models;
using Heydesk.Portal.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Heydesk.Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly HeydeskContext _context;

        public HomeController(HeydeskContext context)
        {
            _context = context;
        }

        public IActionResult Index(HomeViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Chat");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EnterEmail(HomeViewModel model) 
        {
            if (string.IsNullOrWhiteSpace(model.User?.Email)) {
                model.Message = "Please enter your email.";
                return View(model);
            }

            try {
                var response = await ApiHelpers.PostRequest("/Users/EnterEmail", model.User.Email);

                if (!response.IsSuccessStatusCode) {
                    model.Message = "Bad request";
                    return View(model);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                model.User = JsonConvert.DeserializeObject<User>(responseContent);
            }
            catch (Exception ex)
            {
                model.Message = ex.Message;
                return View(model);
            }

            var claims = new List<Claim> {
                new Claim("UserId", model.User?.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Cookie");
            var principal = new ClaimsPrincipal(identity);

            var authenticationProperties = new AuthenticationProperties
            {
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(principal, authenticationProperties);

            return RedirectToAction("Index", "Chat");
        }
    }
}
