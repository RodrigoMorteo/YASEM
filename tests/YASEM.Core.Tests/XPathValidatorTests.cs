using Xunit;
using MimeKit;
using YASEM.Core.Validators;
using YASEM.Core.Models;

namespace YASEM.Core.Tests
{
    public class XPathValidatorTests
    {
        private MimeMessage CreateMessage(string htmlBody)
        {
            var message = new MimeMessage();
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlBody;
            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        [Fact]
        public void PerformOn_Equals_ShouldPass()
        {
            // Arrange
            var message = CreateMessage("<html><body><h1>Hello World</h1></body></html>");
            var validator = new XPathValidator("/html/body/h1", AssertionType.Equals, "Hello World");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void PerformOn_Equals_ShouldFail()
        {
            // Arrange
            var message = CreateMessage("<html><body><h1>Hello Universe</h1></body></html>");
            var validator = new XPathValidator("/html/body/h1", AssertionType.Equals, "Hello World");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
        }

        [Fact]
        public void PerformOn_Contains_ShouldPass()
        {
            // Arrange
            var message = CreateMessage("<html><body><p>Some text here</p></body></html>");
            var validator = new XPathValidator("/html/body/p", AssertionType.Contains, "text");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void PerformOn_NotContains_ShouldPass()
        {
            // Arrange
            var message = CreateMessage("<html><body><p>Some text here</p></body></html>");
            var validator = new XPathValidator("/html/body/p", AssertionType.Notcontains, "universe");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void PerformOn_Notequals_ShouldPass()
        {
            // Arrange
            var message = CreateMessage("<html><body><h1>Hello World</h1></body></html>");
            var validator = new XPathValidator("/html/body/h1", AssertionType.Notequals, "Hello Universe");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Pass, result.Status);
        }

        [Fact]
        public void PerformOn_NoElementFound_ShouldFail()
        {
            // Arrange
            var message = CreateMessage("<html><body><h1>Hello World</h1></body></html>");
            var validator = new XPathValidator("/html/body/p", AssertionType.Equals, "Hello World");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
            Assert.Equal("No element found for the given XPath expression", result.Actual);
        }

        [Fact]
        public void PerformOn_NoHtmlBody_ShouldFail()
        {
            // Arrange
            var message = new MimeMessage();
            var validator = new XPathValidator("/html/body/h1", AssertionType.Equals, "Hello World");

            // Act
            var result = validator.PerformOn(message);

            // Assert
            Assert.Equal(Result.Fail, result.Status);
            Assert.Equal("Email has no HTML body", result.Actual);
        }
    }
}