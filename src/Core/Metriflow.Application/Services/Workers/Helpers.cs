using System.Collections;
using System.Reflection;
using System.Text.Json;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.Services.Workers;
 
public static class HelpersObsolete
{
    // public static IEnumerable<Type> GetAllAnalyticRecordTypes()
    // {
    //     return typeof(IAnalyticRecord)
    //         .Assembly.GetTypes()
    //         .Where(t =>
    //             typeof(IAnalyticRecord).IsAssignableFrom(t)
    //             && t.GetCustomAttribute<AnalyticRecordAttribute>()?.Key != null
    //         );
    // }


    /// <summary>
    /// Deserialize records from Redis values into a dictionary mapping record types to lists of records.
    /// </summary>
    /// <param name="redisValueDictionary"><see cref="Dictionary{string, IEnumerable{RedisValue}}"/> TKey is the name of the redis list, and the value is the list of RedisValue objects. </param>
    /// <returns></returns>
    // public static Dictionary<Type, IList> RecordsDeserialization<T>(
    //     Dictionary<string, IEnumerable<T>> redisValueDictionary,
    //     IEnumerable<Type> analyticRecordTypes,
    //     ILogger logger
    // )
    // {
    //     Dictionary<string, IList> di = analyticRecordTypes
    //         .ToDictionary(
    //             t => t.GetCustomAttribute<AnalyticRecordAttribute>()!.Key,
    //             t =>
    //             {
    //                 var listType = typeof(List<>).MakeGenericType(t);
    //                 return (IList)Activator.CreateInstance(listType)!;
    //             }
    //         );
    //
    //     foreach (var (key, redisValues) in redisValueDictionary)
    //     {
    //         var prefix = key.Split('|')[0];
    //         if (!di.TryGetValue(prefix, out var list))
    //             continue;
    //         var elementType = list.GetType().GetGenericArguments()[0];
    //         foreach (var rv in redisValues)
    //         {
    //             if (rv.IsNullOrEmpty)
    //             {
    //                 logger.LogError($"Null value found for ID {key}.");
    //                 continue;
    //             }
    //             try
    //             {
    //                 var record = (IAnalyticRecord)JsonSerializer.Deserialize(rv!, elementType)!;
    //                 list.Add(record);
    //             }
    //             catch (JsonException ex)
    //             {
    //                 logger.LogError(ex, $"JSON deserialization error for ID {key}.");
    //             }
    //         }
    //     }
    //     var result = di.ToDictionary(
    //         kvp => kvp.Value.GetType().GetGenericArguments()[0],
    //         kvp => kvp.Value
    //     );
    //
    //     return result;
    // }

    // public static string ExtractId(RedisValue key)
    // {
    //     var s = key.ToString();
    //     var firstSeparatorIndex = -1;
    //     var separatorCount = 0;
    //
    //     for (int i = 0; i < s.Length; i++)
    //     {
    //         if (s[i] != '|')
    //             continue;
    //
    //         if (firstSeparatorIndex < 0)
    //             firstSeparatorIndex = i;
    //
    //         separatorCount++;
    //
    //         if (separatorCount > 3)
    //             break;
    //     }
    //
    //     return separatorCount >= 3 ? s[firstSeparatorIndex..] : s;
    // }
}



// Core.Application.Analytics