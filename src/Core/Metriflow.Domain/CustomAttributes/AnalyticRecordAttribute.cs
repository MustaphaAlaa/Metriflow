using Metriflow.Domain.Entities.Enums;

namespace Metriflow.Domain.CustomAttributes;

[AttributeUsage(AttributeTargets.Class)]
public class AnalyticRecordAttribute : Attribute
{
    public string Key { get; }
    public enTypesKey EnumKey { get; }

    // Constructor that takes string (for backward compatibility)
    public AnalyticRecordAttribute(string key)
    {
        this.Key = key;
    }

    public AnalyticRecordAttribute(enTypesKey enumKey)
    {
        this.EnumKey = enumKey;
        this.Key = enumKey.ToString();
    }
}
