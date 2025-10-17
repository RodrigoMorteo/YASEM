using MimeKit;
using YASEM.Core.Models;

namespace YASEM.Core.Validators
{
    public interface IValidator
    {
        ValidationResult Validate(MimeMessage message);
    }
}
