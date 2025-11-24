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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// DB
var isDev = builder.Environment.IsDevelopment();
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
 opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
 // Do not enable sensitive data logging or verbose SQL console output by default to avoid slowdowns
 // and privacy warnings. If you need SQL logs temporarily enable them via configuration.
 if (isDev)
 {
 // Optionally enable detailed errors (does not log SQL)
 opt.EnableDetailedErrors();
 }
});

// JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<SistemaEscolar.Models.Auth.JwtSettings>() ?? new SistemaEscolar.Models.Auth.JwtSettings
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

// Lockout settings (Security:Lockout) - bind config if present
var lockoutSettings = builder.Configuration.GetSection("Security:Lockout").Get<SistemaEscolar.Models.Security.LockoutSettings>() ?? new SistemaEscolar.Models.Security.LockoutSettings();
builder.Services.AddSingleton(lockoutSettings);

// DI services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRolService, RolService>();
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
 // secure policy default (SameAsRequest) - can be tightened in production
 opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
 "Usuarios.Gestion","Cursos.Ver","Cursos.Crear","Cursos.Editar","Cursos.Eliminar","Cursos.AsignarDocente","Historial.Ver","Bitacora.Ver","Seguridad.Gestion","Evaluaciones.Crear"
 };
 foreach (var p in permisos)
 // allow access if user has the permiso claim OR is in Administrator role
 opts.AddPolicy(p, pol => pol.RequireAssertion(ctx => ctx.User.IsInRole("Administrador") || ctx.User.HasClaim("permiso", p)));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// On startup: do not reset database. Only ensure it exists and apply DB patcher if available.
using (var scope = app.Services.CreateScope())
{
 var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
 try
 {
 // In development, drop and recreate database to regenerate
 if (isDev)
 {
 ctx.Database.EnsureDeleted();
 }
 ctx.Database.EnsureCreated();
 DbPatcher.Apply(ctx); // aplica cambios no destructivos
 DataSeeder.Seed(ctx); // seed users if not exist
 }
 catch (Exception ex)
 {
 var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
 logger.LogError(ex, "Error al inicializar la base de datos");
 }
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
