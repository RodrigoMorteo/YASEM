using MimeKit;
using YASEM.Core.Models;
using System.Linq;

namespace YASEM.Core.Validators
{
    public class AttachmentValidator
    {
        private readonly AssertionType _assertion;
        private readonly string _expectedValue;

        public AttachmentValidator(AssertionType assertion, string expectedValue)
        {
            _assertion = assertion;
            _expectedValue = expectedValue;
        }

        public ValidationResult PerformOn(MimeMessage message)
        {
            var result = new ValidationResult { Status = Result.Fail };

            switch (_assertion)
            {
                case AssertionType.Exists:
                    if (message.Attachments.Any())
                    {
                        result.Status = Result.Pass;
                    }
                    result.Actual = message.Attachments.Count().ToString();
                    break;
                case AssertionType.Notexists:
                    if (!message.Attachments.Any())
                    {
                        result.Status = Result.Pass;
                    }
                    result.Actual = message.Attachments.Count().ToString();
                    break;
                case AssertionType.Contains:
                    if (message.Attachments.Any(a => a.ContentType?.Name.Contains(_expectedValue) == true))
                    {
                        result.Status = Result.Pass;
                    }
                    result.Actual = string.Join(", ", message.Attachments.Select(a => a.ContentType?.Name ?? ""));
                    break;
                case AssertionType.Notcontains:
                    if (!message.Attachments.Any(a => a.ContentType?.Name.Contains(_expectedValue) == true))
                    {
                        result.Status = Result.Pass;
                    }
                    result.Actual = string.Join(", ", message.Attachments.Select(a => a.ContentType?.Name ?? ""));
                    break;
            }

            return result;
        }
    }
}
