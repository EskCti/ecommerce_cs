using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Finance.Domain.ValueObjects;

public record Recurrence
{
    public int IntervalDays { get; }
    public string Label { get; }

    private Recurrence(int intervalDays, string label)
    {
        IntervalDays = intervalDays;
        Label = label;
    }

    public static Result<Recurrence> Create(int intervalDays, string? label = null)
    {
        if (intervalDays < 0)
            return Result<Recurrence>.Failure("Recurrence interval cannot be negative.");

        var resolvedLabel = string.IsNullOrWhiteSpace(label)
            ? intervalDays switch
            {
                0 => "Nenhuma",
                1 => "Diária",
                7 => "Semanal",
                30 => "Mensal",
                _ => $"{intervalDays} dias"
            }
            : label.Trim();

        return Result<Recurrence>.Success(new Recurrence(intervalDays, resolvedLabel));
    }

    public static Result<Recurrence> None() => Create(0);
}
