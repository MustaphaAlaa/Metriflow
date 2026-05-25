using Metriflow.Application.Interfaces;
using Metriflow.Application.Services;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.UnitTests.Services;

public class PageAnalyticsServicesTests
{
    private readonly Mock<ILogger<PageAnalyticsServices>> _mockLogger;
    private readonly IPageAnalyticsServices _pageAnalyticsServices;
    private readonly Fixture _fixture;

    public PageAnalyticsServicesTests()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<PageAnalyticsServices>>();
        _pageAnalyticsServices = new PageAnalyticsServices(_mockLogger.Object);
    }

    [Fact]
    public async Task NormalizeRawData_WithValidMessage_ReturnsPageAnalytics()
    {
        // Arrange
        var message = _fixture.Create<CombinedAnalyticsMessage>();
        var page = _fixture.Create<Page>();

        // Act
        var result = await _pageAnalyticsServices.NormalizeRawData(message, page);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(message.Page);
        result.LcpMs.Should().Be(message.LcpMs);
        result.PerformanceScore.Should().Be(message.PerformanceScore);
        result.Users.Should().Be(message.Users);
        result.Sessions.Should().Be(message.Sessions);
        result.Views.Should().Be(message.Views);
    }

    [Fact]
    public async Task NormalizeRawData_WithValidTicks_CreateCorrectDate()
    {
        // Arrange
        var expectedDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var message = _fixture.Build<CombinedAnalyticsMessage>()
            .With(x => x.Ticks, expectedDate.Ticks)
            .Create();
        var page = _fixture.Create<Page>();

        // Act
        var result = await _pageAnalyticsServices.NormalizeRawData(message, page);

        // Assert
        result.Date.Should().Be(expectedDate);
    }

    [Fact]
    public async Task NormalizeRawData_SetsTimeInterval()
    {
        // Arrange
        var message = _fixture.Create<CombinedAnalyticsMessage>();
        var page = _fixture.Create<Page>();

        // Act
        var result = await _pageAnalyticsServices.NormalizeRawData(message, page);

        // Assert
        result.Interval.Should().NotBe(default(byte));
    }

    [Fact]
    public void RecordsToPageAnalytics_WithValidRecords_ReturnsListOfPageAnalytics()
    {
        // Arrange
        var aggregateRecords = _fixture.CreateMany<AggregateRecordsJoins>(3).ToList();

        // Act
        var result = _pageAnalyticsServices.RecordsToPageAnalytics(aggregateRecords);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.All(x => x != null).Should().BeTrue();
    }

    [Fact]
    public void RecordsToPageAnalytics_MapsAllProperties()
    {
        // Arrange
        var aggregateRecord = _fixture.Create<AggregateRecordsJoins>();
        var records = new List<AggregateRecordsJoins> { aggregateRecord };

        // Act
        var result = _pageAnalyticsServices.RecordsToPageAnalytics(records);

        // Assert
        result.Should().HaveCount(1);
        var pageAnalytic = result.First();
        pageAnalytic.PageId.Should().Be(aggregateRecord.PageId);
        pageAnalytic.LcpMs.Should().Be(aggregateRecord.PsaRecord.LCP_MS);
        pageAnalytic.PerformanceScore.Should().Be(aggregateRecord.PsaRecord.PerformanceScore);
        pageAnalytic.Users.Should().Be(aggregateRecord.GARecord.Users);
        pageAnalytic.Sessions.Should().Be(aggregateRecord.GARecord.Sessions);
        pageAnalytic.Views.Should().Be(aggregateRecord.GARecord.Views);
    }

    [Fact]
    public void RecordsToPageAnalytics_WithEmptyRecords_ReturnsEmptyList()
    {
        // Arrange
        var emptyRecords = new List<AggregateRecordsJoins>();

        // Act
        var result = _pageAnalyticsServices.RecordsToPageAnalytics(emptyRecords);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void RecordsToPageAnalytics_LogsInformation()
    {
        // Arrange
        var records = _fixture.CreateMany<AggregateRecordsJoins>(2).ToList();

        // Act
        _pageAnalyticsServices.RecordsToPageAnalytics(records);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void RecordsToPageAnalytics_WithMultipleRecords_AllHaveValidIntervals()
    {
        // Arrange
        var records = _fixture.CreateMany<AggregateRecordsJoins>(5).ToList();

        // Act
        var result = _pageAnalyticsServices.RecordsToPageAnalytics(records);

        // Assert
        result.All(x => x.Interval >= 0 && x.Interval < 24).Should().BeTrue();
    }
}
