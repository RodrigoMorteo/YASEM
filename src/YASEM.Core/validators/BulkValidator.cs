using MimeKit;
using System.Collections.Generic;
using System.Linq;
using YASEM.Core.Models;

namespace YASEM.Core.Validators
{
    public class BulkValidator
    {
        private readonly AssertionType _assertion;
        private readonly List<string> _expectedValues;

        public BulkValidator(AssertionType assertion, List<string> expectedValues)
        {
            _assertion = assertion;
            _expectedValues = expectedValues;
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
                case AssertionType.Eachwith:
                    var missingValues = _expectedValues.Where(v => !emailBody.Contains(v)).ToList();
                    if (!missingValues.Any())
                    {
                        result.Status = Result.Pass;
                        result.Actual = "All expected values were found";
                    }
                    else
                    {
                        result.Actual = $"Missing values: {string.Join(", ", missingValues)}";
                    }
                    break;

                case AssertionType.Anywith:
                    if (_expectedValues.Any(emailBody.Contains))
                    {
                        result.Status = Result.Pass;
                        result.Actual = "At least one expected value was found";
                    }
                    else
                    {
                        result.Actual = "No expected values were found";
                    }
                    break;
            }

            return result;
        }
    }
}