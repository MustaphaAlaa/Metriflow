namespace Metriflow.Application.Interfaces;

public interface ITimeIntervalsOrchestration
{
    Task<int> AggregateTimeIntervalsAsync();
}