using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Moq;
using YASEM.CLI.Interfaces;
using YASEM.Core.Interfaces;
using YASEM.Core.Utilities;

namespace YASEM.CLI.Tests
{
    public class ApplicationTests
    {
        private readonly Mock<ITestCaseLoader> _mockTestCaseLoader;
        private readonly Mock<IMailConnector> _mockMailConnector;
        private readonly Mock<IValidationEngineFactory> _mockValidationEngineFactory;
        private readonly Mock<IReportGenerator> _mockReportGenerator;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<Application>> _mockLogger;
        private readonly Mock<IStringLocalizer<Application>> _mockLocalizer;

        public ApplicationTests()
        {
            _mockTestCaseLoader = new Mock<ITestCaseLoader>();
            _mockMailConnector = new Mock<IMailConnector>();
            _mockValidationEngineFactory = new Mock<IValidationEngineFactory>();
            _mockReportGenerator = new Mock<IReportGenerator>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<Application>>();
            _mockLocalizer = new Mock<IStringLocalizer<Application>>();
        }

        [Fact]
        public void Application_CanBeInstantiated()
        {
            // Arrange
            var app = new Application(
                _mockTestCaseLoader.Object,
                _mockMailConnector.Object,
                _mockValidationEngineFactory.Object,
                _mockReportGenerator.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockLocalizer.Object);

            // Assert
            Assert.NotNull(app);
        }

        [Fact]
        public async Task RunAsync_WithEncryptAndCreateKeyFileOptions_GeneratesKeyAndEncrypts()
        {
            // Arrange
            var app = new Application(
                _mockTestCaseLoader.Object,
                _mockMailConnector.Object,
                _mockValidationEngineFactory.Object,
                _mockReportGenerator.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockLocalizer.Object);

            var keyFilePath = "test.key";
            var stringToEncrypt = "test-string";

            // Act
            var exitCode = await app.RunAsync(new[]
            {
                "--encrypt", stringToEncrypt,
                "--create-key-file", keyFilePath
            });

            // Assert
            Assert.Equal(0, exitCode);

            // Cleanup
            if (File.Exists(keyFilePath))
            {
                File.Delete(keyFilePath);
            }
        }

        [Fact]
        public async Task RunAsync_WithEncryptAndExistingKeyFileOptions_Encrypts()
        {
            // Arrange
            var app = new Application(
                _mockTestCaseLoader.Object,
                _mockMailConnector.Object,
                _mockValidationEngineFactory.Object,
                _mockReportGenerator.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockLocalizer.Object);

            var keyFilePath = "test.key";
            var stringToEncrypt = "test-string";
            var key = CryptoUtil.GenerateKey();
            await File.WriteAllBytesAsync(keyFilePath, key);

            // Act
            var exitCode = await app.RunAsync(new[]
            {
                "--encrypt", stringToEncrypt,
                "--key-path", keyFilePath
            });

            // Assert
            Assert.Equal(0, exitCode);

            // Cleanup
            if (File.Exists(keyFilePath))
            {
                File.Delete(keyFilePath);
            }
        }

        [Fact]
        public async Task RunAsync_WithoutJsonAndReportPathOptions_ReturnsError()
        {
            // Arrange
            var app = new Application(
                _mockTestCaseLoader.Object,
                _mockMailConnector.Object,
                _mockValidationEngineFactory.Object,
                _mockReportGenerator.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockLocalizer.Object);

            // Act
            var exitCode = await app.RunAsync(new string[] { });

            // Assert
            Assert.NotEqual(0, exitCode);
        }
        /*[Fact]
        public async Task RunAsync_WithTimeoutOption_ShouldReturnSuccess()
        {
            // Arrange
            var app = new Application(
                _mockTestCaseLoader.Object,
                _mockMailConnector.Object,
                _mockValidationEngineFactory.Object,
                _mockReportGenerator.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockLocalizer.Object);

            // Act
            var exitCode = await app.RunAsync(new[]
            {
                "--json-path", "test.json",
                "--key-path", "/home/rod/repos/YASEM/my.key",
                "--report-path", "report.html",
                "--timeout", "10"
            });

            // Assert
            Assert.Equal(0, exitCode);
        }
*/
        [Fact]
        public async Task RunAsync_WithHelpOption_ShouldReturnSuccess()
        {
            // Arrange
            var app = new Application(
                _mockTestCaseLoader.Object,
                _mockMailConnector.Object,
                _mockValidationEngineFactory.Object,
                _mockReportGenerator.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockLocalizer.Object);

            // Act
            var exitCode = await app.RunAsync(new[]
            {
                "--help"
            });

            // Assert
            Assert.Equal(0, exitCode);
        }
    }
}   