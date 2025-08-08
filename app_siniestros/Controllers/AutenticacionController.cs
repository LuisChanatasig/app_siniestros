using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace app_siniestros.Controllers
{
    [AllowAnonymous]
    public class AutenticacionController : Controller
    {
        private readonly ILogger<AutenticacionController> _logger;

        public AutenticacionController(ILogger<AutenticacionController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            [FromForm] string username,
            [FromForm] string password,
            [FromForm] bool rememberMe = false,
            [FromForm] string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Aquí tu propia validación
            if (username == "admin" && password == "1234")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    // new Claim(ClaimTypes.Role, "Admin")
                };
                var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(id),
                    new AuthenticationProperties { IsPersistent = rememberMe });

                _logger.LogInformation("Usuario {User} inició sesión.", username);
                return LocalRedirect(returnUrl ?? "~/");
            }

            ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos.");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Usuario {User} cerró sesión.", userName);
            return RedirectToAction(nameof(Login));
        }
    }
}
