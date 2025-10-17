using HtmlAgilityPack;
using MimeKit;
using YASEM.Core.Models;

namespace YASEM.Core.Validators
{
    public class XPathValidator
    {
        private readonly string _xpathExpression;
        private readonly AssertionType _assertion;
        private readonly string _expectedValue;

        public XPathValidator(string xpathExpression, AssertionType assertion, string expectedValue)
        {
            _xpathExpression = xpathExpression;
            _assertion = assertion;
            _expectedValue = expectedValue;
        }

        public ValidationResult PerformOn(MimeMessage message)
        {
            var result = new ValidationResult { Status = Result.Fail };
            var htmlBody = message.HtmlBody;

            if (string.IsNullOrEmpty(htmlBody))
            {
                result.Actual = "Email has no HTML body";
                return result;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlBody);

            var navigator = doc.CreateNavigator();
            var node = navigator.SelectSingleNode(_xpathExpression);

            if (node == null)
            {
                result.Actual = "No element found for the given XPath expression";
                return result;
            }

            var actualValue = node.Value;
            result.Actual = actualValue;

            switch (_assertion)
            {
                case AssertionType.Contains:
                    if (actualValue.Contains(_expectedValue))
                    {
                        result.Status = Result.Pass;
                    }
                    break;
                case AssertionType.Notcontains:
                    if (!actualValue.Contains(_expectedValue))
                    {
                        result.Status = Result.Pass;
                    }
                    break;
                case AssertionType.Equals:
                    if (actualValue.Equals(_expectedValue))
                    {
                        result.Status = Result.Pass;
                    }
                    break;
                case AssertionType.Notequals:
                    if (!actualValue.Equals(_expectedValue))
                    {
                        result.Status = Result.Pass;
                    }
                    break;
            }

            return result;
        }
    }
}