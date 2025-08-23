using System.Collections.Generic;
using YASEM.Core.Models;

namespace YASEM.Core.Interfaces
{
    public interface IValidationEngineFactory
    {
        IValidationEngine Create(List<TestStep> steps);
    }
}