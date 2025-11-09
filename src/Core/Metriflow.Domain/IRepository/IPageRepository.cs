using System.Linq.Expressions;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace IRepository.Generic;

public interface IPageRepository : IBaseRepository<Page>
{
    Task<List<PageReportDto>> PageReportAsync();
}
