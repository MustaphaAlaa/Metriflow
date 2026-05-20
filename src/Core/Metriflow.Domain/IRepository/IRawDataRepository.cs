using Metriflow.Domain.Entities.Workers;

namespace IRepository.Generic;

public interface IRawDataRepository :
    IRawDataStagingRepository,
    IGaRecordRepository,
    IPsaRecordRepository,
    IPageAnalyticsCorrelationRepository
{
}
