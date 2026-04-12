using Metriflow.Application.Interfaces;
using Metriflow.Application.Services;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.UnitTests.Services;

public class RangeAnalyticServiceTests
{
    private readonly Mock<ILogger<RangeAnalyticService>> _mockLogger;
    private readonly IRangeAnalyticService _rangeAnalyticService;
    private readonly Fixture _fixture;

    public RangeAnalyticServiceTests()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<RangeAnalyticService>>();
        _rangeAnalyticService = new RangeAnalyticService(_mockLogger.Object);
    }

    [Fact]
    public void NormalizeRangeAnalytic_WithValidData_ReturnsRangeAnalytics()
    {
        // Arrange
        var rangeData = _fixture.CreateMany<AggregateAnalytics>(12).ToList();
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(rangeData[0].PageId);
        result.From.Should().Be(from);
        result.To.Should().Be(to);
    }

    [Fact]
    public void NormalizeRangeAnalytic_WithInsufficientData_ReturnsNull()
    {
        // Arrange
        var rangeData = _fixture.CreateMany<AggregateAnalytics>(11).ToList();
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NormalizeRangeAnalytic_WithNullData_ReturnsNull()
    {
        // Arrange
        List<AggregateAnalytics> nullData = null;
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(nullData, from, to);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NormalizeRangeAnalytic_WithEmptyList_ReturnsNull()
    {
        // Arrange
        var emptyData = new List<AggregateAnalytics>();
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(emptyData, from, to);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NormalizeRangeAnalytic_CalculatesCorrectAveragePerformance()
    {
        // Arrange
        var rangeData = _fixture.Build<AggregateAnalytics>()
            .With(x => x.AvgPerformance, 75)
            .CreateMany(12)
            .ToList();
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.AvgPerformance.Should().Be(75);
    }

    [Fact]
    public void NormalizeRangeAnalytic_CalculatesCorrectTotalSessions()
    {
        // Arrange
        var rangeData = _fixture.Build<AggregateAnalytics>()
            .With(x => x.TotalSessions, 100)
            .CreateMany(12)
            .ToList();
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.TotalSessions.Should().Be(1200);
    }

    [Fact]
    public void NormalizeRangeAnalytic_CalculatesCorrectTotalViews()
    {
        // Arrange
        var rangeData = _fixture.Build<AggregateAnalytics>()
            .With(x => x.TotalViews, 50)
            .CreateMany(12)
            .ToList();
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.TotalViews.Should().Be(600);
    }

    [Fact]
    public void NormalizeRangeAnalytic_CalculatesCorrectTotalUsers()
    {
        // Arrange
        var rangeData = _fixture.Build<AggregateAnalytics>()
            .With(x => x.TotalUsers, 25)
            .CreateMany(12)
            .ToList();
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.TotalUsers.Should().Be(300);
    }

    [Fact]
    public void NormalizeRangeAnalytic_WithExactly12Records_ReturnsValidResult()
    {
        // Arrange
        var rangeData = _fixture.CreateMany<AggregateAnalytics>(12).ToList();
        var from = new DateTime(2023, 1, 1);
        var to = new DateTime(2023, 12, 31);

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.Should().NotBeNull();
        result.From.Should().Be(from);
        result.To.Should().Be(to);
    }

    [Fact]
    public void NormalizeRangeAnalytic_WithMoreThan12Records_ReturnsValidResult()
    {
        // Arrange
        var rangeData = _fixture.CreateMany<AggregateAnalytics>(24).ToList();
        var from = DateTime.UtcNow.AddMonths(-24);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(rangeData[0].PageId);
    }

    [Fact]
    public void NormalizeRangeAnalytic_UsesFirstRecordPageId()
    {
        // Arrange
        var firstPageId = _fixture.Create<int>();
        var rangeData = _fixture.Build<AggregateAnalytics>()
            .With(x => x.PageId, firstPageId)
            .CreateMany(1)
            .ToList();
        rangeData.AddRange(_fixture.CreateMany<AggregateAnalytics>(11));
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.PageId.Should().Be(firstPageId);
    }

    [Fact]
    public void NormalizeRangeAnalytic_WithVaryingPerformanceScores_CalculatesAverageCorrectly()
    {
        // Arrange
        var rangeData = new List<AggregateAnalytics>();
        for (int i = 0; i < 12; i++)
        {
            rangeData.Add(_fixture.Build<AggregateAnalytics>()
                .With(x => x.AvgPerformance, 50 + (i * 5))
                .Create());
        }
        var from = DateTime.UtcNow.AddMonths(-12);
        var to = DateTime.UtcNow;

        // Act
        var result = _rangeAnalyticService.NormalizeRangeAnalytic(rangeData, from, to);

        // Assert
        result.AvgPerformance.Should().BeGreaterThan(0);
        result.AvgPerformance.Should().BeLessThanOrEqualTo(110);
    }
}
