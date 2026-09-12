using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using OcrOrchestratorApi.BusinessLogic;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var config = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Real Azure Document Intelligence client
builder.Services.AddSingleton(sp =>
{
    var endpoint = config["AzureDocumentIntelligence:Endpoint"];
    var key = config["AzureDocumentIntelligence:ApiKey"];
    return new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(key));
});
builder.Services.AddSingleton<IOcrClient, AzureOcrClient>();

// Real metadata extractor
builder.Services.AddSingleton<IMetadataExtractor, PdfPigMetadataExtractor>();

// Real Azure OpenAI client
builder.Services.AddSingleton(sp =>
{
    var endpoint = config["AzureOpenAI:Endpoint"];
    var key = config["AzureOpenAI:ApiKey"];
    return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
});
builder.Services.AddSingleton<IExplanationClient>(sp =>
{
    var client = sp.GetRequiredService<AzureOpenAIClient>();
    var deployment = config["AzureOpenAI:DeploymentName"];
    return new AzureExplanationClient(client, deployment);
});

// Register a typed HttpClient implementation for ITamperServiceClient
builder.Services.AddHttpClient<ITamperService, TamperServiceClient_old>(client =>
{
    var baseUrl = builder.Configuration["TamperService:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    
});

// Register TamperServiceClient
builder.Services.AddTransient<ITamperService, TamperService>();

//// Register TamperServiceClient with HttpClient
//builder.Services.AddHttpClient<TamperServiceClient_old>(client =>
//{
//    // Base URL of the Python tamper detection microservice
//    var baseUrl = builder.Configuration["TamperService:BaseUrl"];
//    client.BaseAddress = new Uri(baseUrl);
//});

// Register orchestrator
builder.Services.AddTransient<FraudDetectionOrchestrator>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
