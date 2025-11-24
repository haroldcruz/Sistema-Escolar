using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using SistemaEscolar.Data;
using SistemaEscolar.Helpers;

// Simple tool to reset a user's password using the app's PasswordHasher and DbContext
var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".."); // adjust to solution root
var configPath = Path.Combine(basePath, "Sistema Escolar", "appsettings.json");
var cfg = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile(configPath).Build();
var conn = cfg.GetConnectionString("DefaultConnection");
if(string.IsNullOrEmpty(conn)) { Console.WriteLine("Connection string not found in appsettings.json"); return; }

var opts = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(conn).Options;
using var ctx = new ApplicationDbContext(opts);

Console.Write("Email to reset: ");
var email = Console.ReadLine() ?? string.Empty;
Console.Write("New password: ");
var newPass = Console.ReadLine() ?? string.Empty;

var user = ctx.Usuarios.FirstOrDefault(u => u.Email == email);
if(user == null){ Console.WriteLine("Usuario no encontrado"); return; }

PasswordHasher.CreatePasswordHash(newPass, out var hash, out var salt);
user.PasswordHash = hash;
user.PasswordSalt = salt;
user.FailedLoginAttempts =0;
user.LockoutEnd = null;
ctx.SaveChanges();
Console.WriteLine($"Password actualizado para {email}");
