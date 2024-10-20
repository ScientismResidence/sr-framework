namespace Framework.Console.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class HelpAttribute : System.Attribute
{
    public string Help { get; }

    public HelpAttribute(string help)
    {
        Help = help;
    }
}