// using Metriflow.Application.Extensions;
// using Microsoft.Extensions.DependencyInjection;

// namespace Metriflow.Application.UnitTests.Extensions;

// public class ApplicationLayerExtensionsTests
// {
//     [Fact]
//     public void AddApplicationLayerDiServices_RegistersPageServices()
//     {
//         // Arrange
//         var services = new ServiceCollection();

//         // Act
//         services.AddApplicationLayerDiServices();
//         var serviceProvider = services.BuildServiceProvider();

//         // Assert
//         var pageServices = serviceProvider.GetService<IPageServices>();
//         pageServices.Should().NotBeNull();
//     }

//     [Fact]
//     public void AddApplicationLayerDiServices_RegistersDailyAnalyticsService()
//     {
//         // Arrange
//         var services = new ServiceCollection();

//         // Act
//         services.AddApplicationLayerDiServices();
//         var serviceProvider = services.BuildServiceProvider();

//         // Assert
//         var dailyAnalyticsService = serviceProvider.GetService<IDailyAnalyticsService>();
//         dailyAnalyticsService.Should().NotBeNull();
//     }

//     [Fact]
//     public void AddApplicationLayerDiServices_RegistersServiceAsScoped()
//     {
//         // Arrange
//         var services = new ServiceCollection();

//         // Act
//         services.AddApplicationLayerDiServices();

//         // Assert
//         var pageServiceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IPageServices));
//         pageServiceDescriptor.Should().NotBeNull();
//         pageServiceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
//     }

//     [Fact]
//     public void AddApplicationLayerDiServices_MultipleRegistrations_DoesNotThrow()
//     {
//         // Arrange
//         var services = new ServiceCollection();

//         // Act & Assert
//         services.AddApplicationLayerDiServices();
//         services.AddApplicationLayerDiServices();
//         var serviceProvider = services.BuildServiceProvider();
        
//         var pageServices = serviceProvider.GetService<IPageServices>();
//         pageServices.Should().NotBeNull();
//     }

//     [Fact]
//     public void AddApplicationLayerDiServices_ReturnsServiceCollection()
//     {
//         // Arrange
//         var services = new ServiceCollection();

//         // Act
//         var result = services.AddApplicationLayerDiServices();

//         // Assert
//         result.Should().BeOfType<ServiceCollection>();
//         result.Should().Be(services);
//     }

//     [Fact]
//     public void AddApplicationLayerDiServices_RegistersImplementationCorrectly()
//     {
//         // Arrange
//         var services = new ServiceCollection();
//         services.AddLogging();

//         // Act
//         services.AddApplicationLayerDiServices();
//         var serviceProvider = services.BuildServiceProvider();

//         // Act & Assert
//         using (var scope = serviceProvider.CreateScope())
//         {
//             var pageServices = scope.ServiceProvider.GetRequiredService<IPageServices>();
//             pageServices.Should().BeOfType<PageServices>();
//         }
//     }
// }
