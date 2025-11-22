using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Auth;
using SistemaEscolar.Interfaces.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaEscolar.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _auth;
        private readonly ApplicationDbContext _ctx;

        public AuthController(IAuthService auth, ApplicationDbContext ctx)
        {
            _auth = auth;
            _ctx = ctx;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");
            // Inicializar propiedades requeridas
            return View(new LoginRequest { Email = string.Empty, Password = string.Empty });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            if (!ModelState.IsValid) return View(req);

            var resp = await _auth.LoginAsync(req); // llamada correcta con DTO
            if (resp == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                return View(req);
            }

            // Claims base
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, resp.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, resp.NombreCompleto),
                new Claim(ClaimTypes.Email, resp.Email)
            };
            // Roles
            claims.AddRange(resp.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
            // Permisos derivados de roles
            var permisos = await _ctx.RolPermisos
                .Include(rp => rp.Permiso)
                .Where(rp => rp.Rol.UsuarioRoles.Any(ur => ur.UsuarioId == resp.UsuarioId))
                .Select(rp => rp.Permiso.Codigo)
                .Distinct()
                .ToListAsync();
            claims.AddRange(permisos.Select(p => new Claim("permiso", p)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Guardar JWT para APIs
            Response.Cookies.Append("jwt", resp.Token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                Path = "/"
            });

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (Request.Cookies.ContainsKey("jwt")) Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}