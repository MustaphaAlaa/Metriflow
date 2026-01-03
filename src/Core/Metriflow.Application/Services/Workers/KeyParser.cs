using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Services.Workers;
[ServiceRegistration(ServiceLifetime.Scoped, typeof(IKeyParser))]

public class KeyParser : IKeyParser
{
    public string ExtractId(string key)
    {
        var separatorCount = 0;
        var firstSeparatorIndex = -1;

        for (var i = 0; i < key.Length; i++)
        {
            if (key[i] != '|')
                continue;

            if (firstSeparatorIndex < 0)
                firstSeparatorIndex = i;

            if (++separatorCount >= 3)
                break;
        }

        return separatorCount >= 3 ? key[firstSeparatorIndex..] : key;
    }
}
