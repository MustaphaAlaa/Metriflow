// using IRepository.Generic;
// using Metriflow.Application.Interfaces;
// using Metriflow.Application.Services.Orchestration;
// using Metriflow.Domain.Entities;
// using Metriflow.Domain.Entities.Workers;
// using Microsoft.Extensions.Logging;

// namespace Metriflow.Application.UnitTests.Services;

// public class PageAnalyticsOrchestrationTests
// {
//     private readonly Mock<IAggregationProgressRepository> _mockAggregationProgressRepository;
//     private readonly Mock<IBaseRepository<PageAnalytics>> _mockPageAnalyticsRepository;
//     private readonly Mock<IPageAnalyticsServices> _mockPageAnalyticsServices;
//     private readonly Mock<ILogger<PageAnalyticsOrchestration>> _mockLogger;
//     private readonly IPageAnalyticsOrchestration _pageAnalyticsOrchestration;
//     private readonly Fixture _fixture;

//     public PageAnalyticsOrchestrationTests()
//     {
//         _fixture = new Fixture();
//         _mockAggregationProgressRepository = new Mock<IAggregationProgressRepository>();
//         _mockPageAnalyticsRepository = new Mock<IBaseRepository<PageAnalytics>>();
//         _mockPageAnalyticsServices = new Mock<IPageAnalyticsServices>();
//         _mockLogger = new Mock<ILogger<PageAnalyticsOrchestration>>();

//         _pageAnalyticsOrchestration = new PageAnalyticsOrchestration(
//             _mockAggregationProgressRepository.Object,
//             _mockPageAnalyticsRepository.Object,
//             _mockPageAnalyticsServices.Object,
//             _mockLogger.Object
//         );
//     }

//     [Fact]
//     public async Task CreatePageAnalyticsAsync_WithValidRecords_ReturnsCountGreaterThanZero()
//     {
//         // Arrange
//         var aggregateRecords = _fixture.CreateMany<AggregateRecordsJoins>(5).ToList();
//         var pageAnalytics = _fixture.CreateMany<PageAnalytics>(5).ToList();

//         _mockAggregationProgressRepository
//             .Setup(x => x.GetNoneCorrelationAggregateRecords())
//             .Returns(aggregateRecords.AsEnumerable());

//         _mockPageAnalyticsServices
//             .Setup(x => x.RecordsToPageAnalytics(It.IsAny<IEnumerable<AggregateRecordsJoins>>()))
//             .Returns(pageAnalytics);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<PageAnalytics>>()))
//             .Returns(Task.CompletedTask);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.SaveChangesAsync())
//             .Returns(Task.CompletedTask);

//         // Act
//         var result = await _pageAnalyticsOrchestration.CreatePageAnalyticsAsync();

//         // Assert
//         result.Should().Be(5);
//     }

//     [Fact]
//     public async Task CreatePageAnalyticsAsync_WithNoRecords_ReturnsZero()
//     {
//         // Arrange
//         var emptyRecords = new List<AggregateRecordsJoins>();

//         _mockAggregationProgressRepository
//             .Setup(x => x.GetNoneCorrelationAggregateRecords())
//             .Returns(emptyRecords.AsEnumerable());

//         // Act
//         var result = await _pageAnalyticsOrchestration.CreatePageAnalyticsAsync();

//         // Assert
//         result.Should().Be(0);
//         _mockPageAnalyticsServices.Verify(
//             x => x.RecordsToPageAnalytics(It.IsAny<IEnumerable<AggregateRecordsJoins>>()),
//             Times.Never);
//     }

//     [Fact]
//     public async Task CreatePageAnalyticsAsync_CallsPageAnalyticsService()
//     {
//         // Arrange
//         var aggregateRecords = _fixture.CreateMany<AggregateRecordsJoins>(3).ToList();
//         var pageAnalytics = _fixture.CreateMany<PageAnalytics>(3).ToList();

//         _mockAggregationProgressRepository
//             .Setup(x => x.GetNoneCorrelationAggregateRecords())
//             .Returns(aggregateRecords.AsEnumerable());

//         _mockPageAnalyticsServices
//             .Setup(x => x.RecordsToPageAnalytics(It.IsAny<IEnumerable<AggregateRecordsJoins>>()))
//             .Returns(pageAnalytics);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<PageAnalytics>>()))
//             .Returns(Task.CompletedTask);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.SaveChangesAsync())
//             .Returns(Task.CompletedTask);

//         // Act
//         await _pageAnalyticsOrchestration.CreatePageAnalyticsAsync();

//         // Assert
//         _mockPageAnalyticsServices.Verify(
//             x => x.RecordsToPageAnalytics(It.IsAny<IEnumerable<AggregateRecordsJoins>>()),
//             Times.Once);
//     }

//     [Fact]
//     public async Task CreatePageAnalyticsAsync_CallsCreateRangeAsync()
//     {
//         // Arrange
//         var aggregateRecords = _fixture.CreateMany<AggregateRecordsJoins>(3).ToList();
//         var pageAnalytics = _fixture.CreateMany<PageAnalytics>(3).ToList();

//         _mockAggregationProgressRepository
//             .Setup(x => x.GetNoneCorrelationAggregateRecords())
//             .Returns(aggregateRecords.AsEnumerable());

//         _mockPageAnalyticsServices
//             .Setup(x => x.RecordsToPageAnalytics(It.IsAny<IEnumerable<AggregateRecordsJoins>>()))
//             .Returns(pageAnalytics);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<PageAnalytics>>()))
//             .Returns(Task.CompletedTask);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.SaveChangesAsync())
//             .Returns(Task.CompletedTask);

//         // Act
//         await _pageAnalyticsOrchestration.CreatePageAnalyticsAsync();

//         // Assert
//         _mockPageAnalyticsRepository.Verify(
//             x => x.CreateRangeAsync(It.IsAny<IEnumerable<PageAnalytics>>()),
//             Times.Once);
//     }

//     [Fact]
//     public async Task CreatePageAnalyticsAsync_UpdatesAggregationProgress()
//     {
//         // Arrange
//         var aggregateRecords = _fixture.CreateMany<AggregateRecordsJoins>(3).ToList();
//         var pageAnalytics = _fixture.CreateMany<PageAnalytics>(3).ToList();

//         _mockAggregationProgressRepository
//             .Setup(x => x.GetNoneCorrelationAggregateRecords())
//             .Returns(aggregateRecords.AsEnumerable());

//         _mockPageAnalyticsServices
//             .Setup(x => x.RecordsToPageAnalytics(It.IsAny<IEnumerable<AggregateRecordsJoins>>()))
//             .Returns(pageAnalytics);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<PageAnalytics>>()))
//             .Returns(Task.CompletedTask);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.SaveChangesAsync())
//             .Returns(Task.CompletedTask);

//         // Act
//         await _pageAnalyticsOrchestration.CreatePageAnalyticsAsync();

//         // Assert
//         _mockAggregationProgressRepository.Verify(
//             x => x.UpdateRange(It.IsAny<IEnumerable<AggregationProgress>>()),
//             Times.Once);
//     }

//     [Fact]
//     public async Task CreatePageAnalyticsAsync_SavesChanges()
//     {
//         // Arrange
//         var aggregateRecords = _fixture.CreateMany<AggregateRecordsJoins>(2).ToList();
//         var pageAnalytics = _fixture.CreateMany<PageAnalytics>(2).ToList();

//         _mockAggregationProgressRepository
//             .Setup(x => x.GetNoneCorrelationAggregateRecords())
//             .Returns(aggregateRecords.AsEnumerable());

//         _mockPageAnalyticsServices
//             .Setup(x => x.RecordsToPageAnalytics(It.IsAny<IEnumerable<AggregateRecordsJoins>>()))
//             .Returns(pageAnalytics);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<PageAnalytics>>()))
//             .Returns(Task.CompletedTask);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.SaveChangesAsync())
//             .Returns(Task.CompletedTask);

//         // Act
//         await _pageAnalyticsOrchestration.CreatePageAnalyticsAsync();

//         // Assert
//         _mockPageAnalyticsRepository.Verify(
//             x => x.SaveChangesAsync(),
//             Times.Once);
//     }

//     [Fact]
//     public async Task CreatePageAnalyticsAsync_WithValidRecords_LogsInformation()
//     {
//         // Arrange
//         var aggregateRecords = _fixture.CreateMany<AggregateRecordsJoins>(3).ToList();
//         var pageAnalytics = _fixture.CreateMany<PageAnalytics>(3).ToList();

//         _mockAggregationProgressRepository
//             .Setup(x => x.GetNoneCorrelationAggregateRecords())
//             .Returns(aggregateRecords.AsEnumerable());

//         _mockPageAnalyticsServices
//             .Setup(x => x.RecordsToPageAnalytics(It.IsAny<IEnumerable<AggregateRecordsJoins>>()))
//             .Returns(pageAnalytics);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<PageAnalytics>>()))
//             .Returns(Task.CompletedTask);

//         _mockPageAnalyticsRepository
//             .Setup(x => x.SaveChangesAsync())
//             .Returns(Task.CompletedTask);

//         // Act
//         await _pageAnalyticsOrchestration.CreatePageAnalyticsAsync();

//         // Assert
//         _mockLogger.Verify(
//             x => x.Log(
//                 LogLevel.Information,
//                 It.IsAny<EventId>(),
//                 It.IsAny<It.IsAnyType>(),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()),
//             Times.AtLeastOnce);
//     }
// }
