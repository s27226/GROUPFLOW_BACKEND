namespace NAME_WIP_BACKEND.GraphQL.Responses;

public record PostReportResponse(
    int Id,
    int PostId,
    int ReporterId,
    string Reason,
    DateTime ReportedAt
);