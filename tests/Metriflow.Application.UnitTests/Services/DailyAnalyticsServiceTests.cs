using Metriflow.Application.Interfaces;
using Metriflow.Application.Services;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.UnitTests.Services;

public class DailyAnalyticsServiceTests
{
    private readonly Mock<ILogger<DailyAnalyticsService>> _mockLogger;
    private readonly IDailyAnalyticsService _dailyAnalyticsService;
    private readonly Fixture _fixture;

    public DailyAnalyticsServiceTests()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<DailyAnalyticsService>>();
        _dailyAnalyticsService = new DailyAnalyticsService(_mockLogger.Object);
    }

    [Fact]
    public async Task CalculateDailyStat_WithValidPages_ReturnsValidDailyAnalytics()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Date, now)
            .CreateMany(5)
            .ToList();

        // Act
        var result = await _dailyAnalyticsService.CalculateDailyStat(pageAnalytics);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(now.Date);
        result.PageId.Should().Be(pageAnalytics[0].PageId);
        result.ReceivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CalculateDailyStat_WithEmptyPages_ThrowsNullReferenceException()
    {
        // Arrange
        var emptyPages = new List<PageAnalytics>();

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _dailyAnalyticsService.CalculateDailyStat(emptyPages));
    }

    [Fact]
    public async Task CalculateDailyStat_WithNullInList_ThrowsNullReferenceException()
    {
        // Arrange
        var pages = new List<PageAnalytics> { null, _fixture.Create<PageAnalytics>() };

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _dailyAnalyticsService.CalculateDailyStat(pages));
    }

    [Fact]
    public async Task CalculateDailyStat_WithSinglePage_ReturnsValidDailyAnalytics()
    {
        // Arrange
        var pageAnalytic = _fixture.Create<PageAnalytics>();
        var pages = new List<PageAnalytics> { pageAnalytic };

        // Act
        var result = await _dailyAnalyticsService.CalculateDailyStat(pages);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(pageAnalytic.Date.Date);
        result.PageId.Should().Be(pageAnalytic.PageId);
    }

    [Fact]
    public async Task CalculateDailyStat_VerifiesAggregation()
    {
        // Arrange
        var pageAnalytics = _fixture.CreateMany<PageAnalytics>(3).ToList();

        // Act
        var result = await _dailyAnalyticsService.CalculateDailyStat(pageAnalytics);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().NotBe(default(int));
    }
}
