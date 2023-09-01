namespace YASEM
{
    public enum ValidationType
    {
        field, //related to EmailField enum
        content,
        header,
        xpath
    }
    public enum EmailField
    {
        subject,
        sender,
        recipient,
        cc,
        bcc,
        attachments,
        body
    }
    public enum AssertionType
    {
        contains,
        exists_once,
        exists_many,
        does_not_exist,
        expression
    }
}