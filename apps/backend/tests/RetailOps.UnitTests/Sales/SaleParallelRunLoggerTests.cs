using Microsoft.Extensions.Logging;
using Moq;
using RetailOps.Infrastructure.Legacy.Sales;
using Xunit;

namespace RetailOps.UnitTests.Sales;

public class SaleParallelRunLoggerTests
{
    [Fact]
    public void LogComparison_WithinTolerance_LogsInformation()
    {
        var logger = new Mock<ILogger<SaleParallelRunLogger>>();
        var sut = new SaleParallelRunLogger(logger.Object);

        sut.LogComparison(1, Guid.NewGuid(), Guid.NewGuid(), 100m, 100.05m);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogComparison_ExceedsThreshold_LogsWarning()
    {
        var logger = new Mock<ILogger<SaleParallelRunLogger>>();
        var sut = new SaleParallelRunLogger(logger.Object);

        sut.LogComparison(1, Guid.NewGuid(), Guid.NewGuid(), 100m, 110m);

        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
