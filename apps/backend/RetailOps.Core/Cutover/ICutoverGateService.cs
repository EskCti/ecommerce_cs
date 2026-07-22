namespace RetailOps.Core.Cutover;

public interface ICutoverGateService
{
    Task<CutoverChecklistResult> RunPreCutoverChecklistAsync(CancellationToken ct = default);
}
