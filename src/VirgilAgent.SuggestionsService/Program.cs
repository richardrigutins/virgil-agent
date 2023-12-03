using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text.Json;
using VirgilAgent.Core;
using VirgilAgent.SuggestionsService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add Swagger/OpenAPI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;

// Bind AzureOpenAIOptions from configuration.
string azureOpenAIOptionsSection = "AzureOpenAI";
var azureOpenAIOptions = configuration.GetSection(azureOpenAIOptionsSection).Get<AzureOpenAIOptions>()
	?? throw new InvalidOperationException($"Missing configuration section: {azureOpenAIOptionsSection}");
builder.Services.AddSingleton(azureOpenAIOptions);

// Add AI services.
builder.Services.AddSingleton<ISuggestionsAIClient, AzureOpenAISuggestionsAIClient>();
builder.Services.AddSingleton<SuggestionsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseExceptionHandler((exceptionApp) =>
{
	exceptionApp.Run(async context =>
	{
		context.Response.ContentType = MediaTypeNames.Application.Json;
		var feature = context.Features.Get<IExceptionHandlerPathFeature>();
		string errorMessage;
		if (feature?.Error is BadHttpRequestException ex)
		{
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			errorMessage = JsonSerializer.Serialize(ex.Message);
		}
		else
		{
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			errorMessage = JsonSerializer.Serialize("An unexpected error happened. Try again later.");
		}

		await context.Response.WriteAsync(errorMessage);
	});
});

app.MapDefaultEndpoints();

app.MapGet("/suggestions",
	async (
		[FromServices] SuggestionsService suggestionsService,
		[FromServices] ILogger<Program> logger,
		[FromQuery] string message) =>
	{
		try
		{
			// Generate the response message.
			var response = await suggestionsService.GetSuggestedActionsAsync(message);

			return Results.Ok(response);
		}
		catch (ArgumentException argEx)
		{
			logger.LogWarning("Invalid arguments: {errorMessage}", argEx.Message);

			return Results.BadRequest(argEx.Message);
		}
	})
.WithName("GetSuggestions")
.WithDescription("Get suggested follow-up actions for given message")
.WithOpenApi();

app.Run();
