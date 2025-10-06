# Azure Functions Project

## Overview
This project contains three Azure Functions designed to process messages from a Service Bus topic and forward them to an external Payments API. Each function handles a specific type of payment: Credit Card, Debit Card, and Pix.

### Functions
1. **CreditCardFunction**: Processes messages from the `FCG.Payments.CreditCard` subscription.
2. **DebitCardFunction**: Processes messages from the `FCG.Payments.DebitCard` subscription.
3. **PixFunction**: Processes messages from the `FCG.Payments.Pix` subscription.

All functions use the `IApiClient` to send data to the Payments API.

### Tests
Unit tests have been implemented for all three functions:
- **CreditCardFunctionTests**: Verifies the behavior of the `CreditCardFunction` under various scenarios, including successful API calls, failed API calls, and exceptions.
- **DebitCardFunctionTests**: Similar to `CreditCardFunctionTests`, but for the `DebitCardFunction`.
- **PixFunctionTests**: Similar to `CreditCardFunctionTests`, but for the `PixFunction`.

## Folder Structure

### Explanation of Folders and Files
- **ApiClient**: Contains the implementation and interface for the API client used to communicate with the Payments API.
- **Functions**: Houses the Azure Functions, each responsible for processing a specific type of payment.
- **Tests**: Contains unit tests for the functions and the `ApiClient`.
- **local.settings.json**: Stores environment variables for local development, such as connection strings and API credentials.
- **host.json**: Configures runtime settings for Azure Functions, such as concurrency limits.
- **Program.cs**: The main entry point for the application, where dependencies are registered and the application is configured.
- **README.md**: Provides documentation for the project.

## Configuration

### Environment Variables
The following environment variables must be configured in the `local.settings.json` file for local development or in Azure Application Settings for deployment:

- `ServiceBusConnection`: Connection string for the Azure Service Bus.
- `PaymentsAPI_Url`: URL of the Payments API endpoint.
- `PaymentsAPI_Token`: Authentication token for the Payments API.

Example `local.settings.json`:

### host.json
To ensure the functions process one message at a time, the `host.json` file should include the following configuration:

## Dependencies
- **IApiClient**: A custom API client for making HTTP requests to the Payments API.
- **HttpClient**: Used internally by the `ApiClient` for HTTP communication.
- **ILogger**: For logging information, warnings, and errors.

## Local Setup
1. Clone the repository and navigate to the project directory.
2. Configure the `local.settings.json` file with the required environment variables.
3. Ensure the `host.json` file is properly configured.
4. Run the function locally using the following command:

## Deployment

The deployment process is automated using GitHub Actions. The CI/CD pipeline is defined in the `.github/workflows/ci-cd.yml` file and includes the following steps:

1. **Build and Test**:
   - The code is built and tested on every push to any branch.
   - Unit tests are executed, and code coverage is collected.

2. **Deploy to DEV**:
   - After successful build and test, the application is deployed to the DEV environment.
   - The Azure Function App for DEV is `func-fcg-payments-dev`.

3. **Deploy to UAT**:
   - After the DEV deployment, the application is deployed to the UAT environment.
   - The Azure Function App for UAT is `func-fcg-payments-uat`.

4. **Deploy to PRD**:
   - Deployment to PRD occurs only on pushes to the `main` branch.
   - The Azure Function App for PRD is `azure-functions-project-prd`.

### Environment Variables
The following environment variables are configured in GitHub Secrets and Variables:
- `AZURE_CREDENTIALS`: Azure service principal credentials for authentication.
- `SERVICEBUS_CONNECTION`: Connection string for the Azure Service Bus.
- `PAYMENTS_API_URL`: URL of the Payments API endpoint.
- `PAYMENTS_API_TOKEN`: Authentication token for the Payments API.
- `ENV`: Specifies the environment (e.g., Development, UAT, Production).

### Manual Trigger
To manually trigger a deployment, push changes to the desired branch or use the GitHub Actions workflow dispatch feature.

## Example Usage
1. Send a message to the `fcg.paymentstopic` topic with the appropriate subscription:
- `FCG.Payments.CreditCard` for `CreditCardFunction`
- `FCG.Payments.DebitCard` for `DebitCardFunction`
- `FCG.Payments.Pix` for `PixFunction`
2. The corresponding function will process the message and forward it to the Payments API.
3. Monitor the logs for success or failure details.

## Troubleshooting
- Ensure the Service Bus connection string is valid and has the necessary permissions.
- Verify the Payments API URL and token are correct.
- Check the logs for detailed error messages in case of failures.
- For `DebitCardFunction` and `PixFunction`, ensure the respective subscriptions (`FCG.Payments.DebitCard` and `FCG.Payments.Pix`) are correctly configured in the Service Bus.