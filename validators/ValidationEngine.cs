using qualityassurance.tools.JSON;
namespace YASEM
{
    public class ValidationEngine
    {
        public List<Validator> TestSteps { get; set; } = new List<Validator>();
        public ValidationEngine(){}

        public void AddStep(TestStep step) 
        {
            //INFO 
            Console.WriteLine($"Step: {step.Description}" );
            //DEBUG
            Console.WriteLine($"Creating Validator of type {step.ValidationType} \"{step.Assertion}\", with expected value of \"{step.ExpectedValue}\".");
            TestSteps.Add(new Validator(step.Description, step.ValidationType, step.Assertion, step.ExpectedValue));
        }

        public List<ValidationResult> Execute()
        {
            List<ValidationResult> results = new List<ValidationResult>();
            foreach (Validator validator in TestSteps)
            {
                results.Add(Perform(validator));
            }
            return results;
        }

        private ValidationResult Perform(Validator validator)
        {
            ValidationResult result = new ValidationResult();

            return result;
        }

        private int CountAttachments()
        {
            //var attachments = message.BodyParts.OfType<MimePart> ().Where (part => !string.IsNullOrEmpty (part.FileName));
            return 0;
        }
    }
}