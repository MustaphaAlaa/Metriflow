namespace Metriflow.Domain.CustomAttributes;

[AttributeUsage(AttributeTargets.Class)]
public class AnalyticRecordAttribute : Attribute
{
    public string Key { get; }

    public AnalyticRecordAttribute(string key)
    {
        this.Key = key;
    }
}
