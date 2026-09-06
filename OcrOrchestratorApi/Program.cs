using Azure;
using Azure.AI.DocumentIntelligence;
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
    return new OpenAIClient(key);
});
builder.Services.AddSingleton<IExplanationClient>(sp =>
{
    var client = sp.GetRequiredService<OpenAIClient>();
    var deployment = config["AzureOpenAI:DeploymentName"];
    return new AzureExplanationClient(client, deployment);
});

// Register orchestrator
builder.Services.AddTransient<FraudDetectionOrchestrator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}
// Register orchestrator
builder.Services.AddTransient<FraudDetectionOrchestrator>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
