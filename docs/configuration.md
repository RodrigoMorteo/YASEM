# YASEM Configuration

## Configuration

YASEM uses two primary configuration files: `appsettings.json` for application-wide settings and a separate JSON file for test case definitions.

### `appsettings.json` Configuration

This file manages general application settings, including mail server connection details, reporting metadata, and logging configuration. Sensitive values can be encrypted.

```json
{
  "AppConfig": {
    "Mail": {
      "Host": "outlook.office365.com",
      "Port": 993,
      "UseSsl": true,
      "Folder": "INBOX",
      "PollingIntervalSeconds": 10,
      "MarkFilteredEmailsAsRead": false,
      "Credentials": {
        "Address": "user@example.com",
        "Password": "your-plain-text-password"
      }
    },
    "Report": {
      "SystemUnderTest": "My Application",
      "Version": "1.2.3",
      "Author": "QA Team",
      "DateTimeFormat": "yyyyMMdd_HHmmss"
    },
    "Logging": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
}
```

### Test Case Definition (JSON)

The test logic is defined in a separate JSON file, specified via the `--json-path` CLI option. All properties must be inside a main `testParameters` object.

**Global Parameters:**
*   `testName`: A descriptive name for the test scenario.
*   `filterMode`: How multiple filters are combined.
    *   `"inclusive"`: An email is selected if it matches **any** filter (logical OR).
    *   `"exclusive"`: An email is selected only if it matches **all** filters (logical AND).
*   `markAsRead`: `"yes"` or `"no"`. Overrides the global property for this specific test run.
*   `moveFilteredToFolder`: Name of a mailbox folder to move emails to after validation.

**Filtering Logic (`filters` array):**
An array of filter objects. Each object has `type`, `name`, and `value`.

| Filter `type` | Description                                       | `name` options                                                                                  | `value` format                                                                                             |
|---------------|---------------------------------------------------|-------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------|
| `field`         | Filter by standard email fields.                  | `subject`, `recipients` (all), `to`, `from`, `reply-to`, `cc`, `bcc`, `sentDate`, `receivedDate`, `size` | String. For `date` and `size`, can be prefixed with a Comparison Term.                |
| `header`        | Filter by a specific email header.                | The name of the header (e.g., `X-MAILER`).                                                      | String.                                                                                                    |
| `body`          | Filter by searching for text within the email\'s body. | (Not used, can be an empty string).                                                             | String to search for.                                                                                      |
| `flag`          | Filter by the email\'s status flags.               | `isRead`, `isFlagged`, `isRecent`.                                                              | `"yes"` or `"no"`.

**Comparison Terms:**
For `date` and `size` filters, the `value` string should be in the format `term:value` (e.g., `less_than:5000`). If no term is provided, `equals` is the default.
*   `equals`, `greater_than`, `greater_equal`, `less_than`, `less_equal`, `not_equal`.
*   Date format for values is `mmm/dd/yyyy` (e.g., `jun/16/2023`).

**Validation Logic (`validations` array):**
An array of validation objects. Each object represents a test step with `description`, `type`, `assertion`, and `value`.

| Validation `type` | Description                                                              |
|-------------------|--------------------------------------------------------------------------|
| `field`           | Applies assertion to `subject`, `recipients`, `sender`, `reply-to`, `cc`, `bcc`, or `attachments`. |
| `header`          | Applies assertion to a specific email header.                            |
| `content`         | Applies assertion to the email\'s body content.                           |
| `xpath`           | Evaluates an XPath expression against the HTML part of the email. The expression itself is placed in the `assertion` field. |
| `bulk`            | Applies an assertion to the entire collection of filtered emails. |

**Assertion Types:**
The `assertion` field specifies the condition to check.

*   **Single-Mail Assertions** (for `field`, `header`, `content` types):
    *   `contains`: The target text contains the string in `value`.
    *   `exists_once`: The string in `value` appears exactly once in the target text.
    *   `exists_many`: The string in `value` appears more than once in the target text.
    *   `not_exists`: The string in `value` does not appear in the target text.

*   **XPath Validation**:
    *   The `assertion` field holds the XPath 1.0 expression to be evaluated.
    *   The `value` field holds the expected result of the XPath evaluation.
    *   The tool extracts the HTML content from the email, parses it (handling malformed HTML), and evaluates the expression.
    *   Example: To verify that exactly one link to 'example.com' exists, you would use:
        *   `"type": "xpath"`
        *   `"assertion": "count(//a[@href='http://example.com'])"`
        *   `"value": "1"

*   **Bulk Assertions** (for `bulk` type):
    *   `each_with`: For **each** string in the `value` list, at least one email in the filtered set must contain that string. The test passes only if this is true for all strings in the list.
    *   `any_with`: At least one email in the filtered set must contain at least one of the strings from the `value` list.

**Value Formatting**:
The format of the `value` field depends on the validation `type`.

*   For `type: "field"`: The value must be a string in the format `"field_name:expected_value"`.
    *   Example: `"value": "subject:Important Notice"`
*   For `type: "bulk"`: The value must be a standard JSON array of strings.
    *   Example: `"value": ["Coupon Code", "Special Offer"]`
*   For all other types, `value` is a simple string.

### JSON Security Warning
```diff
Note that anything in the JSON file is on plain text and thus succeptible of malicious use if the file leaks outside your testing environment into the public internet (i.e. adding the JSON files or the containing folder into a public git repository).
```
It is recommended that you **NEVER upload any YASEM\'s JSON file into a public repository** as it not only contains potentially sensitive information of your company (via the test cases, but also the access credentials for the email server you connect to).



Note that for PoP3 emails, there is no concept of "folder" as for the imap protocol, thus for pop3 accounts YASEM will perform the configured test steps to all emails found in the INBOX. You can avoid this by using impa instead and preemptively moving the emails intended for validation to a folder and then configure the folder name in the MailOptions.Folder property. **Note that this MailOptions.Folder ALWAYS set to INBOX by default for pop3 connections**.

See the [Proposed Additions-Email Filtering]() section for suggestions on how to customize YASEM for better testing experience when there are just too much emails in your folder or when there are many types of emails that are succeptible for testing (e.g. for personalized email campaings).





