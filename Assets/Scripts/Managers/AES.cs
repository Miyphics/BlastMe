using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class AES
{
    public static int KeyLength = 128;
    private const string SaltKey = "IsEJOuFSdELeM84D";
    private const string IVKey = "z5Q3*1_Zwd):eNCQ"; // TODO: Generate random VI each encryption and store it with encrypted value

    public static string Encrypt(byte[] value, string password)
    {
        byte[] keyBytes = GetSaltKeyBytes(password);
        RijndaelManaged symmetricKey = new() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
        ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.UTF8.GetBytes(IVKey));

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(value, 0, value.Length);
        cryptoStream.FlushFinalBlock();
        cryptoStream.Close();
        memoryStream.Close();

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static string Encrypt(string value, string password)
    {
        return Encrypt(Encoding.UTF8.GetBytes(value), password);
    }

    public static string Decrypt(string value, string password)
    {
        byte[] cipherTextBytes = Convert.FromBase64String(value);
        byte[] keyBytes = GetSaltKeyBytes(password);
        RijndaelManaged symmetricKey = new() { Mode = CipherMode.CBC, Padding = PaddingMode.None };
        ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.UTF8.GetBytes(IVKey));

        using MemoryStream memoryStream = new(cipherTextBytes);
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        byte[] plainTextBytes = new byte[cipherTextBytes.Length];
        int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

        memoryStream.Close();
        cryptoStream.Close();

        return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
    }

    private static byte[] GetSaltKeyBytes(string password)
    {
        return new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(SaltKey)).GetBytes(KeyLength / 8);
    }
}
