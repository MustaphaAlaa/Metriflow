using System.Runtime.InteropServices.JavaScript;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Services;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.UnitTests.Services;

public class TimeIntervalAnalyticServiceTests
{
    private readonly Mock<ILogger<TimeIntervalAnalyticService>> _mockLogger;
    private readonly ITimeIntervalAnalyticService _timeIntervalAnalyticService;
    private readonly Fixture _fixture;

    public TimeIntervalAnalyticServiceTests()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<TimeIntervalAnalyticService>>();
        _timeIntervalAnalyticService = new TimeIntervalAnalyticService(_mockLogger.Object);
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_WithValidData_ReturnsTimeIntervalAnalytic()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Date, now)
            .CreateMany(5)
            .ToList();

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(pageAnalytics);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(pageAnalytics[0].PageId);
        // result.Date.Should().Be(pageAnalytics[0].Date);
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_WithNullData_ReturnsNull()
    {
        // Arrange
        List<PageAnalytics> nullData = null;

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(nullData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_WithNullInList_ReturnsNull()
    {
        // Arrange
        var data = new List<PageAnalytics> { null, _fixture.Create<PageAnalytics>() };

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(data);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_WithSingleRecord_ReturnsValidResult()
    {
        // Arrange
        var pageAnalytic = _fixture.Create<PageAnalytics>();
        var data = new List<PageAnalytics> { pageAnalytic };

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(data);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(pageAnalytic.PageId);
        // result.Date.Should().Be(pageAnalytic.Date);
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_SetsCorrectTimeIntervalId()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15, 14, 30, 0);
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Date, date)
            .CreateMany(3)
            .ToList();

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(pageAnalytics);

        // Assert
        result.Should().NotBeNull();
        result.TimeIntervalId.Should().Be((byte)TimeIntervalUtilities.GetTimeInterval(14));
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_WithMorningHour_SetsCorrectInterval()
    {
        // Arrange
        var morningDate = new DateTime(2024, 1, 15, 8, 30, 0);
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Date, morningDate)
            .Create();
        var data = new List<PageAnalytics> { pageAnalytics };

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(data);

        // Assert
        result.TimeIntervalId.Should().Be((byte)TimeIntervalUtilities.GetTimeInterval(8));
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_WithEveningHour_SetsCorrectInterval()
    {
        // Arrange
        var eveningDate = new DateTime(2024, 1, 15, 20, 30, 0);
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Date, eveningDate)
            .Create();
        var data = new List<PageAnalytics> { pageAnalytics };

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(data);

        // Assert
        result.TimeIntervalId.Should().Be((byte)TimeIntervalUtilities.GetTimeInterval(20));
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_WithMultipleRecords_UsesFirstRecordDate()
    {
        // Arrange
        var firstDate = new DateTime(2024, 1, 15, 10, 0, 0);
        var secondDate = new DateTime(2024, 1, 15, 15, 0, 0);
        var pageAnalytics = new List<PageAnalytics>
        {
            _fixture.Build<PageAnalytics>().With(x => x.Date, firstDate).Create(),
            _fixture.Build<PageAnalytics>().With(x => x.Date, secondDate).Create(),
        };

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(pageAnalytics);

        // Assert
        result.Date.Should().Be(new DateOnly()); // this is total wrong, back to fix it;
        throw new NotImplementedException();
        result.TimeIntervalId.Should().Be((byte)TimeIntervalUtilities.GetTimeInterval(10));
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_WithValidData_AggregatesCorrectly()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15, 12, 0, 0);
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Date, date)
            .CreateMany(5)
            .ToList();

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(pageAnalytics);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().NotBe(default(int));
    }

    [Fact]
    public void NormalizeTimeIntervalAnalytic_WithEmptyList_ReturnsNull()
    {
        // Arrange
        var emptyData = new List<PageAnalytics>();

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(emptyData);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(23)]
    public void NormalizeTimeIntervalAnalytic_WithDifferentHours_SetsValidIntervalId(int hour)
    {
        // Arrange
        var date = new DateTime(2024, 1, 15, hour, 30, 0);
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Date, date)
            .Create();
        var data = new List<PageAnalytics> { pageAnalytics };

        // Act
        var result = _timeIntervalAnalyticService.NormalizeTimeIntervalAnalytic(data);

        // Assert
        result.TimeIntervalId.Should().Be((byte)TimeIntervalUtilities.GetTimeInterval(hour));
    }
}
