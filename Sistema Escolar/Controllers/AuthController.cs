using Microsoft.AspNetCore.Mvc;
using SistemaEscolar.Interfaces.Auth;
using SistemaEscolar.DTOs.Auth;

namespace SistemaEscolar.Controllers
{
 public class AuthController : Controller
 {
 private readonly IAuthService _auth;
 public AuthController(IAuthService auth){ _auth = auth; }

 [HttpGet]
 public IActionResult Login(){ return View(); }

 [HttpPost]
 public async Task<IActionResult> Login(LoginRequest model)
 {
 if(!ModelState.IsValid) return View(model);
 var resp = await _auth.LoginAsync(model);
 if(resp==null){ ModelState.AddModelError("","Credenciales inválidas"); return View(model); }
 Response.Cookies.Append("jwt", resp.Token, new CookieOptions{ HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
 return RedirectToAction("Index","Home");
 }

 [HttpGet]
 public IActionResult Logout()
 {
 Response.Cookies.Delete("jwt");
 return RedirectToAction("Login");
 }
 }
}