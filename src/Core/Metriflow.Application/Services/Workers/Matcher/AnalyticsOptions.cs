namespace Metriflow.Application.Services.Workers;

public sealed class AnalyticsOptions
{
    public string ExchangeName { get; init; } = default!;
    public int HoursPerDay { get; init; }
}