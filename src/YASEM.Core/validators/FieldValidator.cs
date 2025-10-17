using MailKit;
using MimeKit;
using System.Text.RegularExpressions;

namespace YASEM.Core.Validators
{
    class FieldValidator 
    {
        #region MESSAGES
        const string ERR_MSG_INVALID_FIELD = "ERROR: Invalid field type passed. Check the documentation for available field types and correct the field name:";
        const string ERR_MSG_INVALID_ASSERTION = "ERROR: Invalid assertion type passed. Check the documentation for available assertions.";
        #endregion
        private readonly EmailField _field;
        private readonly AssertionType _assertion;
        private readonly string _expectedValue;

        public FieldValidator(EmailField field, AssertionType assertion, string expectedValue)
        {
            _field = field;
            _assertion = assertion;
            _expectedValue = expectedValue;
        }

        public ValidationResult PerformOn(MimeMessage message)
        {
            ValidationResult result = new ValidationResult();
            
            // Switch on the strongly-typed enum, not a fragile string.
            switch(_field) //get the actual value from the selected email field
            {
                case EmailField.Subject:
                    result.Actual = message.Subject ?? string.Empty;
                break;
                case EmailField.Sender: // Fall-through to From, as From is what users typically mean by sender.
                case EmailField.From:
                    result.Actual = message.From?.ToString() ?? string.Empty;
                break;
                case EmailField.Recipient:
                    result.Actual = message.To?.ToString() ?? string.Empty;
                break;
                case EmailField.Cc:
                    result.Actual = message.Cc?.ToString() ?? string.Empty;
                break;
                case EmailField.Bcc:
                    result.Actual = message.Bcc?.ToString() ?? string.Empty;
                break;
                case EmailField.Attachments:
                    result.Actual = message.Attachments.Count().ToString();
                break;
                case EmailField.Body:
                    result.Actual = message.TextBody ?? string.Empty;
                break;
                default:
                    // This case should be unreachable if the Validator constructor does its job.
                    throw new InvalidOperationException($"{ERR_MSG_INVALID_FIELD} {_field}");
            }

            switch(_assertion) //perform the selected assertion with the expected value on the actual value
            {
                case AssertionType.Contains:
                    result.Status = Contains(_expectedValue, result.Actual);
                break;
                case AssertionType.Exists_once:
                    result.Status = ExistsOnce(_expectedValue, result.Actual);
                break;
                case AssertionType.Exists_many:
                    result.Status = ExistsMany(_expectedValue, result.Actual);
                break;
                case AssertionType.Does_not_exist:
                    result.Status = DoesNotExist(_expectedValue, result.Actual);
                break;
                // case AssertionType.Expression: //Field validations do not implement Expression assertions as they are reserved for the XPATH validation type only.
                default:
                throw new InvalidOperationException($"{ERR_MSG_INVALID_ASSERTION}. Check the Assertion field with the value \"{_assertion}\" in your JSON file.");
            }

            return result;
        }

        private Result Contains(string expected, string actual) 
        {
            return actual.Contains(expected)? Result.Pass: Result.Fail; 
        } 

        private Result ExistsOnce(string expected, string actual) 
        {   
            return Regex.Matches(actual, expected).Count == 1? Result.Pass: Result.Fail;
        }

        private Result ExistsMany(string expected, string actual) 
        {
            return Regex.Matches(actual, expected).Count > 1? Result.Pass: Result.Fail;
        }
        private Result DoesNotExist(string expected, string actual) 
        {
            return Regex.Matches(actual, expected).Count == 0? Result.Pass: Result.Fail;
        }
        
    }
}