using System;
using System.Text;

public static class Encryptor
{
    public static string Encode(string text)
    {
        return AES.Encrypt(text, "BlastME2");
    }

    public static string Decode(string text)
    {
        return AES.Decrypt(text, "BlastME2");
    }
}
