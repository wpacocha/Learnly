namespace Learnly.Application.Lessons.Dtos;

public sealed record LessonRoomStateDto(
    IReadOnlyList<LessonMessageDto> Messages,
    IReadOnlyList<WhiteboardEventDto> WhiteboardEvents);
