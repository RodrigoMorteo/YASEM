// Intended path: YASEM.Core/Interfaces/IApplication.cs
using System.Threading.Tasks;

namespace YASEM.CLI.Interfaces
{
    public interface IApplication
    {
        Task<int> RunAsync(string[] args);
    }
}