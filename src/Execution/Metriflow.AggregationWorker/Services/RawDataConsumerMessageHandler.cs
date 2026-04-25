using System.Threading.Channels;
using System.Xml.Schema;
using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;

namespace Metriflow.AggregationWorker.Services;

public interface IRecordBatchSaver<T>
    where T : class, IAnalyticRecord
{
    Task SaveBulkAsync(List<List<T>> batch, int totalCount);
}

// Adapter for GA Records
[ServiceRegistration(lifetime: ServiceLifetime.Scoped, typeof(IRecordBatchSaver<GARecord>))]
public class GaRecordSaver : IRecordBatchSaver<GARecord>
{
    private readonly IRawDataRepository _repository;

    public GaRecordSaver(IRawDataRepository repository) => _repository = repository;

    public Task SaveBulkAsync(List<List<GARecord>> batch, int totalCount) =>
        _repository.AddGaRecordsBulk(batch, totalCount);
}

// Adapter for PSI Records
[ServiceRegistration(lifetime: ServiceLifetime.Scoped, typeof(IRecordBatchSaver<PSIRecord>))]
public class PsiRecordSaver : IRecordBatchSaver<PSIRecord>
{
    private readonly IRawDataRepository _repository;

    public PsiRecordSaver(IRawDataRepository repository) => _repository = repository;

    public Task SaveBulkAsync(List<List<PSIRecord>> batch, int totalCount) =>
        _repository.AddPsiRecordsBulk(batch, totalCount);
}

/// <summary>
/// Default implementation of <see cref="IRawDataConsumerMessageHandler{T}"/>. Stores incoming records in Redis,
/// tracks the maximum received date, and triggers matching/combining logic when appropriate.
/// </summary>
public class RawDataConsumerMessageHandler<T>(
    ILogger<RawDataConsumerMessageHandler<T>> logger,
    IServiceScopeFactory scopeFactory
) : IRawDataConsumerMessageHandler<T>
    where T : class, IAnalyticRecord
{
    const int batchCount = 100_000;
    const int workersCount = 4;

    // int accumulator = 0;
    // List<List<T>> outerRecordsLst = new List<List<T>>();
    static readonly TimeSpan flushTimeout = TimeSpan.FromSeconds(120);

    public async Task HandleIncomingAnalyticsRecordsAsync(
        Channel<List<T>> channel,
        CancellationToken stoppingToken
    )
    {
        try
        {
            var workers = Enumerable
                .Range(0, workersCount)
                .Select(workerId =>
                    Task.Run(() => this.Process(channel, workerId, stoppingToken), stoppingToken)
                );

            await Task.WhenAll(workers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    private async Task Process(
        Channel<List<T>> channel,
        int workerId,
        CancellationToken stoppingToken
    )
    {
        var accumulator = 0;
        List<List<T>> outerRecordsLst = new List<List<T>>();

        logger.LogInformation($"Processing {nameof(T)}");
        logger.LogInformation(
            $"Thread: ${Thread.CurrentThread.Name}, Processing {nameof(T)}, IsBackground ${Thread.CurrentThread.IsBackground}"
        );
        try
        {
            var date = DateTime.UtcNow;
            var lastReceiveTime = DateTime.UtcNow;
            while (true)
            {
                var waitTask = channel.Reader.WaitToReadAsync(stoppingToken).AsTask();

                if (DateTime.UtcNow - lastReceiveTime >= flushTimeout)
                {
                    await Flush(outerRecordsLst, accumulator);
                    outerRecordsLst.Clear();
                    accumulator = 0;
                    continue;
                }

                // channel completed
                if (!await waitTask)
                    break;

                // ===== READ ALL AVAILABLE =====
                while (channel.Reader.TryRead(out var lst))
                {
                    outerRecordsLst.Add(lst);
                    accumulator += lst.Count;
                    lastReceiveTime = DateTime.UtcNow;

                    if (accumulator >= batchCount)
                    {
                        await Flush(outerRecordsLst, accumulator);
                        outerRecordsLst.Clear();
                        accumulator = 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing channel in Worker {WorkerId}", workerId);
            logger.LogError(ex, ex.Message);
        }
        finally
        {
            //should also read remains items in the channel but I'll skip it for now

            if (outerRecordsLst.Count > 0)
            {
                await Flush(outerRecordsLst, accumulator);
                outerRecordsLst.Clear();
                accumulator = 0;
            }
        }
    }

    async Task Flush(List<List<T>> outerRecordsLst, int accumulator)
    {
        logger.LogInformation(
            "@@@@@@ Flushing {Count} {RecordType} records to database...",
            accumulator,
            typeof(T).Name
        );

        using var scope = scopeFactory.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IRecordBatchSaver<T>>();

        await repo.SaveBulkAsync(outerRecordsLst, accumulator);
    }

    // private Task HandleIncomingGaRecordAsync2(
    //     Channel<List<GARecord>> channel,
    //     CancellationToken stoppingToken
    // )
    // {
    //     try
    //     {
    //         var workers = Enumerable
    //             .Range(0, 4)
    //             .Select(_ =>
    //                 Task.Run(
    //                     async () =>
    //                     {
    //                         // var gaAccumulator = 0;
    //                         // List<List<GARecord>> outerGaRecordsLst = new List<List<GARecord>>();

    //                         // logger.LogInformation($"Processing {nameof(GARecord)}");
    //                         // logger.LogInformation(
    //                         //     $"Thread: ${Thread.CurrentThread.Name}, Processing {nameof(GARecord)}, IsBackground ${Thread.CurrentThread.IsBackground}"
    //                         // );

    //                         // var Flush = async () =>
    //                         // {
    //                         //     logger.LogInformation(
    //                         //         "@@@@@ Start ServiceProvider For IRawDataRepository AddGaRecordsBulk"
    //                         //     );

    //                         //     using var scope = scopeFactory.CreateScope();

    //                         //     var repo =
    //                         //         scope.ServiceProvider.GetRequiredService<IRawDataRepository>();

    //                         //     await repo.AddGaRecordsBulk(outerGaRecordsLst, gaAccumulator);
    //                         //     outerGaRecordsLst.Clear();
    //                         //     gaAccumulator = 0;
    //                         // };
    //                         try
    //                         {
    //                             while (true)
    //                             {
    //                                 // var waitTask = channel
    //                                 //     .Reader.WaitToReadAsync(stoppingToken)
    //                                 //     .AsTask();
    //                                 // var delayTask = Task.Delay(flushTimeout, stoppingToken);

    //                                 // var completed = await Task.WhenAny(waitTask, delayTask);

    //                                 // var lst = await channel.Reader.ReadAsync(stoppingToken);

    //                                 // outerGaRecordsLst.Add(lst);
    //                                 // gaAccumulator += lst.Count;

    //                                 // if (completed == waitTask)
    //                                 // {
    //                                 //     await Flush();
    //                                 //     continue;
    //                                 // }

    //                                 // if (gaAccumulator >= batchCount)
    //                                 // {
    //                                 //     await Flush();
    //                                 //     continue;
    //                                 // }
    //                             }
    //                         }
    //                         catch (Exception ex)
    //                         {
    //                             // logger.LogError(ex, ex.Message);
    //                         }
    //                         finally
    //                         {
    //                             // if (outerGaRecordsLst.Count > 0)
    //                             //     await Flush();
    //                         }
    //                     },
    //                     stoppingToken
    //                 )
    //             );

    //         return Task.WhenAll(workers);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, ex.Message);
    //         throw;
    //     }
    // }

    private Task HandleIncomingGaRecordAsync(
        Channel<List<GARecord>> channel,
        CancellationToken stoppingToken
    )
    {
        try
        {
            var workers = Enumerable
                .Range(0, 4)
                .Select(_ =>
                    Task.Run(
                        async () =>
                        {
                            var gaAccumulator = 0;
                            List<List<GARecord>> outerGaRecordsLst = new List<List<GARecord>>();

                            logger.LogInformation($"Processing {nameof(GARecord)}");
                            logger.LogInformation(
                                $"Thread: ${Thread.CurrentThread.Name}, Processing {nameof(GARecord)}, IsBackground ${Thread.CurrentThread.IsBackground}"
                            );

                            var Flush = async () =>
                            {
                                logger.LogInformation(
                                    "@@@@@ Start ServiceProvider For IRawDataRepository AddGaRecordsBulk"
                                );

                                using var scope = scopeFactory.CreateScope();

                                var repo =
                                    scope.ServiceProvider.GetRequiredService<IRawDataRepository>();

                                await repo.AddGaRecordsBulk(outerGaRecordsLst, gaAccumulator);
                                outerGaRecordsLst.Clear();
                                gaAccumulator = 0;
                            };
                            try
                            {
                                while (true)
                                {
                                    var waitTask = channel
                                        .Reader.WaitToReadAsync(stoppingToken)
                                        .AsTask();
                                    var delayTask = Task.Delay(flushTimeout, stoppingToken);

                                    var completed = await Task.WhenAny(waitTask, delayTask);

                                    var lst = await channel.Reader.ReadAsync(stoppingToken);

                                    outerGaRecordsLst.Add(lst);
                                    gaAccumulator += lst.Count;

                                    if (completed == waitTask)
                                    {
                                        await Flush();
                                        continue;
                                    }

                                    if (gaAccumulator >= batchCount)
                                    {
                                        await Flush();
                                        continue;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, ex.Message);
                            }
                            finally
                            {
                                if (outerGaRecordsLst.Count > 0)
                                    await Flush();
                            }
                        },
                        stoppingToken
                    )
                );

            return Task.WhenAll(workers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private Task HandleIncomingPsiRecordAsync(
        Channel<List<PSIRecord>> channel,
        CancellationToken stoppingToken
    )
    {
        try
        {
            var workers = Enumerable
                .Range(0, 4)
                .Select(_ =>
                    Task.Run(
                        async () =>
                        {
                            logger.LogInformation($"Processing {nameof(PSIRecord)}");
                            logger.LogInformation(
                                $"Thread: ${Thread.CurrentThread.Name}, Processing {nameof(PSIRecord)}, IsBackground ${Thread.CurrentThread.IsBackground}"
                            );

                            const int outerListCapacity = 10;
                            var psiAccumulator = 0;
                            List<List<PSIRecord>> outerPsiRecordsLst = new List<List<PSIRecord>>();

                            var Flush = async () =>
                            {
                                logger.LogInformation(
                                    "@@@@@ Start ServiceProvider For IRawDataRepository AddPsiRecordsBulk"
                                );
                                using var scope = scopeFactory.CreateScope();

                                var repo =
                                    scope.ServiceProvider.GetRequiredService<IRawDataRepository>();

                                await repo.AddPsiRecordsBulk(outerPsiRecordsLst, psiAccumulator);
                                outerPsiRecordsLst.Clear();

                                psiAccumulator = 0;
                            };
                            try
                            {
                                while (true)
                                {
                                    var waitTask = channel
                                        .Reader.WaitToReadAsync(stoppingToken)
                                        .AsTask();
                                    var delayTask = Task.Delay(flushTimeout, stoppingToken);

                                    var completed = await Task.WhenAny(waitTask, delayTask);

                                    var lst = await channel.Reader.ReadAsync(stoppingToken);

                                    outerPsiRecordsLst.Add(lst);
                                    psiAccumulator += lst.Count;

                                    outerPsiRecordsLst.Add(lst);
                                    psiAccumulator += lst.Count;

                                    if (completed == waitTask)
                                    {
                                        await Flush();
                                        continue;
                                    }
                                    if (psiAccumulator >= batchCount)
                                    {
                                        await Flush();
                                        continue;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, ex.Message);
                            }
                            finally
                            {
                                if (outerPsiRecordsLst.Count > 0)
                                    await Flush();
                            }
                            stoppingToken.ThrowIfCancellationRequested();
                        },
                        stoppingToken
                    )
                );

            return Task.WhenAll(workers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
