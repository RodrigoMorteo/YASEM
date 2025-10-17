using System.Collections.Generic;

using MailKit;
using MimeKit;

using YASEM.Core.Utilities;
using YASEM.Core.Models;
using YASEM.Core.Exceptions;
using System.Resources;
using System.Text.Json;

namespace YASEM.Core.Validators
{
        
    #region ValidationResult
    public class ValidationResult 
    {
        public string Actual {get; set;}  = "" ; //Defaults to empty string
        public Result Status {get; set;}
    }
    #endregion
    public class Validator
    {
        #region Fields
        public string Description { get; }
        public ValidationType Type { get; }
        public AssertionType Assertion { get; }
        public string? ExpectedValue { get; }
        public List<string> ExpectedValues { get; } = new List<string>();
        public string Expression { get; } = "";
        private EmailField Field = new EmailField();
        private readonly ResourceManager _resourceManager;
        #endregion

        #region ERROR messages
        const string ERR_MSG_EMPTY_ASSERTION = "ERROR: Validators must always have a value in the Assertion field. Read the docs section XX and check your Test Steps in the JSON file.";
        const string ERR_MSG_INVALID_TYPE = "ERROR: Unknowon validator type specified";
        const string ERR_MSG_INVALID_FIELD_STRUCTURE = "Validator of types field and header must contain the name of the element of validation in the for ValidatorType:Element (i.e. field:subject or header:X-MAILER)";
        #endregion

        public Validator(TestStep step)
        {
            if (string.IsNullOrEmpty(step.ValidationType))
            {
                throw new InvalidConfigurationException("The 'validationType' property is missing or empty in one of your test steps in the JSON file.");
            }

            _resourceManager = new ResourceManager("YASEM.Core.Resources.ErrorMessages", typeof(Validator).Assembly);
            Description = step.Description;
            Type = EnumValidator.ValidateEnumValue<ValidationType>(StringUtils.FirstCharToUpperString(step.ValidationType.ToLower()));
            ExpectedValue = step.ExpectedValue?.ToString();

            if (string.IsNullOrEmpty(step.Assertion))
            {
                throw new ValidationException($"{ERR_MSG_EMPTY_ASSERTION}"); //TODO: Add file and section in md docs.
            }

            switch (Type)
            {
                case ValidationType.Field:
                    if (string.IsNullOrEmpty(step.Field))
                    {
                        throw new ArgumentException("Test steps of type 'field' must specify a 'field' property.", nameof(step.Field));
                    }
                    this.Expression = StringUtils.FirstCharToUpperString(step.Field); // e.g., "Subject"
                    this.Field = EnumValidator.ValidateEnumValue<EmailField>(this.Expression);
                    this.Assertion = EnumValidator.ValidateEnumValue<AssertionType>(StringUtils.FirstCharToUpperString(step.Assertion));
                    break;
                case ValidationType.Header:
                    if (string.IsNullOrEmpty(step.Field))
                    {
                        throw new ArgumentException("Test steps of type 'header' must specify a 'field' property for the header name.", nameof(step.Field));
                    }
                    this.Expression = step.Field; // The header name, e.g., "X-Mailer"
                    this.Assertion = EnumValidator.ValidateEnumValue<AssertionType>(StringUtils.FirstCharToUpperString(step.Assertion));
                    break;
                case ValidationType.Xpath:
                    // Per description.md, for XPath, the expression is the assertion.
                    this.Assertion = AssertionType.Expression;
                    this.Expression = step.Assertion; // The XPath expression itself.
                    break;
                case ValidationType.Content:
                    this.Assertion = EnumValidator.ValidateEnumValue<AssertionType>(StringUtils.FirstCharToUpperString(step.Assertion));
                    break;
                case ValidationType.Bulk:
                    this.Assertion = EnumValidator.ValidateEnumValue<AssertionType>(StringUtils.FirstCharToUpperString(step.Assertion));
                    if (step.ExpectedValue != null)
                    {
                        try
                        {
                            var json = step.ExpectedValue.ToString();
                            if (!string.IsNullOrEmpty(json))
                            {
                                var values = JsonSerializer.Deserialize<List<string>>(json);
                                if (values != null)
                                {
                                    ExpectedValues = values;
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            throw new InvalidConfigurationException($"For bulk validation, the 'expectedValue' must be a valid JSON array of strings. Details: {ex.Message}", ex);
                        }
                    }
                    break;
                case ValidationType.Attachment:
                    this.Assertion = EnumValidator.ValidateEnumValue<AssertionType>(StringUtils.FirstCharToUpperString(step.Assertion));
                    break;
                default:
                    throw new ValidationException($"{ERR_MSG_INVALID_TYPE} ({step.ValidationType})");
            }
        }

        public ValidationResult Perform(MimeMessage message)
        {
            ValidationResult result = new ValidationResult();
            //INFO
            //Console.WriteLine($" 		Performing validation {this.Type} {this.Assertion} {this.Expression}");
            switch (this.Type)
            {
                case ValidationType.Content:
                    if (this.ExpectedValue != null)
                    {
                        ContentValidator contentValidator = new ContentValidator(this.Assertion, this.ExpectedValue);
                        result = contentValidator.PerformOn(message);
                    }
                    break;
                case ValidationType.Field:
                    if (this.ExpectedValue != null)
                    {
                        FieldValidator fieldValidator = new FieldValidator(this.Field, this.Assertion, this.ExpectedValue);
                        result = fieldValidator.PerformOn(message);
                    }
                    break;
                case ValidationType.Header:

                    break;
                case ValidationType.Xpath:
                    if (this.ExpectedValue != null)
                    {
                        XPathValidator xpathValidator = new XPathValidator(this.Expression, this.Assertion, this.ExpectedValue);
                        result = xpathValidator.PerformOn(message);
                    }
                    break;
                case ValidationType.Bulk:
                    BulkValidator bulkValidator = new BulkValidator(this.Assertion, this.ExpectedValues);
                    result = bulkValidator.PerformOn(message);
                    break;
                case ValidationType.Attachment:
                    if (this.ExpectedValue != null)
                    {
                        AttachmentValidator attachmentValidator = new AttachmentValidator(this.Assertion, this.ExpectedValue);
                        result = attachmentValidator.PerformOn(message);
                    }
                    break;
            }
            //DEBUG
            //Console.WriteLine($" 		Expected: {this.ExpectedValue}, Actual: {result.Actual}");
            return result;
        }
    }
}