using Metriflow.Application.Interfaces;
using Metriflow.Application.Services;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.UnitTests.Services;

public class MonthlyAnalyticServiceTests
{
    private readonly Mock<ILogger<MonthlyAnalyticService>> _mockLogger;
    private readonly IMonthlyAnalyticService _monthlyAnalyticService;
    private readonly Fixture _fixture;

    public MonthlyAnalyticServiceTests()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<MonthlyAnalyticService>>();
        _monthlyAnalyticService = new MonthlyAnalyticService(_mockLogger.Object);
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_WithValidData_ReturnsMonthlyAnalytic()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Date, now)
            .CreateMany(5)
            .ToList();

        // Act
        var result = _monthlyAnalyticService.NormalizeMonthlyAnalytic(pageAnalytics);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(pageAnalytics[0].PageId);
        result.YearMonth.Year.Should().Be(now.Year);
        result.YearMonth.Month.Should().Be(now.Month);
        result.YearMonth.Day.Should().Be(1);
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_CalculatesCorrectAveragePerformance()
    {
        // Arrange
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.PerformanceScore, 80)
            .CreateMany(4)
            .ToList();

        // Act
        var result = _monthlyAnalyticService.NormalizeMonthlyAnalytic(pageAnalytics);

        // Assert
        result.AvgPerformance.Should().Be(80);
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_CalculatesCorrectTotalSessions()
    {
        // Arrange
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Sessions, 100)
            .CreateMany(3)
            .ToList();

        // Act
        var result = _monthlyAnalyticService.NormalizeMonthlyAnalytic(pageAnalytics);

        // Assert
        result.TotalSessions.Should().Be(300);
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_CalculatesCorrectTotalViews()
    {
        // Arrange
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Views, 50)
            .CreateMany(4)
            .ToList();

        // Act
        var result = _monthlyAnalyticService.NormalizeMonthlyAnalytic(pageAnalytics);

        // Assert
        result.TotalViews.Should().Be(200);
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_CalculatesCorrectTotalUsers()
    {
        // Arrange
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Users, 25)
            .CreateMany(4)
            .ToList();

        // Act
        var result = _monthlyAnalyticService.NormalizeMonthlyAnalytic(pageAnalytics);

        // Assert
        result.TotalUsers.Should().Be(100);
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_WithNullData_ThrowsNullReferenceException()
    {
        // Arrange
        List<PageAnalytics> nullData = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            _monthlyAnalyticService.NormalizeMonthlyAnalytic(nullData));
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_WithNullInList_ThrowsNullReferenceException()
    {
        // Arrange
        var data = new List<PageAnalytics> { null, _fixture.Create<PageAnalytics>() };

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            _monthlyAnalyticService.NormalizeMonthlyAnalytic(data));
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_WithSingleMonth_ReturnsValidResult()
    {
        // Arrange
        var pageAnalytic = _fixture.Create<PageAnalytics>();
        var data = new List<PageAnalytics> { pageAnalytic };

        // Act
        var result = _monthlyAnalyticService.NormalizeMonthlyAnalytic(data);

        // Assert
        result.Should().NotBeNull();
        result.YearMonth.Month.Should().Be(pageAnalytic.Date.Month);
        result.YearMonth.Year.Should().Be(pageAnalytic.Date.Year);
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_NormalizesDateToFirstOfMonth()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15, 14, 30, 0);
        var pageAnalytics = _fixture.Build<PageAnalytics>()
            .With(x => x.Date, date)
            .CreateMany(2)
            .ToList();

        // Act
        var result = _monthlyAnalyticService.NormalizeMonthlyAnalytic(pageAnalytics);

        // Assert
        result.YearMonth.Should().Be(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void NormalizeMonthlyAnalytic_WithVariousPerformanceScores_CalculatesCorrectAverage()
    {
        // Arrange
        var pageAnalytics = new List<PageAnalytics>
        {
            _fixture.Build<PageAnalytics>().With(x => x.PerformanceScore, 60).Create(),
            _fixture.Build<PageAnalytics>().With(x => x.PerformanceScore, 80).Create(),
            _fixture.Build<PageAnalytics>().With(x => x.PerformanceScore, 100).Create(),
        };

        // Act
        var result = _monthlyAnalyticService.NormalizeMonthlyAnalytic(pageAnalytics);

        // Assert
        result.AvgPerformance.Should().Be(80);
    }
}
