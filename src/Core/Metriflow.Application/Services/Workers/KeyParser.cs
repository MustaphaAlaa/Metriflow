using Metriflow.Application.Interfaces;

namespace Metriflow.Application.Services.Workers;

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
