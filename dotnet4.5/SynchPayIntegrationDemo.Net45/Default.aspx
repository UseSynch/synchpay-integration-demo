<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SynchPayIntegrationDemo.Net45.Default" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>SynchPay integration demo</title>
    <link rel="stylesheet" href="Styles/site.css" />
</head>
<body>
    <form id="MainForm" runat="server">
        <main class="app-shell">
            <div class="payment-page">
                <section class="payment-header">
                    <p class="eyebrow">SynchPay integration demo</p>
                    <h1>Create payment</h1>
                </section>

                <section class="payment-form">
                    <div class="form-grid">
                        <label>
                            <span>Client ID</span>
                            <asp:TextBox ID="ClientIdTextBox" runat="server" CssClass="form-control" autocomplete="off" />
                            <asp:RequiredFieldValidator ID="ClientIdRequiredValidator" runat="server" ControlToValidate="ClientIdTextBox" Display="Dynamic" CssClass="validation-message" ErrorMessage="Client ID is required." />
                        </label>

                        <label>
                            <span>Client secret</span>
                            <asp:TextBox ID="ClientSecretTextBox" runat="server" CssClass="form-control" TextMode="Password" autocomplete="off" />
                            <asp:RequiredFieldValidator ID="ClientSecretRequiredValidator" runat="server" ControlToValidate="ClientSecretTextBox" Display="Dynamic" CssClass="validation-message" ErrorMessage="Client secret is required." />
                        </label>

                        <label>
                            <span>Company ID</span>
                            <asp:TextBox ID="CompanyIdTextBox" runat="server" CssClass="form-control" autocomplete="off" />
                            <asp:RequiredFieldValidator ID="CompanyIdRequiredValidator" runat="server" ControlToValidate="CompanyIdTextBox" Display="Dynamic" CssClass="validation-message" ErrorMessage="Company ID is required." />
                        </label>

                        <label>
                            <span>Customer phone number</span>
                            <asp:TextBox ID="ContactNumberTextBox" runat="server" CssClass="form-control" autocomplete="tel" />
                            <asp:RequiredFieldValidator ID="ContactNumberRequiredValidator" runat="server" ControlToValidate="ContactNumberTextBox" Display="Dynamic" CssClass="validation-message" ErrorMessage="Customer phone number is required." />
                        </label>

                        <label>
                            <span>Payment amount</span>
                            <asp:TextBox ID="AmountTextBox" runat="server" CssClass="form-control" TextMode="Number" min="0.01" step="0.01" />
                            <asp:RequiredFieldValidator ID="AmountRequiredValidator" runat="server" ControlToValidate="AmountTextBox" Display="Dynamic" CssClass="validation-message" ErrorMessage="Payment amount is required." />
                            <asp:CustomValidator ID="AmountValidator" runat="server" ControlToValidate="AmountTextBox" Display="Dynamic" CssClass="validation-message" ErrorMessage="Payment amount must be at least 0.01." OnServerValidate="AmountValidator_ServerValidate" />
                        </label>
                    </div>

                    <asp:Button ID="CreatePaymentButton" runat="server" CssClass="btn btn-primary create-button" Text="Create payment" OnClick="CreatePaymentButton_Click" OnClientClick="return markSubmitting(this);" />

                    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert alert-danger" role="alert" Visible="false">
                        <asp:Literal ID="ErrorMessageLiteral" runat="server" />
                    </asp:Panel>
                </section>

                <section class="payment-preview" aria-label="Payment page preview">
                    <label class="url-bar">
                        <span>Payment URL</span>
                        <asp:TextBox ID="PaymentUrlTextBox" runat="server" CssClass="form-control" ReadOnly="true" />
                    </label>

                    <div class="iframe-shell">
                        <asp:Panel ID="PaymentPlaceholderPanel" runat="server" CssClass="iframe-placeholder">
                            Payment page will appear here.
                        </asp:Panel>
                        <asp:Literal ID="PaymentFrameLiteral" runat="server" Mode="PassThrough" />
                    </div>
                </section>
            </div>
        </main>
    </form>

    <script type="text/javascript">
        function markSubmitting(button) {
            if (typeof (Page_ClientValidate) === "function" && !Page_ClientValidate()) {
                return false;
            }

            window.setTimeout(function () {
                button.disabled = true;
                button.value = "Creating payment...";
            }, 0);

            return true;
        }
    </script>
</body>
</html>
