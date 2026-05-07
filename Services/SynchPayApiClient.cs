using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SynchPayIntegrationDemo.Services;

public sealed class SynchPayApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true
    };

    public async Task<string> CreatePaymentAsync(PaymentFormModel form, CancellationToken cancellationToken = default)
    {
        var authRequest = new AuthTokenRequest(form.ClientId, form.ClientSecret);
        using var authResponse = await httpClient.PostAsJsonAsync(
            "https://api.trysynch.com/auth/token",
            authRequest,
            JsonOptions,
            cancellationToken);

        authResponse.EnsureSuccessStatusCode();
        var auth = await authResponse.Content.ReadFromJsonAsync<AuthTokenResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Authentication response was empty.");

        if (string.IsNullOrWhiteSpace(auth.AccessToken))
        {
            throw new InvalidOperationException("Authentication response did not include an access token.");
        }

        var paymentRequest = new CreatePaymentRequest(
            form.ContactNumber,
            form.CompanyId,
            form.AmountInCents,
            "partner");

        using var paymentMessage = new HttpRequestMessage(HttpMethod.Post, "http://api.trysynch.com/payment/create")
        {
            Content = JsonContent.Create(paymentRequest, options: JsonOptions)
        };
        paymentMessage.Headers.Authorization = new AuthenticationHeaderValue(auth.TokenType ?? "Bearer", auth.AccessToken);

        using var paymentResponse = await httpClient.SendAsync(paymentMessage, cancellationToken);
        paymentResponse.EnsureSuccessStatusCode();

        var payment = await paymentResponse.Content.ReadFromJsonAsync<CreatePaymentResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Payment response was empty.");

        return string.IsNullOrWhiteSpace(payment.Url)
            ? throw new InvalidOperationException("Payment response did not include a URL.")
            : payment.Url;
    }
}

public sealed class PaymentFormModel
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    public string CompanyId { get; set; } = string.Empty;

    [Required]
    public string ContactNumber { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; } = 400;

    public int AmountInCents => (int)Math.Round(Amount * 100, MidpointRounding.AwayFromZero);
}

internal sealed record AuthTokenRequest(string ClientId, string ClientSecret);

internal sealed record AuthTokenResponse(string AccessToken, string? TokenType, int ExpiresInSeconds);

internal sealed record CreatePaymentRequest(string ContactNumber, string CompanyId, int Amount, string FeePayer);

internal sealed record CreatePaymentResponse(string PaymentRequestId, string? AccountMask, string Url);
