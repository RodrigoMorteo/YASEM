namespace YASEM.Core.Validators
{
    public enum ValidationType
    {
        Field, //related to EmailField enum
        Content,
        Header,
        Xpath,
        Bulk,
        Attachment
    }
    public enum EmailField
    {
        Subject,
        Sender,
        Recipient,
        Cc,
        Bcc,
        Attachments,
        Body
    }
    public enum AssertionType
    {
        Contains,
        Notcontains,
        Equals,
        Notequals,
        Exists_once,
        Exists_many,
        Does_not_exist,
        Expression,
        Regexmatch,
        Eachwith,
        Anywith,
        Exists,
        Notexists
    }

    /// <summary>
    /// All possible Test Step and Test Case results. 
    /// </summary>
    public enum Result{
        Pass,
        Fail,
        Skip,
        Ignore
    }
}