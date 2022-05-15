namespace EdgeDB.Operators
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class EquivalentOperator : Attribute
    {
        public readonly IEdgeQLOperator Operator;

        public EquivalentOperator(Type operatorType)
        {
            if (!operatorType.GetInterfaces().Contains(typeof(IEdgeQLOperator)))
                throw new ArgumentException($"Cannot use {operatorType} as a equivalent operator because it doesn't implement the IEdgeQLOperator interface");

            Operator = (IEdgeQLOperator)Activator.CreateInstance(operatorType)!;
        }
    }
}
