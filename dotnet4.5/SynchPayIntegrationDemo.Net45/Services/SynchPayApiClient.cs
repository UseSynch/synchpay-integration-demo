using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SynchPayIntegrationDemo.Net45.Models;

namespace SynchPayIntegrationDemo.Net45.Services
{
    public sealed class SynchPayApiClient
    {
        private static readonly Uri AuthEndpoint = new Uri("https://api.trysynch.com/auth/token");
        private static readonly Uri PaymentEndpoint = new Uri("http://api.trysynch.com/payment/create");

        public async Task<string> CreatePaymentAsync(PaymentFormModel form)
        {
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | (SecurityProtocolType)3072;

            using (var httpClient = new HttpClient())
            {
                var authRequest = new AuthTokenRequest
                {
                    ClientId = form.ClientId,
                    ClientSecret = form.ClientSecret
                };

                using (var authResponse = await PostJsonAsync(httpClient, AuthEndpoint, authRequest).ConfigureAwait(false))
                {
                    authResponse.EnsureSuccessStatusCode();

                    var authJson = await authResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var auth = DeserializeJsonObject(authJson, "Authentication response was empty.");
                    var accessToken = GetRequiredString(auth, "AccessToken", "Authentication response did not include an access token.");
                    var tokenType = GetOptionalString(auth, "TokenType");

                    if (string.IsNullOrWhiteSpace(tokenType))
                    {
                        tokenType = "Bearer";
                    }

                    var paymentRequest = new CreatePaymentRequest
                    {
                        ContactNumber = form.ContactNumber,
                        CompanyId = form.CompanyId,
                        Amount = form.AmountInCents,
                        FeePayer = "partner"
                    };

                    using (var paymentMessage = new HttpRequestMessage(HttpMethod.Post, PaymentEndpoint))
                    {
                        paymentMessage.Content = CreateJsonContent(paymentRequest);
                        paymentMessage.Headers.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);

                        using (var paymentResponse = await httpClient.SendAsync(paymentMessage).ConfigureAwait(false))
                        {
                            paymentResponse.EnsureSuccessStatusCode();

                            var paymentJson = await paymentResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var payment = DeserializeJsonObject(paymentJson, "Payment response was empty.");
                            return GetRequiredString(payment, "Url", "Payment response did not include a URL.");
                        }
                    }
                }
            }
        }

        private static Task<HttpResponseMessage> PostJsonAsync(HttpClient httpClient, Uri uri, object value)
        {
            return httpClient.PostAsync(uri, CreateJsonContent(value));
        }

        private static HttpContent CreateJsonContent(object value)
        {
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(value);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static IDictionary<string, object> DeserializeJsonObject(string json, string emptyMessage)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException(emptyMessage);
            }

            var serializer = new JavaScriptSerializer();
            var values = serializer.DeserializeObject(json) as IDictionary<string, object>;

            if (values == null)
            {
                throw new InvalidOperationException(emptyMessage);
            }

            return values;
        }

        private static string GetRequiredString(IDictionary<string, object> values, string key, string message)
        {
            var value = GetOptionalString(values, key);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(message);
            }

            return value;
        }

        private static string GetOptionalString(IDictionary<string, object> values, string key)
        {
            foreach (var item in values)
            {
                if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return Convert.ToString(item.Value);
                }
            }

            return null;
        }

        private sealed class AuthTokenRequest
        {
            public string ClientId { get; set; }

            public string ClientSecret { get; set; }
        }

        private sealed class CreatePaymentRequest
        {
            public string ContactNumber { get; set; }

            public string CompanyId { get; set; }

            public int Amount { get; set; }

            public string FeePayer { get; set; }
        }
    }
}
