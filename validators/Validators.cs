using System.Collections.Generic;

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
        private string Expression = "";
        private EmailField Field = new EmailField();
        #endregion

        public  Validator(string description, string type, string assertion, string expectedValue)
        {
            Description = description; //Asign test step description
            Type = EnumValidator.ValidateEnumValue<ValidationType>(type.ToLower()); // Assign Validtaion Type
            switch(Type)
            {   //if validation type is either field or header, it must conform to the format field:subject / header:XMAILER
                case ValidationType.field:
                case ValidationType.header:
                    if(assertion.Contains(":"))
                    {
                        this.Expression = assertion.Split(":")[0]; //assing the 1st part of the string to the expression
                        Assertion = EnumValidator.ValidateEnumValue<AssertionType>(assertion.Split(":")[1]); //assing the 2nd part of the string to the expression
                    }
                    else
                    {
                        throw new Exception("Validator of types field and header must contain the name of the element of validation in the for ValidatorType:Element (i.e. field:subject or header:X-MAILER)");
                    }
                    if(Type == ValidationType.field){ //check if the field name is valid
                        this.Field = EnumValidator.ValidateEnumValue<EmailField>(this.Expression);
                    }
                break;
                case ValidationType.xpath:
                    Assertion = AssertionType.expression;
                    this.Expression = assertion;
                break;
            }
            ExpectedValue = expectedValue; //set expected value for comparisson according to the assertionType
        }
    }

    public class ValidationResult 
    {

    }

    

}