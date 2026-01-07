using Metriflow.Domain.Entities;

namespace Metriflow.Domain.Entities;

public class Page
{
    public int Id { get; set; }

    // The actual URL path
    public string Path { get; set; } 
}