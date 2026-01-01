using Metriflow.Application.Models.Enums;

namespace Metriflow.Application.Interfaces;

public interface IListsKeysServices
{
    List<string> GetAllKeysWithPrefixes(string key);
    HashSet<string> GetCompletedListsSharedKeys(
        Dictionary<enCompletedListsNames, IEnumerable<string>> completedListsKeysDic
    );
}
