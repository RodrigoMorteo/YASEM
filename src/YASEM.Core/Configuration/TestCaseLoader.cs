using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using YASEM.Core.Interfaces;
using YASEM.Core.Models;
using YASEM.Core.Exceptions;
using System.Resources;

namespace YASEM.Core.Configuration
{
        // This class is refactored from ConfigLoader.
    // Its sole responsibility is now loading and deserializing the test case JSON.
    public class TestCaseLoader : ITestCaseLoader
    {
        private readonly ResourceManager _resourceManager;

        public TestCaseLoader()
        {
            _resourceManager = new ResourceManager("YASEM.Core.Resources.ErrorMessages", typeof(TestCaseLoader).Assembly);
        }

        public async Task<Config> LoadAsync(string jsonPath)
        {
            var fullPath = Path.GetFullPath(jsonPath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(_resourceManager.GetString("InvalidConfigurationError"), fullPath);
            }

            await using var stream = File.OpenRead(fullPath);
            try
            {
                // Use System.Text.Json for better performance and to remove Newtonsoft dependency.
                // Options allow for case-insensitive properties to match the JSON file.
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return await JsonSerializer.DeserializeAsync<Config>(stream, options);
            }
            catch (JsonException ex)
            {
                throw new InvalidConfigurationException($"Error deserializing JSON file at {fullPath}. Details: {ex.Message}", ex);
            }
        }
    }

    //TODO: check and set defaults
}
