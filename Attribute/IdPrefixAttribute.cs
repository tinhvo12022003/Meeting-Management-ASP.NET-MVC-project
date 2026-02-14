namespace MeetingManagement.Attr.IdPrefix;

[AttributeUsage(AttributeTargets.Class)]
public class IdPrefixAttribute : Attribute
{
    public string Prefix { get; }

    public IdPrefixAttribute(string prefix)
    {
        Prefix = prefix;
    }
}