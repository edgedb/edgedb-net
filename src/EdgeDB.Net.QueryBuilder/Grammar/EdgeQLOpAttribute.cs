namespace EdgeDB;

internal class EdgeQLOpAttribute(string v) : Attribute
{
    public string Operator { get; } = v;
}
