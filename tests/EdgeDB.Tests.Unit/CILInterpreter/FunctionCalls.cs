using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace EdgeDB.Tests.Unit
{
    public partial class CILTester
    {
        [TestMethod]
        public void TestFunctionCalls()
        {
            CompileExpression = true;
            RunAndCompareExpression = true;

            // basic func calls
            TestFunction(() => InstanceVoid());
            TestFunction(() => InstanceArgumentVoid(123));
            TestFunction(() => InstanceReturn());
            TestFunction(() => InstanceArgReturn(123));
            TestFunction(() => StaticVoid());
            TestFunction(() => StaticArgument(123));
            TestFunction(() => StaticReturn());
            TestFunction(() => StaticArgReturn(123));

            // supplied root func args
            TestFunction((int x) => InstanceArgReturn(x), 123);
            TestFunction((int x) => InstanceArgumentVoid(x), 123);
            TestFunction((int x) => StaticArgReturn(x), 123);
            TestFunction((int x) => StaticArgument(x), 123);

            // other class method
            TestFunction(() => DummyClass.DummyStaticMethod());

            // external instance
            var inst = new DummyClass();
            TestFunction(() => inst.DummyInstanceMethod());
            TestFunction((int x) => inst.DummyReturnMethod(x), 123);
        }

        public void InstanceVoid() { }
        public void InstanceArgumentVoid(int x) { }
        public int InstanceReturn() { return 123; }
        public int InstanceArgReturn(int x) { return x + 1; }
        public static void StaticVoid() { }
        public static void StaticArgument(int x) { }
        public static int StaticReturn() { return 123; }
        public static int StaticArgReturn(int x) { return x + 1; }

        private class DummyClass
        {
            public static void DummyStaticMethod() { }

            public void DummyInstanceMethod() { }

            public int DummyReturnMethod(int x)
            {
                return x * 2;
            }
        }
    }
}

