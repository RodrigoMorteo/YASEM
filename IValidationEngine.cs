using System.Collections.Generic;
using MimeKit;

namespace YASEM.Core.Interfaces
{
    public interface IValidationEngine
    {
        List<ValidationResult> Execute(List<MimeMessage> messages);
    }
}