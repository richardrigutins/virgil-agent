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

// Bind OpenAIOptions from configuration.
string openAIOptionsSection = "OpenAI";
var openAIOptions = configuration.GetSection(openAIOptionsSection).Get<OpenAIOptions>()
	?? throw new InvalidOperationException($"Missing configuration section: {openAIOptionsSection}");
builder.Services.AddSingleton(openAIOptions);

// Add AI services.
builder.Services.AddSingleton<ISuggestionsAIClient, OpenAISuggestionsAIClient>();
builder.Services.AddSingleton<SuggestionsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Handle exceptions.
app.UseExceptionHandler((exceptionApp) =>
{
	exceptionApp.Run(async context =>
	{
		// Return all error messages as JSON.
		context.Response.ContentType = MediaTypeNames.Application.Json;

		var feature = context.Features.Get<IExceptionHandlerPathFeature>();

		string errorMessage;

		// If the exception is of type BadHttpRequestException, set the status code to 400 and serialize the exception message.
		if (feature?.Error is BadHttpRequestException ex)
		{
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			errorMessage = JsonSerializer.Serialize(ex.Message);
		}
		// For all other types of exceptions, set the status code to 500 and return a generic error message.
		else
		{
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			errorMessage = JsonSerializer.Serialize("An unexpected error happened. Try again later.");
		}

		// Write the error message to the response.
		await context.Response.WriteAsync(errorMessage);
	});
});

app.MapDefaultEndpoints();

// Endpoint for getting suggestions.
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
.WithDescription("Get suggested follow-up actions for a given message")
.WithOpenApi();

app.Run();
