namespace IRepository.Generic;

public interface IPsaStagingRepository
{
    Task ExecuteStagePsaRecordsAsync(int processedCount, CancellationToken stoppingToken);
}
