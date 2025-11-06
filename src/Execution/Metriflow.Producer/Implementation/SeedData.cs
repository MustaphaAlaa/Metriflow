using System.Text.Json;
using Metriflow.Producers.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Metriflow.Producers.Implementation;

/// <summary>
/// Implements the data seeding functionality for analytics records.
/// Loads mock data from JSON files for both GA and PSI records.
/// </summary>
public class SeedData : ISeedData
{
    private readonly IHostEnvironment _environment;

    /// <inheritdoc/>
    public List<GARecord> GARecords { get; private set; }

    /// <inheritdoc/>
    public List<PSIRecord> PSIRecords { get; private set; }

    /// <summary>
    /// Initializes a new instance of the SeedData class and loads mock data.
    /// </summary>
    /// <param name="environment">The host environment for accessing content root path.</param>
    public SeedData(IHostEnvironment environment)
    {
        _environment = environment;
        this.SeedingData().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Reads and deserializes data from a JSON file.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON data into.</typeparam>
    /// <param name="filename">The name of the JSON file to read.</param>
    /// <returns>A list of deserialized objects.</returns>
    private async Task<List<T>> GetDataFromJSONAsync<T>(string filename)
    {
        var filePath = Path.Combine(_environment.ContentRootPath, "data", filename);

        // Read files
        var gaJson = await File.ReadAllTextAsync(filePath);

        // Deserialize
        var data = JsonSerializer.Deserialize<List<T>>(gaJson);

        return data;
    }

    /// <summary>
    /// Initializes both GA and PSI records from their respective JSON files.
    /// </summary>
    private async Task SeedingData()
    {
        GARecords = await GetDataFromJSONAsync<GARecord>("GA-mock.json");
        PSIRecords = await GetDataFromJSONAsync<PSIRecord>("PSI-mock.json");
    }
}
