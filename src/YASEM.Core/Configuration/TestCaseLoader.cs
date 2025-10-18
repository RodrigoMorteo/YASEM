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

            var schemaPath = Path.Combine(AppContext.BaseDirectory, "Configuration", "test-schema.json");
            if (!File.Exists(schemaPath))
            {
                throw new FileNotFoundException("Schema file 'test-schema.json' not found in the application's configuration directory.", schemaPath);
            }
            var schema = await JsonSchema.FromFileAsync(schemaPath);

            var jsonContent = await File.ReadAllTextAsync(fullPath);

            try
            {
                var validationErrors = schema.Validate(jsonContent);
                if (validationErrors.Any())
                {
                    var errorMessages = validationErrors.Select(e => $"{e.Path}: {e.Kind}");
                    var combinedErrorMessage = $"JSON validation failed:{Environment.NewLine}  - " + string.Join($"{Environment.NewLine}  - ", errorMessages);
                    throw new InvalidConfigurationException(combinedErrorMessage);
                }
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                throw new InvalidConfigurationException($"Error deserializing JSON file at {fullPath}. Details: {ex.Message}", ex);
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<Config>(jsonContent, options);
                if (config == null)
                {
                    throw new InvalidConfigurationException($"The JSON file at {fullPath} is empty or invalid.");
                }
                return config;
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                throw new InvalidConfigurationException($"Error deserializing JSON file at {fullPath}. Details: {ex.Message}", ex);
            }
        }
    }
}