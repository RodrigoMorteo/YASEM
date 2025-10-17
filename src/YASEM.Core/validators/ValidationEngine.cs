using MimeKit;
using YASEM.Core.Interfaces;
using YASEM.Core.Models;
using YASEM.Core.Exceptions;
using System.Resources;

namespace YASEM.Core.Validators
{
    public class ValidationEngine : IValidationEngine
    {
        #region Messages
        const string INFO_MSG_FINISHED ="Finished Executing Test Case Scenarios.";
        #endregion
        public List<Validator> TestSteps { get; set; } = new List<Validator>();
        private readonly ResourceManager _resourceManager;

        public ValidationEngine(List<TestStep> steps){
            _resourceManager = new ResourceManager("YASEM.Core.Resources.ErrorMessages", typeof(ValidationEngine).Assembly);
            foreach (var step in steps)
                {
                    AddStep(step);
                }         
        }
        public List<ValidationResult> Execute(List<MimeMessage> messages)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            foreach( MimeMessage message in messages)
            {
                Console.WriteLine($"Starting validations on email with subject: {message.Subject.ToString()}");
                foreach (Validator validator in TestSteps)
                {
                    ValidationResult res = validator.Perform(message);
                    results.Add(res);
                    //INFO
                    Console.WriteLine($"\t[{res.Status.ToString().ToUpper()}] Step: {validator.Description}");//INFO
                    if(res.Status != Result.Pass)
                        //TODO: WARN
                        Console.WriteLine($"\t\tExpected: {validator.ExpectedValue} Actual: {res.Actual}");
                }
            }
            //TODO: INFO log
            //Console.WriteLine($"{INFO_MSG_FINISHED}");
            
            return results;
        }
        private void AddStep(TestStep step) 
        {
            //TODO: INFO 
            Console.WriteLine($"Step: {step.Description}" );
            //TODO: DEBUG
            Console.WriteLine($"\tCreating Validator of type {step.ValidationType} for field '{step.Field ?? "N/A"}' with assertion '{step.Assertion}' and expected value '{step.ExpectedValue}'.");
            try
            {
                TestSteps.Add(new Validator(step));
            }
            catch (NotSupportedException ex)
            {
                throw new ValidationException(_resourceManager.GetString("ValidationError") ?? "A validation error occurred.", ex);
            }
        }
    }

    public class ValidationEngineFactory : IValidationEngineFactory
    {
        public IValidationEngine Create(List<TestStep> steps)
        {
            return new ValidationEngine(steps);
        }
    }
}