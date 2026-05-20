using System;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;
using SynchPayIntegrationDemo.Net45.Models;
using SynchPayIntegrationDemo.Net45.Services;

namespace SynchPayIntegrationDemo.Net45
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                AmountTextBox.Text = "1";
                ReturnUrlTextBox.Text = ResolveUrl("~/PaymentStatus.aspx");
                ReturnUrlTextBox.Text = new Uri(Request.Url, ReturnUrlTextBox.Text).ToString();
                PaymentUrlTextBox.Text = "Payment URL will appear here.";
            }
        }

        protected async void CreatePaymentButton_Click(object sender, EventArgs e)
        {
            Page.Validate();

            if (!Page.IsValid)
            {
                return;
            }

            ErrorPanel.Visible = false;
            ApiRequestPanel.Visible = false;
            PaymentPlaceholderPanel.Visible = true;
            PaymentFrameLiteral.Text = string.Empty;
            PaymentUrlTextBox.Text = "Payment URL will appear here.";
            OpenPaymentLink.Visible = false;

            try
            {
                var form = CreatePaymentFormModel();
                var synchPay = new SynchPayApiClient();
                var payment = await synchPay.CreatePaymentAsync(form);

                ShowPaymentRequestBody(payment.PaymentRequestBody);

                PaymentUrlTextBox.Text = payment.Url;
                OpenPaymentLink.NavigateUrl = payment.Url;
                OpenPaymentLink.Visible = true;

                if (!string.IsNullOrWhiteSpace(payment.RegistrationPersonId))
                {
                    RegistrationIdTextBox.Text = payment.RegistrationPersonId;
                }

                PaymentPlaceholderPanel.Visible = false;
                PaymentFrameLiteral.Text = "<iframe title=\"SynchPay payment\" sandbox=\"allow-scripts allow-forms allow-same-origin\" src=\"" + HttpUtility.HtmlAttributeEncode(payment.Url) + "\"></iframe>";
            }
            catch (SynchPayApiException ex)
            {
                ShowPaymentRequestBody(ex.PaymentRequestBody);
                ErrorMessageLiteral.Text = HttpUtility.HtmlEncode(ex.Message);
                ErrorPanel.Visible = true;
            }
            catch (Exception ex)
            {
                ErrorMessageLiteral.Text = HttpUtility.HtmlEncode(ex.Message);
                ErrorPanel.Visible = true;
            }
        }

        protected void AmountValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            decimal amount;
            args.IsValid = TryParseAmount(args.Value, out amount) && amount >= 0.01m;
        }

        private PaymentFormModel CreatePaymentFormModel()
        {
            decimal amount;
            if (!TryParseAmount(AmountTextBox.Text, out amount))
            {
                throw new InvalidOperationException("Payment amount must be a valid decimal number.");
            }

            return new PaymentFormModel
            {
                ClientId = ClientIdTextBox.Text.Trim(),
                ClientSecret = ClientSecretTextBox.Text.Trim(),
                CompanyId = CompanyIdTextBox.Text.Trim(),
                ContactNumber = ContactNumberTextBox.Text.Trim(),
                Amount = amount,
                ReturnUrl = ReturnUrlTextBox.Text.Trim(),
                RegistrationId = RegistrationIdTextBox.Text.Trim(),
                StatusEncryptionKey = StatusEncryptionKeyTextBox.Text
            };
        }

        private static bool TryParseAmount(string value, out decimal amount)
        {
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount)
                || decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out amount);
        }

        private void ShowPaymentRequestBody(string requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                ApiRequestPanel.Visible = false;
                ApiRequestBodyLiteral.Text = string.Empty;
                return;
            }

            ApiRequestBodyLiteral.Text = HttpUtility.HtmlEncode(requestBody);
            ApiRequestPanel.Visible = true;
        }
    }
}
