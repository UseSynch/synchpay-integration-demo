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

        public async Task<CreatePaymentResult> CreatePaymentAsync(PaymentFormModel form)
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
                    var authJson = await authResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    EnsureSuccessStatusCode(authResponse, authJson, "Authentication");

                    var auth = DeserializeJsonObject(authJson, "Authentication response was empty.");
                    var accessToken = GetRequiredString(auth, "AccessToken", "Authentication response did not include an access token.");
                    var tokenType = GetOptionalString(auth, "TokenType");

                    if (string.IsNullOrWhiteSpace(tokenType))
                    {
                        tokenType = "Bearer";
                    }

                    var paymentRequest = CreatePaymentRequest(form);
                    var paymentRequestJson = SerializeJson(paymentRequest);

                    using (var paymentMessage = new HttpRequestMessage(HttpMethod.Post, PaymentEndpoint))
                    {
                        paymentMessage.Content = CreateJsonContent(paymentRequestJson);
                        paymentMessage.Headers.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);

                        using (var paymentResponse = await httpClient.SendAsync(paymentMessage).ConfigureAwait(false))
                        {
                            var paymentJson = await paymentResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                            EnsureSuccessStatusCode(paymentResponse, paymentJson, "Payment creation", paymentRequestJson);

                            var payment = DeserializeJsonObject(paymentJson, "Payment response was empty.");
                            return new CreatePaymentResult
                            {
                                Url = GetRequiredString(payment, "Url", "Payment response did not include a URL."),
                                RegistrationPersonId = GetOptionalString(payment, "RegistrationPersonId"),
                                PaymentRequestBody = paymentRequestJson
                            };
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
            return CreateJsonContent(SerializeJson(value));
        }

        private static HttpContent CreateJsonContent(string json)
        {
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static string SerializeJson(object value)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(value);
        }

        private static IDictionary<string, object> CreatePaymentRequest(PaymentFormModel form)
        {
            var request = new Dictionary<string, object>
            {
                { "ContactNumber", form.ContactNumber },
                { "CompanyId", form.CompanyId },
                { "Amount", form.AmountInCents },
                { "FeePayer", "partner" }
            };

            AddOptionalValue(request, "ReturnUrl", form.ReturnUrl);
            AddOptionalValue(request, "RegistrationId", form.RegistrationId);

            return request;
        }

        private static void AddOptionalValue(IDictionary<string, object> request, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Add(key, value.Trim());
            }
        }

        private static void EnsureSuccessStatusCode(HttpResponseMessage response, string responseBody, string operationName, string paymentRequestBody = null)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var message = new StringBuilder();
            message.Append(operationName);
            message.Append(" request failed with HTTP ");
            message.Append((int)response.StatusCode);
            message.Append(" ");
            message.Append(response.ReasonPhrase);
            message.Append(".");

            if (!string.IsNullOrWhiteSpace(paymentRequestBody))
            {
                message.AppendLine();
                message.AppendLine();
                message.AppendLine("Payment create request body:");
                message.Append(paymentRequestBody);
            }

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                message.AppendLine();
                message.AppendLine();
                message.AppendLine("Response body:");
                message.Append(responseBody);
            }

            throw new SynchPayApiException(message.ToString(), paymentRequestBody);
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

    }

    public sealed class CreatePaymentResult
    {
        public string Url { get; set; }

        public string RegistrationPersonId { get; set; }

        public string PaymentRequestBody { get; set; }
    }

    public sealed class SynchPayApiException : InvalidOperationException
    {
        public SynchPayApiException(string message, string paymentRequestBody)
            : base(message)
        {
            PaymentRequestBody = paymentRequestBody;
        }

        public string PaymentRequestBody { get; private set; }
    }
}
