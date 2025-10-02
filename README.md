# Azure Functions Project

## Overview
This project contains three Azure Functions designed to process messages from a Service Bus topic and forward them to an external Payments API. Each function handles a specific type of payment: Credit Card, Debit Card, and Pix.

### Functions
1. **CreditCardFunction**: Processes messages from the `FCG.Payments.CreditCard` subscription.
2. **DebitCardFunction**: Processes messages from the `FCG.Payments.DebitCard` subscription.
3. **PixFunction**: Processes messages from the `FCG.Payments.Pix` subscription.

All functions use the `IApiClient` to send data to the Payments API.

## Configuration

### Environment Variables
The following environment variables must be configured in the `local.settings.json` file for local development or in Azure Application Settings for deployment:

- `ServiceBusConnection`: Connection string for the Azure Service Bus.
- `PaymentsAPI_Url`: URL of the Payments API endpoint.
- `PaymentsAPI_Token`: Authentication token for the Payments API.

Example `local.settings.json`: