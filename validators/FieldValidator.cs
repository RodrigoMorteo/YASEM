using MailKit;
using MimeKit;
using System.Text.RegularExpressions;

namespace YASEM
{
    class FieldValidator 
    {
        #region MESSAGES
        const string ERR_MSG_INVALID_FIELD = "ERROR: Invalid field type passed. Check the documentation for available field types and correct the field name:";
        const string ERR_MSG_INVALID_ASSERTION = "ERROR: Invalid assertion type passed. Check the documentation for available assertions.";
        #endregion
        Validator testStep;
        public FieldValidator(Validator validator)
        {
            this.testStep = validator;
        }

        public ValidationResult PerformOn(MimeMessage message)
        {
            ValidationResult result = new ValidationResult();
            
            switch(testStep.Expression) //get the actual value from the selected email field
            {
                case "Subject":
                    result.Actual = message.Subject;
                break;
                case "Sender":
                    result.Actual = message.Sender.ToString();
                break;
                case "Recipient":
                    result.Actual = message.To.ToString();
                break;
                case "Cc":
                    result.Actual = message.Cc.ToString();
                break;
                case "Bcc":
                    result.Actual = message.Bcc.ToString();
                break;
                case "Attachments":
                    result.Actual = message.Attachments.Count().ToString();
                break;
                case "Body":
                    result.Actual = message.TextBody;
                break;
                default:
                    throw new Exception($"{ERR_MSG_INVALID_FIELD} {testStep.Expression}"); //Constructor in the Validator class should have taken care of safe checking the validator.Expression to the allowed values. If this excepiton is thrown check the aforementioned class.  
            }

            switch(this.testStep.Assertion) //perform the selected assertion with the expected value on the actual value
            {
                case AssertionType.Contains:
                    result.Status = Contains(this.testStep.ExpectedValue, result.Actual);
                break;
                case AssertionType.Exists_once:
                    result.Status = ExistsOnce(this.testStep.ExpectedValue, result.Actual);
                break;
                case AssertionType.Exists_many:
                    result.Status = ExistsMany(this.testStep.ExpectedValue, result.Actual);
                break;
                case AssertionType.Does_not_exist:
                    result.Status = DoesNotExist(this.testStep.ExpectedValue, result.Actual);
                break;
                // case AssertionType.Expression: //Field validations do not implement Expression assertions as they are reserved for the XPATH validation type only.
                default:
                throw new Exception($"{ERR_MSG_INVALID_ASSERTION}. Check the Assertion field with the value \"{testStep.Assertion}\" in your JSON file."); //Constructor in the Validator class should have taken care of safe chacking the assertion type. If this wxception is thrown check the Validation constructor
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