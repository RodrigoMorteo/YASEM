# YASEM Configuration

## The YASEM.JSON Schema

### General Test Case Configuration

### Mail Options

### Log Options

### Reportiong Options

### Test Step Definition

#### Validation Types

#### Assetion Types



### JSON Security Warning
```diff
Note that anything in the JSON file is on plain text and thus succeptible of malicious use if the file leaks outside your testing environment into the public internet (i.e. adding the JSON files or the containing folder into a public git repository). 
```
It is recommended that you **NEVER upload any YASEM's JSON file into a public repository** as it not only contains potentially sensitive information of your company (via the test cases, but also the access credentials for the email server you connect to).

See the [Proposed Additions-JSON Encrpytion]() section for suggestions on how to mitigate this issue.


Note that for PoP3 emails, there is no concept of "folder" as for the imap protocol, thus for pop3 accounts YASEM will perform the configured test steps to all emails found in the INBOX. You can avoid this by using impa instead and preemptively moving the emails intended for validation to a folder and then configure the folder name in the MailOptions.Folder property. **Note that this MailOptions.Folder ALWAYS set to INBOX by default for pop3 connections**.

See the [Proposed Additions-Email Filtering]() section for suggestions on how to customize YASEM for better testing experience when there are just too much emails in your folder or when there are many types of emails that are succeptible for testing (e.g. for personalized email campaings).

### The Gmail Exceptions

Over the years Google has changed the "default" behavior of  //TODO: state Gmail Special Cases

[How do I access GMail using MailKit?](http://www.mimekit.net/docs/html/Frequently-Asked-Questions.htm#GMailAccess).


## Proposed Additions

## JSON Encrpytion
One could add an extra security layer by encrypting/decrypting JSON strings (e.g. by adding the prefix "encripted" to the sensitive string in your JSON) and having your added encryption module decrypt the values with that prefix in the key name. This would help in case your test cases are leaked from your testing environment into a public repository **(as long as the decryption key is not also leaked)**, but, as the password still needs to be decrypted in memory, the program will still be succeptible to memory reading/leaking attacks. If you want the key file in the same folder as your git's project, be sure to add an exception for it in your .gitignore file.

Alternatively, the whole JSON file could be encrypted, but this may add extra complexity and more effort when debugging a failing test or when deploying test cases into the intended environments (as you must be sure to upload the encrypted JSONs and provide the decryption key to each environment). 

Additionally, when considering shared (a.k.a. private) key encryption algorithms (as opossed to public key encryption algorithms), bare in mind that key management and distribution can quicly become a very complex process as the number of keys, users and environments that need them grows.

Finally, when using public key encryption algorithms one can gain the advantage of "asseting" the authenticity of the script's author and contents, analog to how web browsers assert the identtiy of a web site secured with https.

## Email Filtering
When testers share a common email account or (as mentioned in section XXX //TODO: add section ) when the email account has a huge histroy of emails, YASEM may take a lot of time applying the configured test steps to each of the emails found. To help refine the number of emails retrieved by YASEM for testing, the MailKit email client libraries (see acknowledgements [3] in docs folder) have features that can be implemented as filtering options and setup as an additional section in the JSON condiguratioin file.

One option is to modify the EmailConnector class to use a combination of certain email fields (e.g. [sender, addresses, subjec, reply-to, etc](http://www.mimekit.net/docs/html/Frequently-Asked-Questions.htm#AddressHeaders).) or other message [message properties](http://www.mimekit.net/docs/html/Properties_T_MimeKit_MimeMessage.htm) to filter email messages that will be added to the list that will be passed in the YSEM.CS Main method for Test Case Execution.
