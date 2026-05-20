# SynchPay Integration Demo - .NET Framework 4.5

This folder contains an ASP.NET Web Forms copy of the SynchPay integration demo targeting .NET Framework 4.5.

## Prerequisites

- Visual Studio 2013 or newer with .NET Framework 4.5 targeting support
- IIS Express or IIS
- Internet access to reach SynchPay API endpoints

## Run the App

1. Open `SynchPayIntegrationDemo.Net45.sln` in Visual Studio.
2. Restore/build the solution.
3. Start the `SynchPayIntegrationDemo.Net45` web project with IIS Express.

## Behavior

The Web Forms page mirrors the Blazor app:

1. Enter `Client ID`, `Client secret`, `Company ID`, `Customer phone number`, `Payment amount`, `Return URL`, `Registration ID`, and `Status encryption key`.
2. Click `Create payment`.
3. The server authenticates with SynchPay, creates the payment request, displays the returned payment URL, and loads it in the iframe.
4. If the payment completion redirect points back to this app at `/PaymentStatus.aspx?data=<encrypted-payload>`, the status page decrypts and displays the returned JSON.
5. If `registrationPersonId` is returned by payment creation, it is copied into the `Registration ID` input for the next payment request.

The amount is entered as a normal decimal amount and sent to SynchPay as cents. For example, `4.25` is sent as `425`.

## Payment Status Redirect

The status decrypt page is available at:

```text
http://localhost:5112/PaymentStatus.aspx?data=<encrypted-payload>
```

The page expects the `data` query parameter to contain the payload produced by SynchPay's AES-GCM status encryption:

```text
base64(nonce[12] + tag[16] + ciphertext)
```

The key entered in `Status encryption key` on the payment form is stored in browser session storage and reused by the payment status page. The .NET Framework 4.5 page decrypts in the browser with WebCrypto because .NET Framework 4.5 does not provide a built-in AES-GCM API.
