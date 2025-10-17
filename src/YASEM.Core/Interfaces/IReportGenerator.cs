using System.Collections.Generic;
using YASEM.Core.Validators;

namespace YASEM.Core.Interfaces
{
    public interface IReportGenerator
    {
        void Generate(List<ValidationResult> results, string outputPath);
    }
}
