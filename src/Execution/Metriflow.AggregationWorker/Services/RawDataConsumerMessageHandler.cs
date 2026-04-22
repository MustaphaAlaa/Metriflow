using System.Threading.Channels;
using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;

namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Default implementation of <see cref="IRawDataConsumerMessageHandler{T}"/>. Stores incoming records in Redis,
/// tracks the maximum received date, and triggers matching/combining logic when appropriate.
/// </summary>
public class RawDataConsumerMessageHandler<T>(
    ILogger<RawDataConsumerMessageHandler<T>> logger,
    IServiceScopeFactory scopeFactory
)
    : IRawDataConsumerMessageHandler<T> where T : class, IAnalyticRecord
{
    const int batchCount = 250000;

    public Task HandleIncomingGaRecordAsync(Channel<List<GARecord>> channel, CancellationToken stoppingToken)
    {
        try
        {
            var workers = Enumerable.Range(0, 4).Select(_ => Task.Run(async () =>
            {
                logger.LogInformation($"Processing {nameof(GARecord)}");
                logger.LogInformation(
                    $"Thread: ${Thread.CurrentThread.Name}, Processing {nameof(GARecord)}, IsBackground ${Thread.CurrentThread.IsBackground}");


                var gaAccumulator = 0;
                List<List<GARecord>> outerGaRecordsLst = new List<List<GARecord>>();

                while (await channel.Reader.WaitToReadAsync(stoppingToken))
                {
                    var lst = await channel.Reader.ReadAsync(stoppingToken);

                    outerGaRecordsLst.Add(lst);
                    gaAccumulator += lst.Count;


                    if (gaAccumulator >= batchCount)
                    {
                        logger.LogInformation("@@@@@ Start ServiceProvider For IRawDataRepository AddGaRecordsBulk");

                        using var scope = scopeFactory.CreateScope();

                        var repo = scope.ServiceProvider
                            .GetRequiredService<IRawDataRepository>();

                        await repo.AddGaRecordsBulk(outerGaRecordsLst, gaAccumulator);
                        outerGaRecordsLst.Clear();
                        gaAccumulator = 0;
                    }
                }

                if (outerGaRecordsLst.Count > 0)
                {
                    logger.LogInformation(
                        "@@@@@ Start ServiceProvider For IRawDataRepository AddGaRecordsBulk {{after while}}");

                    using var scope = scopeFactory.CreateScope();

                    var repo = scope.ServiceProvider
                        .GetRequiredService<IRawDataRepository>();

                    await repo.AddGaRecordsBulk(outerGaRecordsLst, gaAccumulator);
                    outerGaRecordsLst.Clear();
                    gaAccumulator = 0;
                }

                stoppingToken.ThrowIfCancellationRequested();
            }, stoppingToken));

            return Task.WhenAll(workers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public Task HandleIncomingPsiRecordAsync(Channel<List<PSIRecord>> channel, CancellationToken stoppingToken)
    {
        try
        {
            var workers = Enumerable.Range(0, 4).Select(_ => Task.Run(async () =>
            {
                logger.LogInformation($"Processing {nameof(PSIRecord)}");
                logger.LogInformation(
                    $"Thread: ${Thread.CurrentThread.Name}, Processing {nameof(PSIRecord)}, IsBackground ${Thread.CurrentThread.IsBackground}");


                const int outerListCapacity = 10;
                var psiAccumulator = 0;
                List<List<PSIRecord>> outerPsiRecordsLst = new List<List<PSIRecord>>();


                while (await channel.Reader.WaitToReadAsync(stoppingToken))
                {
                    var lst = await channel.Reader.ReadAsync(stoppingToken);

                    outerPsiRecordsLst.Add(lst);
                    psiAccumulator += lst.Count;

                    if (psiAccumulator >= batchCount)
                    {
                        logger.LogInformation("@@@@@ Start ServiceProvider For IRawDataRepository AddPsiRecordsBulk");
                        using var scope = scopeFactory.CreateScope();

                        var repo = scope.ServiceProvider
                            .GetRequiredService<IRawDataRepository>();

                        await repo.AddPsiRecordsBulk(outerPsiRecordsLst, psiAccumulator);
                        outerPsiRecordsLst.Clear();

                        psiAccumulator = 0;
                    }
                }

                if (outerPsiRecordsLst.Count > 0)
                {
                    logger.LogInformation(
                        "@@@@@ Start ServiceProvider For IRawDataRepository AddPsiRecordsBulk {{after while}}");

                    using var scope = scopeFactory.CreateScope();

                    var repo = scope.ServiceProvider
                        .GetRequiredService<IRawDataRepository>();

                    await repo.AddPsiRecordsBulk(outerPsiRecordsLst, psiAccumulator);
                    outerPsiRecordsLst.Clear();
                    psiAccumulator = 0;
                }

                stoppingToken.ThrowIfCancellationRequested();
            }, stoppingToken));

            return Task.WhenAll(workers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }
}