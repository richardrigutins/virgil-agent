using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using VirgilAgent.BotService;
using VirgilAgent.BotService.Bots;
using VirgilAgent.BotService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
{
	options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
});

// Add API clients
builder.Services.AddHttpClient<ChatApiClient>(client => client.BaseAddress = new("http://chatservice"));
builder.Services.AddHttpClient<SuggestionsApiClient>(client => client.BaseAddress = new("http://suggestionsservice"));

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, VirgilBot>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
	.UseStaticFiles()
	.UseWebSockets()
	.UseRouting()
	.UseAuthorization()
	.UseEndpoints(endpoints =>
	{
		endpoints.MapControllers();
	});

app.Run();
