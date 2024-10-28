using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers;
public class Security
{
    public static string Encrypt(string password)
    {
        var provider = SHA512.Create();
        string salt = "ThisSaltIsUncr@ble@-2300&^%$#@!";
        byte[] bytes = provider.ComputeHash(Encoding.UTF32.GetBytes(salt + password));
        var pass = BitConverter.ToString(bytes).Replace("-", "").ToLower();
        return pass;
    }

    public static string GenerateRandomPassword(int length = 8)
    {
        var random = new Random();
        const string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        StringBuilder result = new StringBuilder();

        while (length-- > 0)
        {
            result.Append(validCharacters[random.Next(validCharacters.Length)]);
        }
        return result.ToString();
    }
}
