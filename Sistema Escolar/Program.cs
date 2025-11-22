using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaEscolar.Data;
using SistemaEscolar.Helpers;
using SistemaEscolar.Interfaces.Auth;
using SistemaEscolar.Services.Auth;
using SistemaEscolar.Interfaces.Usuarios;
using SistemaEscolar.Services.Usuarios;
using SistemaEscolar.Interfaces.Cursos;
using SistemaEscolar.Services.Cursos;
using SistemaEscolar.Interfaces.Historial;
using SistemaEscolar.Services.Historial;
using SistemaEscolar.Interfaces.Bitacora;
using SistemaEscolar.Services.Bitacora;
using SistemaEscolar.Middleware;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// Carga correcta de appsettings.json
// ========================================
builder.Configuration
 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
 .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
 .AddEnvironmentVariables();

// ========================================
// DB CONTEXT
// ========================================
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseSqlServer(conn)); // removido UseQuerySplittingBehavior

// ========================================
// JWT Settings (correcto: usa la sección "Jwt")
// ========================================
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// ========================================
// Dependency Injection (Servicios)
// ========================================
builder.Services.AddHttpContextAccessor(); // NECESARIO para inyección en AuthService, CursoService, UsuarioService
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IHistorialService, HistorialService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();

builder.Services.AddControllersWithViews(); // habilita vistas + API
builder.Services.AddRazorPages();

// ========================================
// AUTHENTICATION JWT
// ========================================
var jwtSection = builder.Configuration.GetSection("Jwt");

// Debug para verificar lectura de configuración
Console.WriteLine(">>> JWT SECTION TEST");
Console.WriteLine("Issuer: " + builder.Configuration["Jwt:Issuer"]);
Console.WriteLine("Audience: " + builder.Configuration["Jwt:Audience"]);
Console.WriteLine("Key: " + builder.Configuration["Jwt:Key"]);

var issuer = jwtSection.GetValue<string>("Issuer") ?? "SistemaEscolar";
var audience = jwtSection.GetValue<string>("Audience") ?? "SistemaEscolarUsers";
var key = jwtSection.GetValue<string>("Key") ?? "DevFallbackKey_ChangeMe123!";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services.AddAuthentication(options =>
{
 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
 options.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidateAudience = true,
 ValidateLifetime = true,
 ValidateIssuerSigningKey = true,
 ValidIssuer = issuer,
 ValidAudience = audience,
 IssuerSigningKey = signingKey,
 ClockSkew = TimeSpan.FromMinutes(1) // reducir ventana para seguridad
 };
 options.Events = new JwtBearerEvents
 {
 OnMessageReceived = context =>
 {
 if (string.IsNullOrEmpty(context.Token))
 {
 var cookie = context.Request.Cookies["jwt"]; // token guardado tras login
 if (!string.IsNullOrEmpty(cookie))
 context.Token = cookie;
 }
 return Task.CompletedTask;
 },
 OnAuthenticationFailed = ctx =>
 {
 Console.WriteLine($"JWT auth failed: {ctx.Exception.Message}");
 return Task.CompletedTask;
 }
 };
});

// Definir policies basadas en claim "permiso"
builder.Services.AddAuthorization(options =>
{
 string[] permisos = new[]
 {
 "Usuarios.Gestion","Cursos.Ver","Cursos.Crear","Cursos.Editar","Cursos.Eliminar","Cursos.AsignarDocente","Historial.Ver","Bitacora.Ver","Seguridad.Gestion"
 };
 foreach (var p in permisos)
 options.AddPolicy(p, policy => policy.RequireClaim("permiso", p));
});

// ========================================
// APP PIPELINE
// ========================================
var app = builder.Build();

// Aplicar migraciones automáticamente (incluye tabla RefreshTokens)
using (var scope = app.Services.CreateScope())
{
 var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
 ctx.Database.Migrate();
 // Seed data
 DataSeeder.Seed(ctx);
}

// Manejo global de errores
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Jwt cookie helper antes de autenticación
app.UseMiddleware<JwtMiddleware>();
// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Middlewares personalizados
app.UseMiddleware<BitacoraMiddleware>();

// MVC + API
app.MapControllers();
app.MapRazorPages();

// RUTA POR DEFECTO PARA CONTROLADORES MVC
app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
