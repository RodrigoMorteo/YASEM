using MailKit;
using MimeKit;
using System.Text.RegularExpressions;

namespace YASEM.Core.Validators
{
    public class PartValidator 
    {
        #region MESSAGES
        const string ERR_MSG_INVALID_PART = "ERROR: Invalid part type passed. Check the documentation for available part types and correct the part name:";
        const string ERR_MSG_INVALID_ASSERTION = "ERROR: Invalid assertion type passed. Check the documentation for available assertions.";
        #endregion
        private readonly MessagePart _part;
        private readonly AssertionType _assertion;
        private readonly string _expectedValue;

        public PartValidator(MessagePart part, AssertionType assertion, string expectedValue)
        {
            _part = part;
            _assertion = assertion;
            _expectedValue = expectedValue;
        }

        public ValidationResult PerformOn(MimeMessage message)
        {
            ValidationResult result = new ValidationResult();
            
            switch(_part) //get the actual value from the selected email part
            {
                case MessagePart.Body:
                    result.Actual = message.TextBody ?? string.Empty;
                break;
                case MessagePart.Attachments:
                    result.Actual = message.Attachments.Count().ToString();
                break;
                default:
                    throw new InvalidOperationException($"{ERR_MSG_INVALID_PART} {_part}");
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