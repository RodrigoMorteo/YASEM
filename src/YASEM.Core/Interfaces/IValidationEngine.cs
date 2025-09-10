using System.Collections.Generic;
using MimeKit;
using YASEM.Core.Validators;

namespace YASEM.Core.Interfaces
{
    public interface IValidationEngine
    {
        List<ValidationResult> Execute(List<MimeMessage> messages);
    }
}