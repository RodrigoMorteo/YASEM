using NUnit.Framework;
using YASEM.Core.Models;
using YASEM.Core.Utilities;
using YASEM;

namespace YASEM.Tests
{
    [TestFixture]
    public class ValidatorTests
    {
        [Test]
        public void Constructor_WithValidFieldStep_ShouldSucceed()
        {
            var step = new TestStep
            {
                Description = "Valid Field Test",
                ValidationType = "field",
                Field = "Subject",
                Assertion = "Contains",
                ExpectedValue = "Hello"
            };

            Assert.DoesNotThrow(() => new Validator(step));
        }

        [Test]
        public void Constructor_WithFieldStepAndMissingField_ShouldThrowArgumentException()
        {
            var step = new TestStep
            {
                Description = "Invalid Field Test",
                ValidationType = "field",
                Field = null, // Missing field
                Assertion = "Contains",
                ExpectedValue = "Hello"
            };

            var ex = Assert.Throws<ArgumentException>(() => new Validator(step));
            Assert.That(ex.Message, Does.Contain("must specify a 'field' property"));
        }

        [Test]
        public void Constructor_WithValidXPathStep_ShouldSetPropertiesCorrectly()
        {
            var step = new TestStep
            {
                Description = "Valid XPath Test",
                ValidationType = "xpath",
                Assertion = "count(//a)", // The XPath expression goes here
                ExpectedValue = "5"
            };

            // Act
            var validator = new Validator(step);

            // Assert
            Assert.That(validator.Type, Is.EqualTo(ValidationType.Xpath));
            Assert.That(validator.Assertion, Is.EqualTo(AssertionType.Expression));
            Assert.That(validator.Expression, Is.EqualTo("count(//a)"));
        }

        [Test]
        public void Constructor_WithInvalidValidationType_ShouldThrowEnumValidationException()
        {
            var step = new TestStep { ValidationType = "non_existent_type" };

            Assert.Throws<EnumValidationException>(() => new Validator(step));
        }

        [Test]
        public void Constructor_WithValidHeaderStep_ShouldSucceed()
        {
            var step = new TestStep
            {
                Description = "Valid Header Test",
                ValidationType = "header",
                Field = "X-Custom-Header",
                Assertion = "Exists_once",
                ExpectedValue = "some-value"
            };

            Assert.DoesNotThrow(() => new Validator(step));
        }

        [Test]
        public void Constructor_WithHeaderStepAndMissingField_ShouldThrowArgumentException()
        {
            var step = new TestStep
            {
                Description = "Invalid Header Test",
                ValidationType = "header",
                Field = null, // Missing header name
                Assertion = "Contains",
                ExpectedValue = "some-value"
            };

            var ex = Assert.Throws<ArgumentException>(() => new Validator(step));
            Assert.That(ex.Message, Does.Contain("must specify a 'field' property for the header name"));
        }

        [Test]
        public void Constructor_WithEmptyAssertion_ShouldThrowException()
        {
            var step = new TestStep { ValidationType = "content", Assertion = "" };

            var ex = Assert.Throws<Exception>(() => new Validator(step));
            Assert.That(ex.Message, Does.Contain("Validators must always have a value in the Assertion field"));
        }
    }
}