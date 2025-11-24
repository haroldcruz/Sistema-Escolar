using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

// Simple console tool to reset admin password using same HMACSHA512 logic as the app
Console.WriteLine("SetAdminPassword tool");

// Support a generation-only mode: --gen <password>
if (args.Length ==2 && args[0] == "--gen")
{
 var genPass = args[1];
 using var hmacG = new HMACSHA512();
 var saltG = hmacG.Key;
 var hashG = hmacG.ComputeHash(Encoding.UTF8.GetBytes(genPass));
 Console.WriteLine("HASH=" + BitConverter.ToString(hashG).Replace("-", ""));
 Console.WriteLine("SALT=" + BitConverter.ToString(saltG).Replace("-", ""));
 Environment.Exit(0);
}

if (args.Length <3)
{
 Console.WriteLine("Usage: SetAdminPassword <connectionString> <email> <newPassword>");
 Console.WriteLine("Or: SetAdminPassword --gen <newPassword> (to print hash/salt)");
 Environment.Exit(1);
}
var connStr = args[0];
var email = args[1];
var newPass = args[2];

// Create salt and hash using HMACSHA512 (same approach as app's PasswordHasher)
byte[] saltBytes;
byte[] hashBytes;
using (var hmac = new HMACSHA512())
{
 saltBytes = hmac.Key;
 hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(newPass));
}

try
{
 using var conn = new SqlConnection(connStr);
 conn.Open();
 using var tran = conn.BeginTransaction();
 using var cmd = conn.CreateCommand();
 cmd.Transaction = tran;
 cmd.CommandText = @"UPDATE Usuarios SET PasswordHash = @hash, PasswordSalt = @salt, FailedLoginAttempts =0, LockoutEnd = NULL WHERE Email = @email";
 var pHash = new SqlParameter("@hash", SqlDbType.VarBinary, -1) { Value = hashBytes };
 var pSalt = new SqlParameter("@salt", SqlDbType.VarBinary, -1) { Value = saltBytes };
 cmd.Parameters.Add(pHash);
 cmd.Parameters.Add(pSalt);
 cmd.Parameters.Add(new SqlParameter("@email", SqlDbType.NVarChar,256) { Value = email });
 var rows = cmd.ExecuteNonQuery();
 tran.Commit();
 Console.WriteLine($"Rows updated: {rows}");
 Environment.Exit(rows >0 ?0 :2);
}
catch (Exception ex)
{
 Console.WriteLine("Error: " + ex.Message);
 Environment.Exit(3);
}