using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaEscolar.Data;
using SistemaEscolar.Helpers;
using SistemaEscolar.Interfaces.Auth;
using SistemaEscolar.Interfaces.Bitacora;
using SistemaEscolar.Interfaces.Cursos;
using SistemaEscolar.Interfaces.Historial;
using SistemaEscolar.Interfaces.Usuarios;
using SistemaEscolar.Middleware;
using SistemaEscolar.Services.Auth;
using SistemaEscolar.Services.Bitacora;
using SistemaEscolar.Services.Cursos;
using SistemaEscolar.Services.Historial;
using SistemaEscolar.Services.Usuarios;
using System.Text;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
 opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings
{
 Secret = "DEV_SUPER_LONG_SECRET_KEY_CHANGE_ME__1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".PadRight(64,'X'),
 Issuer = "SistemaEscolar",
 Audience = "SistemaEscolar",
 ExpirationMinutes =60
};
if(string.IsNullOrWhiteSpace(jwtSettings.Secret) || Encoding.UTF8.GetBytes(jwtSettings.Secret).Length <32) //256 bits
{
 jwtSettings.Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
builder.Services.AddSingleton(jwtSettings);

// DI services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRolService, RolService>(); // REGISTRO FALTANTE
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IHistorialService, HistorialService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddHttpContextAccessor();

// Auth
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
// Cookie para vistas + JWT para APIs
builder.Services.AddAuthentication(options =>
{
 options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
 options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
{
 opt.LoginPath = "/Auth/Login";
 opt.LogoutPath = "/Auth/Logout";
 opt.AccessDeniedPath = "/Auth/AccessDenied"; // ruta existente
 opt.Cookie.Name = "app_auth";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{
 o.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidateAudience = true,
 ValidateLifetime = true,
 ValidateIssuerSigningKey = true,
 ValidIssuer = jwtSettings.Issuer,
 ValidAudience = jwtSettings.Audience,
 IssuerSigningKey = new SymmetricSecurityKey(key)
 };
 // Recuperar token desde cookie "jwt"
 o.Events = new JwtBearerEvents
 {
 OnMessageReceived = ctx =>
 {
 var token = ctx.Request.Cookies["jwt"]; // cookie creada al login
 if (!string.IsNullOrEmpty(token)) ctx.Token = token;
 return Task.CompletedTask;
 }
 };
});

// Authorization policies (permiso claim)
builder.Services.AddAuthorization(opts =>
{
 string[] permisos = new[] {
 "Usuarios.Gestion","Cursos.Ver","Cursos.Crear","Cursos.Editar","Cursos.Eliminar","Cursos.AsignarDocente","Historial.Ver","Bitacora.Ver","Seguridad.Gestion"
 };
 foreach (var p in permisos)
 opts.AddPolicy(p, pol => pol.RequireClaim("permiso", p));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed
using (var scope = app.Services.CreateScope())
{
 var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    ctx.Database.EnsureCreated();
    DataSeeder.Seed(ctx);
}

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();
app.UseMiddleware<BitacoraMiddleware>();

// Habilitar rutas por atributos en controladores API (ej: /api/cursos)
app.MapControllers();

app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
