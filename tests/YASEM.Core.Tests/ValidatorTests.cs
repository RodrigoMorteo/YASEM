using Xunit;
using Moq;
using MimeKit;
using YASEM.Core.Validators;
using YASEM.Core.Models;
using System.Collections.Generic;

namespace YASEM.Core.Tests
{
    public class ValidatorTests
    {
        [Fact]
        public void XPathValidator_Should_Pass_When_Element_Contains_Expected_Value()
        {
            // Arrange
            var message = new MimeMessage();
            message.Body = new TextPart("html") { Text = "<html><body><h1>Hello World</h1></body></html>" };
            var validator = new XPathValidator("//h1", AssertionType.Contains, "Hello");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void XPathValidator_Should_Fail_When_Element_Does_Not_Contain_Expected_Value()
        {
            // Arrange
            var message = new MimeMessage();
            message.Body = new TextPart("html") { Text = "<html><body><h1>Hello World</h1></body></html>" };
            var validator = new XPathValidator("//h1", AssertionType.Contains, "Goodbye");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
        }

        [Fact]
        public void BulkValidator_Should_Pass_When_EachWith_Finds_All_Values()
        {
            // Arrange
            var message = new MimeMessage();
            message.Body = new TextPart("plain") { Text = "This email contains a cat, a dog, and a bird." };
            var validator = new BulkValidator(AssertionType.Eachwith, new List<string> { "cat", "dog", "bird" });

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void BulkValidator_Should_Fail_When_EachWith_Does_Not_Find_All_Values()
        {
            // Arrange
            var message = new MimeMessage();
            message.Body = new TextPart("plain") { Text = "This email contains a cat, a dog, and a bird." };
            var validator = new BulkValidator(AssertionType.Eachwith, new List<string> { "cat", "dog", "fish" });

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
        }

        [Fact]
        public void BulkValidator_Should_Pass_When_AnyWith_Finds_At_Least_One_Value()
        {
            // Arrange
            var message = new MimeMessage();
            message.Body = new TextPart("plain") { Text = "This email contains a cat, a dog, and a bird." };
            var validator = new BulkValidator(AssertionType.Anywith, new List<string> { "fish", "cat", "horse" });

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void BulkValidator_Should_Fail_When_AnyWith_Finds_No_Values()
        {
            // Arrange
            var message = new MimeMessage();
            message.Body = new TextPart("plain") { Text = "This email contains a cat, a dog, and a bird." };
            var validator = new BulkValidator(AssertionType.Anywith, new List<string> { "fish", "snake", "horse" });

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
        }
        [Fact]
        public void ContentValidator_Should_Pass_When_Regex_Matches_Content()
        {
            // Arrange
            var message = new MimeMessage();
            message.Body = new TextPart("plain") { Text = "This email contains the number 12345." };
            var validator = new ContentValidator(AssertionType.Regexmatch, @"\d{5}");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void AttachmentValidator_Should_Pass_When_Attachment_Exists()
        {
            // Arrange
            var message = new MimeMessage();
            var builder = new BodyBuilder();
            builder.Attachments.Add("test.txt", new byte[0]);
            message.Body = builder.ToMessageBody();
            var validator = new AttachmentValidator(AssertionType.Exists, string.Empty);

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void AttachmentValidator_Should_Fail_When_Attachment_Does_Not_Exist()
        {
            // Arrange
            var message = new MimeMessage();
            var validator = new AttachmentValidator(AssertionType.Exists, string.Empty);

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
        }

        [Fact]
        public void AttachmentValidator_Should_Pass_When_Attachment_Contains_Name()
        {
            // Arrange
            var message = new MimeMessage();
            var builder = new BodyBuilder();
            builder.Attachments.Add("test.txt", new byte[0]);
            message.Body = builder.ToMessageBody();
            var validator = new AttachmentValidator(AssertionType.Contains, "test");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void AttachmentValidator_Should_Fail_When_Attachment_Does_Not_Contain_Name()
        {
            // Arrange
            var message = new MimeMessage();
            var builder = new BodyBuilder();
            builder.Attachments.Add("test.txt", new byte[0]);
            message.Body = builder.ToMessageBody();
            var validator = new AttachmentValidator(AssertionType.Contains, "other");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
        }
    }
}