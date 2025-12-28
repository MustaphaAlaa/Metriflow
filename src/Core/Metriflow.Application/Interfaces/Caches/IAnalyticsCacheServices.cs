using Metriflow.Application.Models.Enums;

namespace Metriflow.Application.Interfaces.Caches;

public interface IAnalyticsCacheServices
{
    Task<Dictionary<string, IEnumerable<byte[]>>> ExecutePopTransactionAsync(
        IEnumerable<string> listsKeys,
        int expectedListLength = 24
    );
    Task<Dictionary<enCompletedListsNames, IEnumerable<string>>> GetCompletedListsKeys();
    Task<bool> RemoveKeysFromCompletedLists(IEnumerable<string> listsKeys);
}