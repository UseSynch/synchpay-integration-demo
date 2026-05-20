using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SynchPayIntegrationDemo.Services;

public sealed class PaymentStatusDecryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public string DecryptToJson(string encryptedData, string encryptionKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedData))
        {
            throw new InvalidOperationException("Payment status data was not provided.");
        }

        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            throw new InvalidOperationException("Status encryption key is required.");
        }

        var encryptedPayload = DecodePayload(encryptedData);

        if (encryptedPayload.Length <= NonceSize + TagSize)
        {
            throw new InvalidOperationException("Payment status data is not a valid encrypted payload.");
        }

        var nonce = encryptedPayload[..NonceSize];
        var tag = encryptedPayload[NonceSize..(NonceSize + TagSize)];
        var ciphertext = encryptedPayload[(NonceSize + TagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(CreateKey(encryptionKey), TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return FormatJson(Encoding.UTF8.GetString(plaintext));
    }

    private static byte[] DecodePayload(string encryptedData)
    {
        var normalized = encryptedData.Trim().Replace(' ', '+');

        if (normalized.Contains('-') || normalized.Contains('_'))
        {
            normalized = normalized.Replace('-', '+').Replace('_', '/');
            normalized = normalized.PadRight(normalized.Length + (4 - normalized.Length % 4) % 4, '=');
        }

        return Convert.FromBase64String(normalized);
    }

    private static byte[] CreateKey(string key)
        => SHA256.HashData(Encoding.UTF8.GetBytes(key));

    private static string FormatJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
    }
}
