using System.ComponentModel.DataAnnotations;

namespace GROUPFLOW.Features.Chat.Inputs;

public record EntryInput(
    [property: Range(1, int.MaxValue)]
    int UserChatId,

    [property: Required]
    [property: StringLength(500, MinimumLength = 1)]
    string Message,

    bool Public
);
