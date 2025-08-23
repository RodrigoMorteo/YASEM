// Intended path: YASEM.Core/Interfaces/ITestCaseLoader.cs
using System.Threading.Tasks;
using YASEM.Core.Models;

namespace YASEM.Core.Interfaces
{
    public interface ITestCaseLoader
    {
        Task<Config> LoadAsync(string jsonPath);
    }
}