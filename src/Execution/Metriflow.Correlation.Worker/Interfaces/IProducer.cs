using System.Collections.Generic;
using System.Threading.Tasks;
using Metriflow.Domain;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.Correlation.Worker.Interfaces;

/// <summary>
/// Produces rows (raw analytic records) to an external messaging system.
/// </summary>
public interface IRowDataProducer
{
    /// <summary>
    /// Publish a collection of <see cref="RawData"/> objects to the configured destination.
    /// </summary>
    /// <param name="rawRecords">The list of raw records to publish.</param>
    Task PublishRawRecord(IEnumerable<CombinedAnalyticsMessage> combineAnalyticsMessages);
}
