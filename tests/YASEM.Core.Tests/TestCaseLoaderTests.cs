using Xunit;
using YASEM.Core.Configuration;
using YASEM.Core.Models;
using System.IO;
using System.Text.Json;

namespace YASEM.Core.Tests
{
    public class TestCaseLoaderTests
    {
        [Fact]
        public async Task LoadTestCase_WithValidJson_ShouldReturnTestCase()
        {
            // Arrange
            var json = "{\"Id\":\"test-case-1\",\"Name\":\"Test Case 1\",\"TestSteps\":[{\"Description\":\"Test Step 1\",\"Field\":\"Subject\",\"ValidationType\":\"Field\",\"Assertion\":\"Contains\",\"ExpectedValue\":\"test\"}]}";
            var filePath = "test.json";
            await File.WriteAllTextAsync(filePath, json);
            var loader = new TestCaseLoader();

            // Act
            var testCase = await loader.LoadAsync(filePath);

            // Assert
            Assert.NotNull(testCase);
            Assert.Single(testCase.TestSteps);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public async Task LoadTestCase_WithInvalidJson_ShouldThrowException()
        {
            // Arrange
            var json = "{\"TestSteps\":[{\"Description\":\"Test Step 1\"}";
            var filePath = "test.json";
            await File.WriteAllTextAsync(filePath, json);
            var loader = new TestCaseLoader();

            // Assert
            await Assert.ThrowsAsync<YASEM.Core.Exceptions.InvalidConfigurationException>(() => loader.LoadAsync(filePath));

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public async Task LoadTestCase_WithMissingFile_ShouldThrowException()
        {
            // Arrange
            var loader = new TestCaseLoader();

            // Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => loader.LoadAsync("nonexistent.json"));
        }
    }
}