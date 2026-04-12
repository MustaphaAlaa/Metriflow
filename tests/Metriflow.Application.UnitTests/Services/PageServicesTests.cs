// using IRepository.Generic;
// using Metriflow.Application.Interfaces;
// using Metriflow.Application.Services;
// using Metriflow.Domain.Entities;
// using Metriflow.Domain.enums;
// using Microsoft.Extensions.Logging;

// namespace Metriflow.Application.UnitTests.Services;

// public class PageServicesTests
// {
//     private readonly Mock<IPageRepository> _mockPageRepository;
//     private readonly Mock<ILogger<PageServices>> _mockLogger;
//     private readonly IPageServices _pageServices;
//     private readonly Fixture _fixture;

//     public PageServicesTests()
//     {
//         _fixture = new Fixture();
//         _mockPageRepository = new Mock<IPageRepository>();
//         _mockLogger = new Mock<ILogger<PageServices>>();
//         _pageServices = new PageServices(_mockPageRepository.Object, _mockLogger.Object);
//     }

//     [Fact]
//     public async Task NormalizePage_WithValidMessage_ReturnsNormalizedMessage()
//     {
//         // Arrange
//         var message = _fixture.Create<CombinedAnalyticsMessage>();

//         // Act
//         var result = await _pageServices.NormalizePage(message);

//         // Assert
//         result.Should().NotBeNull();
//         result.Should().Be(message);
//     }

//     [Fact]
//     public async Task NormalizePage_WithNullMessage_ReturnsNull()
//     {
//         // Arrange
//         CombinedAnalyticsMessage? message = null;

//         // Act
//         var result = await _pageServices.NormalizePage(message);

//         // Assert
//         result.Should().BeNull();
//     }

//     [Fact]
//     public async Task NormalizePage_LogsInformation()
//     {
//         // Arrange
//         var message = _fixture.Create<CombinedAnalyticsMessage>();

//         // Act
//         await _pageServices.NormalizePage(message);

//         // Assert
//         _mockLogger.Verify(
//             x => x.Log(
//                 LogLevel.Information,
//                 It.IsAny<EventId>(),
//                 It.IsAny<It.IsAnyType>(),
//                 It.IsAny<Exception>(),
//                 It.IsAny<Func<It.IsAnyType, Exception, string>>()),
//             Times.Once);
//     }

//     [Fact]
//     public async Task GetAsync_WithValidPath_ReturnPage()
//     {
//         // Arrange
//         var page = _fixture.Create<Page>();
//         _mockPageRepository.Setup(x => x.RetrieveAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Page, bool>>>()))
//             .ReturnsAsync(page);

//         // Act
//         var result = await _pageServices.GetAsync(enPages.home);

//         // Assert
//         result.Should().NotBeNull();
//         result.Should().Be(page);
//         _mockPageRepository.Verify(x => x.RetrieveAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Page, bool>>>()), Times.Once);
//     }

//     [Fact]
//     public async Task PageReport_ReturnsList()
//     {
//         // Arrange
//         var reports = _fixture.CreateMany<PageReport>(3).ToList();
//         _mockPageRepository.Setup(x => x.PageReportAsync())
//             .ReturnsAsync(reports);

//         // Act
//         var result = await _pageServices.PageReport();

//         // Assert
//         result.Should().NotBeNull();
//         result.Should().HaveCount(3);
//         result.Should().BeEquivalentTo(reports);
//         _mockPageRepository.Verify(x => x.PageReportAsync(), Times.Once);
//     }

//     [Fact]
//     public async Task PageReport_ReturnsEmptyList()
//     {
//         // Arrange
//         var emptyList = new List<PageReport>();
//         _mockPageRepository.Setup(x => x.PageReportAsync())
//             .ReturnsAsync(emptyList);

//         // Act
//         var result = await _pageServices.PageReport();

//         // Assert
//         result.Should().NotBeNull();
//         result.Should().BeEmpty();
//     }
// }
