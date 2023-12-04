using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text.Json;
using VirgilAgent.ChatService.Services;
using VirgilAgent.Core;

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
string azureOpenAIOptionsSectionName = "AzureOpenAI";
AzureOpenAIOptions azureOpenAIOptions = configuration.GetSection(azureOpenAIOptionsSectionName).Get<AzureOpenAIOptions>()
	?? throw new InvalidOperationException($"Missing configuration section: {azureOpenAIOptionsSectionName}");
builder.Services.AddSingleton(azureOpenAIOptions);

// Add cache.
builder.AddCache("Cache");

// Add AI services.
string chatOptionsSectionName = "Chat";
ChatOptions chatOptions = configuration.GetSection(chatOptionsSectionName).Get<ChatOptions>()
	?? throw new InvalidOperationException($"Missing configuration section: {chatOptionsSectionName}");
builder.Services.AddSingleton(chatOptions);
builder.Services.AddSingleton<IChatAIClient, AzureOpenAIChatAIClient>();
builder.Services.AddSingleton<ChatService>();

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

app.MapGet("/start",
	async (
		[FromServices] ChatService chatService,
		[FromQuery] string? locale,
		[FromQuery] string? conversationId) =>
	{
		// Create a new conversation with the initial message.
		Conversation conversation = await chatService.StartConversationAsync(conversationId, locale);

		// Get the initial message.
		string responseMessage = conversation.Messages.FirstOrDefault()?.Content ?? string.Empty;

		// Return the response.
		ChatMessageResponse response = new(responseMessage, conversation.Id);

		return Results.Ok(response);
	})
.WithName("Start")
.WithDescription("Start a new conversation in the specified locale.")
.WithOpenApi();

app.MapPost("/chat",
	async (
		[FromServices] ChatService chatService,
		[FromServices] ILogger<Program> logger,
		[FromBody] ChatMessageRequest request) =>
	{
		try
		{
			// Generate the response message.
			string responseMessage =
				await chatService.GetChatResponseAsync(request.Message, request.ConversationId);

			// Return the response.
			ChatMessageResponse response = new(responseMessage, request.ConversationId);

			return Results.Ok(response);
		}
		catch (ArgumentException argEx)
		{
			logger.LogWarning("Invalid arguments: {errorMessage}", argEx.Message);

			return Results.BadRequest(argEx.Message);
		}
	})
.WithName("Chat")
.WithDescription("Send a chat message and get a response.")
.WithOpenApi();

app.Run();
