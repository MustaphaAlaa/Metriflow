using System.Collections;

namespace Metriflow.Application.Interfaces;

public interface IAnalyticRecordsDeserializer
{
    Dictionary<Type, IList> Deserialize(
        Dictionary<string, IEnumerable<byte[]>> redisData);
}