namespace IRepository.Generic;

public interface IGaStagingRepository
{
    Task ExecuteStageGaRecordsAsync(int processedCount, CancellationToken stoppingToken);
}
