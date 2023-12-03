﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using VirgilAgent.BotService.Services;
using VirgilAgent.Core;

namespace VirgilAgent.BotService.Bots;

internal class VirgilBot(ChatApiClient chatApiClient, SuggestionsApiClient suggestionsApiClient, ILogger<VirgilBot> logger)
	: ActivityHandler
{
	private const string ErrorResponseMessage = "Something went wrong, please try again.";

	private readonly ChatApiClient _chatApiClient = chatApiClient;
	private readonly SuggestionsApiClient _suggestionsApiClient = suggestionsApiClient;
	private readonly ILogger<VirgilBot> _logger = logger;

	protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
	{
		// If the user sent an attachment, then send an error message as a reply, indicating that this bot does not yet accept attachments.
		if (turnContext.Activity.Attachments is not null && turnContext.Activity.Attachments.Any())
		{
			await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, attachments aren't supported at the moment."), cancellationToken);
			return;
		}

		string inputText = turnContext.Activity.Text;
		string conversationId = turnContext.Activity.Conversation.Id;

		// Get a reply from the chat API.
		string responseMessage = await GetChatResponseAsync(inputText, conversationId);
		if (string.IsNullOrWhiteSpace(responseMessage))
		{
			await turnContext.SendActivityAsync(MessageFactory.Text(ErrorResponseMessage), cancellationToken);
			return;
		}

		// Try to get suggested actions from suggestions API.
		var suggestedActions = await TryGetSuggestedActions(responseMessage);

		var reply = MessageFactory.Text(responseMessage);
		if (suggestedActions.Any())
		{
			reply.SuggestedActions = new SuggestedActions()
			{
				Actions = suggestedActions.Select(MapToCardAction).ToList()
			};
		}

		await turnContext.SendActivityAsync(reply, cancellationToken);
	}

	protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
	{
		// Try to get the user locale
		string? locale = turnContext.Activity.GetLocale();

		string conversationId = turnContext.Activity.Conversation.Id;

		// Get the start message from the chat API.
		string welcomeText = await StartConversationAsync(locale, conversationId);
		if (string.IsNullOrWhiteSpace(welcomeText))
		{
			await turnContext.SendActivityAsync(MessageFactory.Text(ErrorResponseMessage), cancellationToken);
			return;
		}

		foreach (var member in membersAdded)
		{
			if (member.Id != turnContext.Activity.Recipient.Id)
			{
				await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
			}
		}
	}

	private async Task<string> GetChatResponseAsync(string message, string conversationId)
	{
		try
		{
			ChatMessageRequest chatRequest = new(message, conversationId);
			ChatMessageResponse chatResponse = await _chatApiClient.SendMessageAsync(chatRequest);

			return chatResponse.Message;
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "An error occurred while getting chat response from API");
			return string.Empty;
		}
	}

	private async Task<IEnumerable<SuggestedAction>> TryGetSuggestedActions(string message)
	{
		try
		{
			return await _suggestionsApiClient.GetSuggestedActionsAsync(message);
		}
		catch (ApiException apiEx)
		{
			// Log the error and return an empty list, without surfacing the error to the user.
			_logger.LogWarning("An error occurred while trying to get suggestions from API: {errorMessage}", apiEx.Message);
			return [];
		}
	}

	private async Task<string> StartConversationAsync(string? locale = null, string? conversationId = null)
	{
		try
		{
			ChatMessageResponse chatResponse = await _chatApiClient.StartConversationAsync(locale, conversationId);

			return chatResponse.Message;
		}
		catch (ApiException apiEx)
		{
			_logger.LogError(apiEx, "An error occurred while starting a new conversation");
			return string.Empty;
		}
	}

	private static CardAction MapToCardAction(SuggestedAction action)
	{
		if (action.ActionType == ActionType.OpenUrl)
		{
			return new CardAction()
			{
				Title = $"Website - {action.Text}",
				Type = ActionTypes.OpenUrl,
				Value = action.ActionData,
			};
		}
		else if (action.ActionType == ActionType.Map)
		{
			// Open Google Maps with the location.
			return new CardAction()
			{
				Title = $"Maps - {action.Text}",
				Type = ActionTypes.OpenUrl,
				Value = $"https://www.google.com/maps/search/?api=1&query={action.ActionData}",
			};
		}
		else
		{
			return new CardAction()
			{
				Title = action.Text,
				Type = ActionTypes.ImBack,
				Value = action.Text
			};
		}
	}
}

