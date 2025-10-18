
using Xunit;
using MimeKit;
using YASEM.Core.Validators;
using YASEM.Core.Models;

namespace YASEM.Core.Tests
{
    public class FieldValidatorTests
    {
        [Fact]
        public void PerformOn_WithValidSender_ShouldPass()
        {
            // Arrange
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Test Sender", "test@example.com"));
            var validator = new FieldValidator(EmailField.Sender, AssertionType.Contains, "test@example.com");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void PerformOn_WithInvalidSender_ShouldFail()
        {
            // Arrange
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Test Sender", "test@example.com"));
            var validator = new FieldValidator(EmailField.Sender, AssertionType.Contains, "other@example.com");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
        }

        [Fact]
        public void PerformOn_WithValidSubject_ShouldPass()
        {
            // Arrange
            var message = new MimeMessage();
            message.Subject = "Test Subject";
            var validator = new FieldValidator(EmailField.Subject, AssertionType.Contains, "Test");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void PerformOn_WithNullMessage_ShouldFail()
        {
            // Arrange
            var validator = new FieldValidator(EmailField.Subject, AssertionType.Contains, "Test");

            // Act
            var result = validator.PerformOn(null);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
        }
    }
}
