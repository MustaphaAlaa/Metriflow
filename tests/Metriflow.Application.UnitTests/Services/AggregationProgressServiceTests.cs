// using Metriflow.Application.Interfaces;
// using Metriflow.Application.Services;
// using Metriflow.Domain.Entities;
// using Metriflow.Domain.Entities.Workers;
// using IRepository.Generic;
// using Microsoft.Extensions.Logging;

// namespace Metriflow.Application.UnitTests.Services;

// public class AggregationProgressServiceTests
// {
//     private readonly Mock<ILogger<AggregationProgressService>> _mockLogger;
//     private readonly Mock<IAggregationProgressRepository> _mockAggregationProgressRepository;
//     private readonly IBaseRepository<PageAnalytics> _mockPageAnalyticsRepository;
//     private readonly IAggregationProgressService _aggregationProgressService;
//     private readonly Fixture _fixture;

//     public AggregationProgressServiceTests()
//     {
//         _fixture = new Fixture();
//         _mockLogger = new Mock<ILogger<AggregationProgressService>>();
//         _mockAggregationProgressRepository = new Mock<IAggregationProgressRepository>();
//         _aggregationProgressService = new AggregationProgressService(
//             _mockAggregationProgressRepository.Object,
//             _mockLogger.Object
//         );
//     }

//     [Fact]
//     public async Task UpdateProgressAsync_WithValidId_UpdatesProgress()
//     {
//         // Arrange
//         var id = _fixture.Create<int>();
//         var aggregationProgress = _fixture.Create<AggregationProgress>();
        
//         _mockAggregationProgressRepository
//             .Setup(x => x.RetrieveAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AggregationProgress, bool>>>()))
//             .ReturnsAsync(aggregationProgress);

//         // Act
//         await _aggregationProgressService.UpdateProgressAsync(id);

//         // Assert
//         _mockAggregationProgressRepository.Verify(
//             x => x.RetrieveAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AggregationProgress, bool>>>()), 
//             Times.Once);
//     }

//     [Fact]
//     public async Task UpdateProgressAsync_VerifiesRepositoryInteraction()
//     {
//         // Arrange
//         var id = _fixture.Create<int>();
//         var aggregationProgress = _fixture.Create<AggregationProgress>();
        
//         _mockAggregationProgressRepository
//             .Setup(x => x.RetrieveAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AggregationProgress, bool>>>()))
//             .ReturnsAsync(aggregationProgress);

//         // Act
//         await _aggregationProgressService.UpdateProgressAsync(id);

//         // Assert
//         _mockAggregationProgressRepository.Verify(
//             x => x.RetrieveAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AggregationProgress, bool>>>()), 
//             Times.Once);
//     }

//     [Fact]
//     public async Task UpdateProgressAsync_LogsInformation()
//     {
//         // Arrange
//         var id = _fixture.Create<int>();
//         var aggregationProgress = _fixture.Create<AggregationProgress>();
        
//         _mockAggregationProgressRepository
//             .Setup(x => x.RetrieveAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AggregationProgress, bool>>>()))
//             .ReturnsAsync(aggregationProgress);

//         // Act
//         await _aggregationProgressService.UpdateProgressAsync(id);

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

//     [Fact]
//     public async Task UpdateProgressAsync_WithMultipleCalls_CallsRepositoryMultipleTimes()
//     {
//         // Arrange
//         var ids = _fixture.CreateMany<int>(3).ToList();
//         var aggregationProgress = _fixture.Create<AggregationProgress>();
        
//         _mockAggregationProgressRepository
//             .Setup(x => x.RetrieveAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AggregationProgress, bool>>>()))
//             .ReturnsAsync(aggregationProgress);

//         // Act
//         foreach (var id in ids)
//         {
//             await _aggregationProgressService.UpdateProgressAsync(id);
//         }

//         // Assert
//         _mockAggregationProgressRepository.Verify(
//             x => x.RetrieveAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AggregationProgress, bool>>>()), 
//             Times.Exactly(3));
//     }
// }
