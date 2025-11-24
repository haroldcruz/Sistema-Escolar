using System;
using System.Security.Cryptography;
using System.Text;

public class PasswordHasher
{
    public (byte[] hash, byte[] salt) HashPassword(string password)
    {
        byte[] salt = Encoding.UTF8.GetBytes("salt123"); // Fixed salt for testing
        using var hmac = new HMACSHA512(salt);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return (hash, salt);
    }
}

class Program
{
    static void Main()
    {
        var hasher = new PasswordHasher();
        var (hash, salt) = hasher.HashPassword("Admin123!");
        Console.WriteLine("Hash: " + BitConverter.ToString(hash).Replace("-", "").ToLower());
        Console.WriteLine("Salt: " + BitConverter.ToString(salt).Replace("-", "").ToLower());
    }
}