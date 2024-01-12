namespace EdgeDB
{
    internal class EdgeQLOpAttribute : Attribute
    {
        public string Operator { get; }

        public EdgeQLOpAttribute(string v)
        {
            Operator = v;
        }
    }
}
