using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Services;

public class RawDataIngestionOrchestrator : IRawDataIngestionOrchestrator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPageServices _pageServices;
    private readonly IRawDataServices _rawDataServices;
    private readonly ILogger<RawDataIngestionOrchestrator> _logger;
    private readonly IPageRepository _pageRepository;
    private readonly IBaseRepository<RawData> _rawDataRepository;

    public RawDataIngestionOrchestrator(
        IUnitOfWork unitOfWork,
        IPageServices pageServices,
        IRawDataServices rawDataServices,
        ILogger<RawDataIngestionOrchestrator> logger,
        IPageRepository pageRepository,
        IBaseRepository<RawData> rawDataRepository
    )
    {
        _unitOfWork = unitOfWork;
        _pageServices = pageServices;
        _rawDataServices = rawDataServices;
        _logger = logger;
        _pageRepository = pageRepository;
        _rawDataRepository = rawDataRepository;
    }

    public async Task Ingest(List<CombinedAnalyticsMessage> combinedAnalyticsMessages)
    {
        await this.ExecuteTransactionAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            foreach (var msg in combinedAnalyticsMessages)
            {
                var normalizedPage = await _pageServices.NormalizePage(msg);
                var page = await _pageRepository.GetOrCreatePageAsync(normalizedPage);
                var normalizedRawData = await _rawDataServices.NormalizeRawData(msg, page);
                var rawData = await _rawDataRepository.CreateAsync(normalizedRawData);
                _logger.LogInformation(
                    $"Creating RawData Id => {normalizedRawData.Id} =>> {normalizedRawData.Date}"
                );
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        });
    }

    private async Task ExecuteTransactionAsync(Func<Task> action)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            await action();
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(e, "Raw data ingestion failed during transaction.");
            throw;
        }
    }
}
