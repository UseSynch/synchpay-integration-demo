# SynchPay Integration Demo

Simple Blazor Web App demo for creating a SynchPay payment request and opening the returned payment page inside an iframe.

## Prerequisites

- .NET SDK 9.0 or newer
- Internet access to reach SynchPay API endpoints

## Run the App

From the project directory:

```powershell
dotnet run
```

The app will start on the URL shown in the terminal. With the generated launch settings, this is usually:

```text
http://localhost:5111
```

Open that URL in a browser.

## How to Use

1. Enter the SynchPay API credentials:
   - `Client ID`
   - `Client secret`
2. Enter payment details:
   - `Company ID`
   - `Customer phone number`
   - `Payment amount`
3. Click `Create payment`.
4. The app authenticates with SynchPay, creates a payment request, displays the returned payment URL, and loads it in the iframe below the form.

## Amount Format

The amount input is entered as a normal decimal amount, for example:

```text
4
4.25
```

Before sending the request to SynchPay, the app converts the amount to cents:

```text
4    -> 400
4.25 -> 425
```

## API Flow

When `Create payment` is clicked, the app performs two server-side API calls.

First, it authenticates:

```http
POST https://api.trysynch.com/auth/token
Content-Type: application/json
```

Request body:

```json
{
  "ClientId": "<client-id>",
  "ClientSecret": "<client-secret>"
}
```

Then it creates the payment request:

```http
POST http://api.trysynch.com/payment/create
Content-Type: application/json
Authorization: Bearer <access-token>
```

Request body:

```json
{
  "ContactNumber": "<customer-phone-number>",
  "CompanyId": "<company-id>",
  "Amount": 400,
  "FeePayer": "partner"
}
```

The `url` returned by the payment creation endpoint is displayed above the iframe and loaded into the iframe.

## Project Structure

- `Components/Pages/Home.razor` - main Blazor view with the form and iframe
- `Services/SynchPayApiClient.cs` - SynchPay authentication and payment API calls
- `wwwroot/app.css` - demo styling

## Notes

- API credentials are entered in the UI for demo purposes.
- API calls are made server-side by the Blazor app.
- The payment fee payer is currently hardcoded as `partner`.
