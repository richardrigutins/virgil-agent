var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedisContainer("redis");

var chatService = builder.AddProject<Projects.VirgilAgent_ChatService>("chatservice")
	.WithReference(redis);

var suggestionsService = builder.AddProject<Projects.VirgilAgent_SuggestionsService>("suggestionsservice");

builder.AddProject<Projects.VirgilAgent_BotService>("botservice")
	.WithReference(chatService)
	.WithReference(suggestionsService);

builder.Build().Run();
