namespace RetailOps.Core.Cutover;

public sealed record CutoverCheckItem(string Name, bool Passed, string Detail);

public sealed record CutoverChecklistResult(
    bool CanProceed,
    IReadOnlyList<CutoverCheckItem> Items);
