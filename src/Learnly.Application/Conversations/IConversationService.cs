using Learnly.Application.Conversations.Dtos;

namespace Learnly.Application.Conversations;

public interface IConversationService
{
    Task<IReadOnlyList<ConversationDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<(ConversationDto? conversation, string? error)> StartAsync(
        StartConversationRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ConversationMessageDto>? messages, string? error)> GetMessagesAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<(ConversationMessageDto? message, string? error)> SendMessageAsync(
        Guid conversationId,
        SendConversationMessageRequestDto dto,
        CancellationToken cancellationToken = default);
}
