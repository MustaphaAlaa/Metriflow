using Metriflow.Application.Interfaces.Workers;
using Metriflow.Application.Models.Enums;

namespace Metriflow.Application.Interfaces;

public interface IRecordsMatcher
{
    Task MatchRecords(
        Dictionary<enCompletedListsNames, IEnumerable<string>> listsKeys,
        string[] listsPrefixes
    );
}