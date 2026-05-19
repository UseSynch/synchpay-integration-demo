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
            PaymentPlaceholderPanel.Visible = true;
            PaymentFrameLiteral.Text = string.Empty;
            PaymentUrlTextBox.Text = "Payment URL will appear here.";

            try
            {
                var form = CreatePaymentFormModel();
                var synchPay = new SynchPayApiClient();
                var paymentUrl = await synchPay.CreatePaymentAsync(form);

                PaymentUrlTextBox.Text = paymentUrl;
                PaymentPlaceholderPanel.Visible = false;
                PaymentFrameLiteral.Text = "<iframe title=\"SynchPay payment\" src=\"" + HttpUtility.HtmlAttributeEncode(paymentUrl) + "\"></iframe>";
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
                Amount = amount
            };
        }

        private static bool TryParseAmount(string value, out decimal amount)
        {
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount)
                || decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out amount);
        }
    }
}
