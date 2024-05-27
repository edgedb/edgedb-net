namespace EdgeDB;

internal class EdgeQLOpAttribute : Attribute
{
    public EdgeQLOpAttribute(string v)
    {
        Operator = v;
    }

    public string Operator { get; }
}
