using Metriflow.Application.Interfaces;
using Metriflow.Application.Models.Enums;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Services.Workers;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IListsKeysServices))]
public class ListsKeysServices : IListsKeysServices
{
    public HashSet<string> GetCompletedListsSharedKeys(
        Dictionary<enCompletedListsNames, IEnumerable<string>> completedListsKeysDic
    )
    {
        if (completedListsKeysDic == null || !completedListsKeysDic.Any())
            throw new ArgumentException(
                "Lists keys cannot be empty",
                nameof(completedListsKeysDic)
            );

        var first = completedListsKeysDic.First().Key;

        var keysSet = new HashSet<string>(completedListsKeysDic[first]);

        foreach (var kvp in completedListsKeysDic)
        {
            if (kvp.Key == first)
                continue;

            keysSet.IntersectWith(kvp.Value);
        }

        Console.WriteLine($"Keys Set: {keysSet.Count}");
        return keysSet;
    }

    public List<string> GetAllKeysWithPrefixes(string key)
    {
        var keys = new List<string>();
        foreach (var typeKey in Enum.GetNames(typeof(enTypesKey)))
            keys.Add($"{typeKey}|{key}");

        return keys;
    }
}