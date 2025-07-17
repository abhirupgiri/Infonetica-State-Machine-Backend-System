namespace back.Models
{
    public record State(
        string Id,
        bool IsInitial= false,
        bool IsFinal= false,
        bool Enabled= true
    );
    public record Action(
        string Id,
        List<string> FromStates,
        string ToState,
        bool Enabled= true
    );
    public record WorkflowDefinition(
        string Id,
        List<State> States,
        List<Action> Actions
    );
    public record WorkflowInstance(
        Guid Id,
        string DefinitionId,
        string CurrentStateId,
        List<HistoryEntry> History
    );
    public record HistoryEntry(
        string ActionID,
        DateTime Timestamp
    );
    public record ExecuteActionRequest(string ActionId);
}