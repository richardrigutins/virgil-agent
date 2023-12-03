# Virgil Agent

**Virgil Agent** is a simple chatbot application that helps you get tourist information and plan your next trip.

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

Because of the limitations of the AI services, only a given number of messages is stored in the cache for a conversation; after that, old messages are removed from the cache whenever a new message is received.

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/)
- [Docker](https://www.docker.com/)
- [Bot Framework Emulator](https://github.com/microsoft/BotFramework-Emulator/blob/master/README.md)
- .NET Aspire workload
- Azure OpenAI endpoint and key

### Configuration

Before running the application, you need to configure the Azure OpenAI endpoint and key that will be used by the chat service and the suggestions service.

To do so, open the `appsettings.json` file in both the `VirgilAgent.ChatService` and `VirgilAgent.SuggestionsService` projects, and set the following values to the endpoint and key of your Azure OpenAI service:

```json
"AzureOpenAI": {
    "Endpoint": "YOUR_ENDPOINT_HERE",
    "Key": "YOUR_KEY_HERE",
    ...
},
```

## Contributing

Contributions are welcome! Here are some ways you can contribute:

- Report bugs and suggest new features.
- Improve the documentation.
- Fix bugs or implement new features.

Before submitting a pull request, please make sure that your changes are consistent with the project's coding style.
