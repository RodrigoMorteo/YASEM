using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using YASEM.Core.Interfaces;
using YASEM.Core.Models;
using YASEM.Core.Exceptions;
using System.Resources;

using System.Linq;
using NJsonSchema;

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

            // Load the schema
            var schemaPath = Path.Combine(AppContext.BaseDirectory, "Configuration", "test-schema.json");
            if (!File.Exists(schemaPath))
            {
                // This is a development-time error, should not happen in a deployed app
                throw new FileNotFoundException("Schema file 'test-schema.json' not found in the application's configuration directory.", schemaPath);
            }
            var schema = await JsonSchema.FromFileAsync(schemaPath);

            // Read the test case content
            var jsonContent = await File.ReadAllTextAsync(fullPath);

            // Validate against the schema
            var validationErrors = schema.Validate(jsonContent);
            if (validationErrors.Any())
            {
                var errorMessages = validationErrors.Select(e => $"{e.Path}: {e.Kind}");
                var combinedErrorMessage = $"JSON validation failed:{Environment.NewLine}  - " + string.Join($"{Environment.NewLine}  - ", errorMessages);
                throw new InvalidConfigurationException(combinedErrorMessage);
            }

            // Deserialize if validation passes
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<Config>(jsonContent, options);
            }
            catch (JsonException ex)
            {
                throw new InvalidConfigurationException($"Error deserializing JSON file at {fullPath}. Details: {ex.Message}", ex);
            }
        }
    }

    //TODO: check and set defaults
}
