using System.Collections.Generic;

using MailKit;
using MimeKit;

using qualityassurance.tools;
using qualityassurance.tools.JSON;

namespace YASEM
{
    public class Validator
    {
        #region Fields
        public string Description { get; }
        public ValidationType Type { get; }
        public AssertionType Assertion { get; }
        public string ExpectedValue { get; }
        public string Expression { get; } = "";
        private EmailField Field = new EmailField();
        #endregion

        #region ERROR messages
            const string ERR_MSG_EMPTY_ASSERTION = "ERROR: Validators must always have a value in the Assertion field. Read the docs section XX and check your Test Steps in the JSON file.";
            const string ERR_MSG_INVALID_TYPE = "ERROR: Unknowon validator type specified";
            const string ERR_MSG_INVALID_FIELD_STRUCTURE = "Validator of types field and header must contain the name of the element of validation in the for ValidatorType:Element (i.e. field:subject or header:X-MAILER)";
        #endregion

    /// <summary>
    /// Create a validator from Strings loaded from the config file.
    /// </summary>
    /// <param name="description">Test Step description</param>
    /// <param name="type">Validation type</param>
    /// <param name="assertion">Assertion type</param>
    /// <param name="expectedValue">Value to to be comparing with</param>
    /// <exception cref="Exception">Returns an exception if invalid types and assertions are found</exception>
        public  Validator(string description, string type, string assertion, string expectedValue)
        {
            Description = description; //Asign test step description
            Type = EnumValidator.ValidateEnumValue<ValidationType>(StringUtils.FirstCharToUpperString(type.ToLower())); // Convert string to lower, set frist letter to upper case and assign Validtaion Type
            if(assertion.Length == 0)
            {
                throw new Exception($"{ERR_MSG_EMPTY_ASSERTION}"); //TODO: Add file and section in md docs.
            }
            switch(Type)
            {   //if validation type is either field or header, it must conform to the format field:subject / header:XMAILER
                case ValidationType.Field:
                case ValidationType.Header:
                    if(assertion.Contains(":"))
                    {
                        this.Expression = StringUtils.FirstCharToUpperString(assertion.Split(":")[0]); //Convert firts letter to upper case and assing the 1st part of the string to the expression
                        Assertion = EnumValidator.ValidateEnumValue<AssertionType>(StringUtils.FirstCharToUpperString(assertion.Split(":")[1])); //Convert first letter to upper case and assing the 2nd part of the string to the expression
                    }
                    else
                    {
                        throw new Exception($"{ERR_MSG_INVALID_FIELD_STRUCTURE}");
                    }
                    if(Type == ValidationType.Field){ //check if the field name is valid
                        this.Field = EnumValidator.ValidateEnumValue<EmailField>(this.Expression);
                    }
                break;
                case ValidationType.Xpath:
                    Assertion = AssertionType.Expression;
                    this.Expression = assertion; //TODO: check assertioin is a valid XPATH expression (Regex?)
                break;
                default:
                    throw new Exception($"{ERR_MSG_INVALID_TYPE} ({type})");
                break;
            }
            ExpectedValue = expectedValue; //set expected value for comparisson according to the assertionType
        }

        public ValidationResult Perform(MimeMessage message)
        {
            ValidationResult result = new ValidationResult();
            //INFO
            //Console.WriteLine($"\t\tPerforming validation {this.Type} {this.Assertion} {this.Expression}");
            switch(this.Type)
            {
                case ValidationType.Content:

                break;
                case ValidationType.Field:
                    FieldValidator fieldValidator = new FieldValidator(this);
                    result = fieldValidator.PerformOn(message);
                break;
                case ValidationType.Header:

                break;
                case ValidationType.Xpath:

                break;
            }
            //DEBUG
                //Console.WriteLine($"\t\tExpected: {this.ExpectedValue}, Actual: {result.Actual}");
            return result;
        }


    }

    #region ValidationResult
    public class ValidationResult 
    {
        public string Actual {get; set;}  = "" ; //Defaults to empty string
        public Result Status {get; set;}
    }
    #endregion
}