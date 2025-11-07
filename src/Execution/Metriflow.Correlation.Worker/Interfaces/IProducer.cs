using System.Collections.Generic;
using System.Threading.Tasks;
using Metriflow.Domain;

namespace Metriflow.Correlation.Worker.Interfaces;

/// <summary>
/// Produces rows (raw analytic records) to an external messaging system.
/// </summary>
public interface IRowRecordProducer
{
    /// <summary>
    /// Publish a collection of <see cref="RawRecord"/> objects to the configured destination.
    /// </summary>
    /// <param name="rawRecords">The list of raw records to publish.</param>
    Task PublishRawRecord(List<RawRecord> rawRecords);
}
