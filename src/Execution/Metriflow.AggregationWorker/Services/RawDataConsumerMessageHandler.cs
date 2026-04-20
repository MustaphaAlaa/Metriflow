using System.Runtime.CompilerServices;
using IRepository;
using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Default implementation of <see cref="IRawDataConsumerMessageHandler{T}"/>. Stores incoming records in Redis,
/// tracks the maximum received date, and triggers matching/combining logic when appropriate.
/// </summary>
public class RawDataConsumerMessageHandler<T>(
    ILogger<RawDataConsumerMessageHandler<T>> logger,
    IOptions<RabbitMqSettings> options,
    IAggregationProgressRepository aggregationProgressRepository,
    IRawDataRepository rawDataRepository,
    IUow unitOfWork,
    IBaseRepository<T> repository
)
    : IRawDataConsumerMessageHandler<T> where T : class, IAnalyticRecord
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;

    /// <inheritdoc />
    public async Task HandleIncomingRecordAsync(enTypesKey type, List<T> records)

    {
        if (records is null || records.Count == 0)
        {
            logger.LogDebug("HandleIncomingRecordAsync called with no items; nothing to handle.");
            return;
        }

        try
        {
            logger.LogInformation(
                $"@@@@@@RawDataConsumerMessageHandler<{type}>Start Handling incoming Request, for type: {type}"
            );
            await unitOfWork.BeginTransactionAsync();
           
            if (type is enTypesKey.GA)
                await rawDataRepository.AddGaRecordsBulk(records.Cast<GARecord>());
            else
                await rawDataRepository.AddPsiRecordsBulk(records.Cast<PSIRecord>());

            await unitOfWork.CommitTransactionAsync();

            logger.LogInformation($"@@@@@@@{type}records have been created.");
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {

            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "@@@@@@@@@@@@@@@@@@@@@@Duplicate error");

        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "@@@@@@@@@@@@@@@@@@@@@@@@Error handling incoming records.");
            throw;
        }
    }
}