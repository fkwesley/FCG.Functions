using FCG.Functions.ApiClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Cria o builder para configurar a aplicação de Azure Functions
var builder = FunctionsApplication.CreateBuilder(args);

// Configura o ambiente web para as Azure Functions
builder.ConfigureFunctionsWebApplication();

// Configura o Application Insights para monitoramento e telemetria
builder.Services
    .AddApplicationInsightsTelemetryWorkerService() // Adiciona suporte ao Application Insights para o worker
    .ConfigureFunctionsApplicationInsights(); // Configura o Application Insights para Azure Functions

// Register the HttpClient dependency
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IApiClient, ApiClient>();

// Constrói e executa a aplicação
builder.Build().Run();
