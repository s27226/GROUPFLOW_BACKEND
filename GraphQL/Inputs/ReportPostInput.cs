namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public class ReportPostInput
{
    public int PostId { get; set; }
    public string Reason { get; set; } = null!;
}
