using Microsoft.AspNetCore.Mvc;
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

// Bind OpenAIOptions from configuration.
var azureOpenAIOptions = configuration.GetSection(AzureOpenAIOptions.SectionName).Get<AzureOpenAIOptions>()
	?? throw new InvalidOperationException($"Missing configuration section: {AzureOpenAIOptions.SectionName}");
builder.Services.AddSingleton(azureOpenAIOptions);

// Add cache.
builder.AddCache("Cache");

// Add AI services.
builder.Services.AddSingleton<IChatAIClient, AzureOpenAIChatAIClient>();
builder.Services.AddSingleton<ChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseExceptionHandler();

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
		[FromServices] ILogger<ChatService> logger,
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
