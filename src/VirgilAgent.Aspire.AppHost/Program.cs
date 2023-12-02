var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedisContainer("redis");

builder.AddProject<Projects.VirgilAgent_ChatService>("chatservice")
	.WithReference(redis);

builder.Build().Run();