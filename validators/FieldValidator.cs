using MailKit;
using MimeKit;

namespace YASEM
{
    class FieldValidator 
    {
        #region MESSAGES
        const string ERR_MSG_INVALID_FIELD ="ERROR: Invalid field type passed. Check the documentation for available field types and correct the field name:";
        #endregion

        Validator testStep;
        public FieldValidator(Validator validator)
        {
            this.testStep = validator;
        }

        public ValidationResult PerformOn(MimeMessage message)
        {
            ValidationResult result = new ValidationResult();
            
            switch(testStep.Expression)
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
            result.Status = result.Actual.Contains(this.testStep.ExpectedValue)? Result.Pass: Result.Fail; //TODO: Add other validation Types (currently only CONTAIS is implemented)
            return result;
        }

    private bool Contains() 
    {
        return false;
    } 

    private bool ExistsOnce()
    {
        return false;
    }

    private bool ExistsMany()
    {
        return false;
    }
    private bool DoesNotExist() 
    {
        return false;
    }


        
    }
}