using System.Security.Cryptography;
using System.Text;
using Inedo.ProGet;

namespace PgUtil.Config;

internal sealed record class PgUtilSource(string Name, string Url, string? DefaultFeed = null, string? Username = null, string? Password = null, string? Token = null, byte[]? EncryptedPassword = null, byte[]? EncryptedToken = null)
{
    // This is not intended as a true security measure, only a means to prevent accidental casual disclosure
    private static ReadOnlySpan<byte> ObfuscationKey => [207, 133, 227, 29, 231, 23, 59, 35, 79, 120, 240, 230, 231, 15, 243, 173];

    public ProGetClient GetProGetClient()
    {
        if (!string.IsNullOrEmpty(this.Token))
            return new(this.Url, this.Token);
        else if (!string.IsNullOrEmpty(this.Username))
            return new(this.Url, this.Username, this.Password ?? string.Empty);
        else 
            return new ProGetClient(this.Url);
    }
    public PgUtilSource Obfuscate()
    {
        var inst = this;
        if (!string.IsNullOrEmpty(inst.Password))
            inst = inst with { EncryptedPassword = Obfuscate(inst.Password), Password = null };

        if (!string.IsNullOrEmpty(inst.Token))
            inst = inst with { EncryptedToken = Obfuscate(inst.Token), Token = null };

        return inst;
    }
    public PgUtilSource Unobfuscate()
    {
        var inst = this;
        if (inst.EncryptedPassword is not null)
            inst = inst with { Password = Unobfuscate(inst.EncryptedPassword) };

        if (inst.EncryptedToken is not null)
            inst = inst with { Token = Unobfuscate(inst.EncryptedToken) };

        return inst;
    }

    private static byte[] Obfuscate(string value)
    {
        Span<byte> iv = stackalloc byte[16];
        RandomNumberGenerator.Fill(iv);
        using var aes = Aes.Create();
        aes.Key = ObfuscationKey.ToArray();
        var data = aes.EncryptCbc(new UTF8Encoding(false).GetBytes(value), iv);
        var result = new byte[iv.Length + data.Length];
        iv.CopyTo(result);
        data.AsSpan().CopyTo(result.AsSpan(iv.Length));
        return result;
    }
    private static string Unobfuscate(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = ObfuscationKey.ToArray();
        aes.IV = data[..16];
        var result = aes.DecryptCbc(data.AsSpan(16), data.AsSpan(0, 16));
        return Encoding.UTF8.GetString(result);
    }
}
