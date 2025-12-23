namespace Metriflow.Correlation.Worker.Interfaces;

public interface IRecordsMatcher
{
    Task MatchRecords(
        Dictionary<enRedisCompletedListsNames, IEnumerable<string>> listsKeys,
        string[] listsPrefixes
    );
}
