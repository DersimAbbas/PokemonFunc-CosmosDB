using AzureLabb3.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
var conn = builder.Configuration
    .GetConnectionString("cosmosDB")
     ?? throw new InvalidOperationException("Connection string 'AzureConnection:cosmosdb' not found.");
builder.Services.AddScoped<CosmosRepository>(_ => new CosmosRepository(
    connectionString: conn,
    databaseName: "PokemonDB"
));
builder.Build().Run();
