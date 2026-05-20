<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaymentStatus.aspx.cs" Inherits="SynchPayIntegrationDemo.Net45.PaymentStatus" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Payment status</title>
    <link rel="stylesheet" href="Styles/site.css" />
</head>
<body>
    <form id="MainForm" runat="server">
        <main class="app-shell">
            <div class="payment-page payment-status-page">
                <section class="payment-header">
                    <p class="eyebrow">SynchPay integration demo</p>
                    <h1>Payment status</h1>
                </section>

                <section class="payment-form">
                    <div class="form-grid">
                        <label>
                            <span>Status encryption key</span>
                            <input id="StatusEncryptionKeyInput" class="form-control" type="password" autocomplete="off" />
                        </label>

                        <label>
                            <span>Encrypted data</span>
                            <textarea id="EncryptedDataInput" class="form-control encrypted-data-input"><%= Server.HtmlEncode(EncryptedData) %></textarea>
                        </label>
                    </div>

                    <button id="DecryptStatusButton" class="btn btn-primary create-button" type="button">Decrypt status</button>

                    <div id="ErrorPanel" class="alert alert-danger" role="alert" hidden></div>
                </section>

                <section id="StatusResult" class="status-result" aria-label="Decrypted payment status">
                    <div id="StatusPlaceholder" class="iframe-placeholder">Payment status will appear here.</div>
                    <pre id="StatusJson" hidden></pre>
                </section>
            </div>
        </main>
    </form>

    <script src="Scripts/status-decryption.js"></script>
</body>
</html>
