using Metriflow.Domain.enums;

namespace Metriflow.Application.Services;

public static class BatchingUtilities
{
    public static int ResolveRawDataBatchSize(int processedCount) =>
        processedCount > (int)enBatchSizes.RawDataBaseBatch
            ? processedCount + 100000
            : (int)enBatchSizes.RawDataBaseBatch;
}