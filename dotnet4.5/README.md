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

1. Enter `Client ID`, `Client secret`, `Company ID`, `Customer phone number`, and `Payment amount`.
2. Click `Create payment`.
3. The server authenticates with SynchPay, creates the payment request, displays the returned payment URL, and loads it in the iframe.

The amount is entered as a normal decimal amount and sent to SynchPay as cents. For example, `4.25` is sent as `425`.
