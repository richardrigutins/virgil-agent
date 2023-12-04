# VirgilAgent - your virtual tourist guide

**VirgilAgent** is a simple chatbot application that helps you get tourist information and plan your next trip.

The application allows the user to chat with **Virgil**, an AI-powered virtual tourist guide that replies to the user's questions about cities, art, places of interest, restaurants, hotels, etc. in a natural language, using the same language as the user.

Virgil helps the user by suggesting places to visit, finding the opening hours of a museum, helping organize the trip, finding the best restaurants and hotels, and much more.

Virgil also enriches chat messages by suggesting follow-up actions (e.g. opening a map, providing a link to a website, etc.).

## Architecture

Virgil Agent is powered by .NET 8, using [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview) to build and deploy the application, and the [Microsoft Bot Framework](https://dev.botframework.com/) to provide the chatbot functionality.

The AI capabilities are provided by the [Azure OpenAI Service](https://azure.microsoft.com/en-us/products/ai-services/openai-service).

The application consists of multiple components:

- **Chat service**: handles the user's chat messages and generates responses using the AI service.
- **Suggestions service**: generates suggestions for follow-up actions based on an input message (e.g. suggested replies, opening a map, website links, etc.).
- **Cache**: stores the user's chat history for a given conversation. It is run using [Redis](https://redis.io/).
- **Bot**: the chatbot application that handles the user's messages and generates responses using the chat service and the suggestions service.

Chat messages are not persisted in a database, but are stored in the cache for a limited amount of time. Previous messages are used as context for the AI service to generate better responses.

Because of the limitations of the AI services, only a given number of messages are stored in the cache for a conversation; after that, old messages are removed from the cache whenever a new message is received.

VirgilAgent does not currently include a frontend application, but, through the Bot functionality, it can be integrated inside web pages or with a variety of channels (e.g. Telegram, Teams, etc.).

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/)
- [Docker](https://www.docker.com/)
- [Bot Framework Emulator](https://github.com/microsoft/BotFramework-Emulator/blob/master/README.md)
- .NET Aspire workload
- Azure OpenAI Service endpoint and key

### Required configuration

Before running the application, you need to configure the Azure OpenAI endpoint and key that will be used by the chat service and the suggestions service.

To do so, open the `appsettings.json` file in both the `VirgilAgent.ChatService` and `VirgilAgent.SuggestionsService` projects, and set the following values to the endpoint and key of your Azure OpenAI service:

```json
"AzureOpenAI": {
    "Endpoint": "YOUR_ENDPOINT_HERE",
    "Key": "YOUR_KEY_HERE",
    ...
},
```

### Additional configuration

The application has a few additional configuration options that you can set in the `appsettings.json` files, to customize the behavior of the application and tweak the performance of the AI service.

For instance, the `AzureOpenAI` section can be used to set the parameters of the Azure OpenAI service (e.g. the temperature of the generated responses, the maximum number of tokens, etc.).
Instead, the `Chat` section can be used to set the maximum number of messages that are stored in the cache for a conversation (a greater number of messages will improve the quality of the responses, but will also increase the number of tokens used by the AI service).

> **Note**: be careful to set the parameters appropriately, to avoid exceeding the limits of the Azure OpenAI service (e.g. the maximum number of tokens for a conversation).

### Running the application

To run the application locally, follow these steps:

1. Ensure that the Docker daemon is running. This is required to run the Redis cache.
2. Run the `VirgilAgent.AppHost` project. For instance, you can set it as the startup project in Visual Studio, or you can run it from the command line, using the `dotnet run` command and specifying the path to the project file.
3. Open the .NET Aspire developer dashboard at `http://localhost:15173` and ensure that all the services are running.
4. Open the Bot Framework Emulator and connect to the chatbot at `http://localhost:3978/api/messages`.
5. Start chatting with Virgil!

## Deploying the application

The application can be deployed to Azure using Azure Container Apps and an Azure Bot.

### 1. Deploying the application to Azure Container Apps

The services can be deployed using the [Azure Developer CLI (azd)](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/).

Once you have the CLI installed, run the following commands:

1. Login to Azure: `azd auth login`
2. Change to the host project directory: `cd src/VirgilAgent.Aspire.AppHost`
3. Initialize the project (if not already initialized) and provide an environment name: `azd init`. Only the bot service needs to be accessible from the internet, so you can set the other services to be private.
4. Create the resources and deploy the application: `azd up`

Once the application is deployed, update the environment variables on the chat service and suggestions service to set the Azure OpenAI endpoint and key (see the [Required configuration](#required-configuration) section for more details).

### 2. Creating the Azure Bot

To create an Azure Bot resource, refer to the following documentation:

- [Create an Azure Bot resource in the Azure portal - Bot Service | Microsoft Learn](https://learn.microsoft.com/en-us/azure/bot-service/abs-quickstart?view=azure-bot-service-4.0&tabs=userassigned)
- [Provision and publish a bot in Azure - Bot Service | Microsoft Learn](https://learn.microsoft.com/en-us/azure/bot-service/provision-and-publish-a-bot?view=azure-bot-service-4.0&source=recommendations&tabs=userassigned%2Ccsharp)

As an alternative, you can also use the Azure Resource Manager (ARM) template in the `src/VirgilAgent.BotService/DeploymentTemplates` folder to create the Azure Bot resource.

Once the Azure Bot is created, you need to configure the bot to use the chatbot application that you deployed to Azure Container Apps.

To do so, open the Azure Bot resource in the Azure portal, and follow these steps:

1. Click on **Configuration** in the left menu.
2. Set the **Messaging endpoint** to the URL of the bot service (including the `/api/messages` path).

Finally, update the environment variables on the bot service and set the following values, according to how you created the Azure Bot resource (see the previous documentation for more details):

- MicrosoftAppType
- MicrosoftAppId
- MicrosoftAppPassword
- MicrosoftAppTenantId

> **Note**: if you create the Azure Bot resource using the User Assigned Identity option, you need to assign the same identity to the bot service.

## Future work

Features planned for future work include:

- Photo recognition: Virgil will be able to recognize objects in photos (monuments, paintings, etc.) and provide information about them.
- Mobile app: Virgil will be available as a mobile app, with additional features (e.g. speech recognition, saving past conversations, etc.).

## Contributing

Contributions are welcome! Here are some ways you can contribute:

- Report bugs and suggest new features.
- Improve the documentation.
- Fix bugs or implement new features.

Before submitting a pull request, please make sure that your changes are consistent with the project's coding style.
