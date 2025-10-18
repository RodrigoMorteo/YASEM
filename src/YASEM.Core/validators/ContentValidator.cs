using MimeKit;
using YASEM.Core.Models;
using System.Text.RegularExpressions;

namespace YASEM.Core.Validators
{
    public class ContentValidator
    {
        private readonly AssertionType _assertion;
        private readonly string _expectedValue;

        public ContentValidator(AssertionType assertion, string expectedValue)
        {
            _assertion = assertion;
            _expectedValue = expectedValue;
        }

        public ValidationResult PerformOn(MimeMessage message)
        {
            var result = new ValidationResult { Status = Result.Fail };
            var emailBody = message.TextBody ?? message.HtmlBody ?? "";

            if (string.IsNullOrEmpty(emailBody))
            {
                result.Actual = "Email body is empty";
                return result;
            }

            switch (_assertion)
            {
                case AssertionType.Contains:
                    if (emailBody.Contains(_expectedValue))
                    {
                        result.Status = Result.Pass;
                    }
                    result.Actual = emailBody;
                    break;
                case AssertionType.Notcontains:
                    if (!emailBody.Contains(_expectedValue))
                    {
                        result.Status = Result.Pass;
                    }
                    result.Actual = emailBody;
                    break;
                case AssertionType.Regexmatch:
                    var regex = new Regex(_expectedValue);
                    if (regex.IsMatch(emailBody))
                    {
                        result.Status = Result.Pass;
                    }
                    result.Actual = emailBody;
                    break;
            }

            return result;
        }
    }
}
