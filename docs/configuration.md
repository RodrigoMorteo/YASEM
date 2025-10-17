# Configuration

This document details the configuration for YASEM, which is split into two main files: `appsettings.json` for application settings and a JSON file for test cases.

## `appsettings.json`

This file contains the configuration for the mail server and other application-level settings.

```json
{
  "MailSettings": {
    "Server": "imap.example.com",
    "Port": 993,
    "UseSsl": true,
    "Email": "user@example.com",
    "Password": {
      "EncryptedValue": "..."
    },
    "MailServerType": "imap",
    "IgnoreCertificateErrors": false,
    "Folder": "INBOX"
  }
}
```

### Encryption

Passwords can be encrypted using the `--encrypt` CLI option. The encryption key is stored in a separate file and passed to the tool using the `--key-path` option.

## Test Case JSON

This file defines the test case, including filters and validations. See `test-cases.md` for more details.