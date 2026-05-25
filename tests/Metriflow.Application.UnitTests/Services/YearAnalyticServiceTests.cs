using Metriflow.Application.Interfaces;
using Metriflow.Application.Services;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Metriflow.Application.UnitTests.Services;

public class YearAnalyticServiceTests
{
    private readonly Mock<ILogger<YearAnalyticService>> _mockLogger;
    private readonly IYearAnalyticService _yearAnalyticService;
    private readonly Fixture _fixture;

    public YearAnalyticServiceTests()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<YearAnalyticService>>();
        _yearAnalyticService = new YearAnalyticService(_mockLogger.Object);
    }

    [Fact]
    public void NormalizeYearlyAnalytic_WithValidData_ReturnsYearlyAnalytics()
    {
        // Arrange
        var monthData = _fixture.Build<MonthlyAnalytic>()
            .With(x => x.YearMonth, new DateOnly(2024, 1, 1))
            .CreateMany(12)
            .ToList();

        // Act
        var result = _yearAnalyticService.NormalizeYearlyAnalytic(monthData);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(monthData[0].PageId);
        result.Year.Should().Be(2024);
    }

    [Fact]
    public void NormalizeYearlyAnalytic_WithNullData_ThrowsNullReferenceException()
    {
        // Arrange
        List<MonthlyAnalytic> nullData = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            _yearAnalyticService.NormalizeYearlyAnalytic(nullData));
    }

    [Fact]
    public void NormalizeYearlyAnalytic_WithNullInList_ThrowsNullReferenceException()
    {
        // Arrange
        var data = new List<MonthlyAnalytic> { null, _fixture.Create<MonthlyAnalytic>() };

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            _yearAnalyticService.NormalizeYearlyAnalytic(data));
    }

    [Fact]
    public void NormalizeYearlyAnalytic_WithSingleMonth_ReturnsValidResult()
    {
        // Arrange
        var monthData = new List<MonthlyAnalytic> { _fixture.Create<MonthlyAnalytic>() };

        // Act
        var result = _yearAnalyticService.NormalizeYearlyAnalytic(monthData);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(monthData[0].YearMonth.Year);
    }

    [Fact]
    public void NormalizeYearlyAnalytic_UsesFirstMonthYearValue()
    {
        // Arrange
        var monthData = _fixture.Build<MonthlyAnalytic>()
            .With(x => x.YearMonth, new DateOnly(2023, 5, 1))
            .CreateMany(5)
            .ToList();

        // Act
        var result = _yearAnalyticService.NormalizeYearlyAnalytic(monthData);

        // Assert
        result.Year.Should().Be(2023);
    }

    [Fact]
    public void NormalizeYearlyAnalytic_WithDifferentYears_UsesFirstYear()
    {
        // Arrange
        var monthData = new List<MonthlyAnalytic>
        {
            _fixture.Build<MonthlyAnalytic>()
                .With(x => x.YearMonth, new DateOnly(2024, 1, 1))
                .Create(),
            _fixture.Build<MonthlyAnalytic>()
                .With(x => x.YearMonth, new DateOnly(2025, 1, 1))
                .Create(),
        };

        // Act
        var result = _yearAnalyticService.NormalizeYearlyAnalytic(monthData);

        // Assert
        result.Year.Should().Be(2024);
    }

    [Fact]
    public void NormalizeYearlyAnalytic_WithFullYear_ReturnsValidAnalytics()
    {
        // Arrange
        var pageId = _fixture.Create<int>();
        var monthData = new List<MonthlyAnalytic>();
        for (int month = 1; month <= 12; month++)
        {
            monthData.Add(_fixture.Build<MonthlyAnalytic>()
                .With(x => x.PageId, pageId)
                .With(x => x.YearMonth, new DateOnly(2024, month, 1))
                .Create());
        }

        // Act
        var result = _yearAnalyticService.NormalizeYearlyAnalytic(monthData);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().Be(pageId);
        result.Year.Should().Be(2024);
    }

    [Fact]
    public void NormalizeYearlyAnalytic_WithMultipleMonths_AggregatesCorrectly()
    {
        // Arrange
        var monthData = _fixture.Build<MonthlyAnalytic>()
            .With(x => x.YearMonth, new DateOnly(2024, 1, 1))
            .CreateMany(6)
            .ToList();

        // Act
        var result = _yearAnalyticService.NormalizeYearlyAnalytic(monthData);

        // Assert
        result.Should().NotBeNull();
        result.PageId.Should().NotBe(default(int));
    }

    [Fact]
    public void NormalizeYearlyAnalytic_ParsFromFirstMonth()
    {
        // Arrange
        var firstPageId = _fixture.Create<int>();
        var monthData = _fixture.Build<MonthlyAnalytic>()
            .With(x => x.PageId, firstPageId)
            .With(x => x.YearMonth, new DateOnly(2024, 1, 1))
            .CreateMany(1)
            .ToList();

        monthData.AddRange(_fixture.Build<MonthlyAnalytic>()
            .With(x => x.YearMonth, new DateOnly(2024, 2, 1))
            .CreateMany(1)
            .ToList());

        // Act
        var result = _yearAnalyticService.NormalizeYearlyAnalytic(monthData);

        // Assert
        result.PageId.Should().Be(firstPageId);
    }
}
