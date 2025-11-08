using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.Application.interfaces;

public interface IPageServices
{
    Task<Page> CreatePage(CombinedAnalyticsMessage combinedAnalyticsMessage);
}