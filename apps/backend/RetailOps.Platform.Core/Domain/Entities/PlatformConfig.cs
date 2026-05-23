using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Platform.Core.Domain.Entities;

public sealed class PlatformConfig
{
    public int TrialDays { get; private set; }
    public int BlockDays { get; private set; }
    public string BlockMessage { get; private set; } = string.Empty;

    private PlatformConfig(int trialDays, int blockDays, string blockMessage)
    {
        TrialDays = trialDays;
        BlockDays = blockDays;
        BlockMessage = blockMessage;
    }

    public static Result<PlatformConfig> Create(int trialDays, int blockDays, string blockMessage)
    {
        if (trialDays < 0)
            return Result<PlatformConfig>.Failure("Trial days must be non-negative.");
        if (blockDays < 0)
            return Result<PlatformConfig>.Failure("Block days must be non-negative.");

        return Result<PlatformConfig>.Success(new PlatformConfig(
            trialDays,
            blockDays,
            string.IsNullOrWhiteSpace(blockMessage) ? "Conta suspensa por inadimplência." : blockMessage.Trim()));
    }
}
