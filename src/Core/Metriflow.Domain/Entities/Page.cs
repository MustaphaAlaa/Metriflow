using System.ComponentModel.DataAnnotations;
using Metriflow.Domain.Entities;
using Metriflow.Domain.enums;

namespace Metriflow.Domain.Entities;

public class Page
{
    [Key]
    public int Id { get; set; }

    // The actual URL path
    public enPages Path { get; set; } 
}