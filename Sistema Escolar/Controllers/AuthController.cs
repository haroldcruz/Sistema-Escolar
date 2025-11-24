using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SistemaEscolar.Data;
using SistemaEscolar.DTOs.Auth;
using SistemaEscolar.Interfaces.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace SistemaEscolar.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _auth;
        private readonly ApplicationDbContext _ctx;
        private readonly IConfiguration _cfg;

        public AuthController(IAuthService auth, ApplicationDbContext ctx, IConfiguration cfg)
        {
            _auth = auth;
            _ctx = ctx;
            _cfg = cfg;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] SistemaEscolar.DTOs.Auth.LoginRequest req)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return BadRequest(new { message = "Entrada inválida" });
                return View(req);
            }

            var resp = await _auth.LoginAsync(req); // llamada correcta con DTO
            if (resp == null)
            {
                // Incrementar intentos fallidos (simple, en memoria/configurable) - puede reemplazarse por DB/cache
                // Leer max intentos desde config
                int maxAttempts = _cfg.GetValue<int>("Auth:MaxFailedAttempts",5);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos");
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

            // Use explicit authentication properties so cookie is created and persisted
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            // Guardar JWT para APIs - use SameSite=Lax for dev to allow cross-port requests
            Response.Cookies.Append("jwt", resp.Token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Path = "/"
            });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok(new { message = "Autenticado" });

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