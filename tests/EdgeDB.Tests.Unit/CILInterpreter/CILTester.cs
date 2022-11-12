using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace EdgeDB.Tests.Unit
{
    [TestClass]
    public partial class CILTester
    {
        public bool CompileExpression { get; set; } = true;
        public bool RunAndCompareExpression { get; set; }
        public bool TranslateToEdgeQL { get; set; }

        public void TestFunction<TDelegate>(TDelegate func, params object?[] args)
            where TDelegate : Delegate
        {
            // interpret the function
            var interpreted = CIL.CILInterpreter.InterpretFunc(func);

            if (CompileExpression)
            {
                try
                {
                    interpreted.Compile();
                }
                catch(Exception x)
                {
                    _ = x;
                    throw;
                }
            }

            if (RunAndCompareExpression)
            {
                var funcResult = func.DynamicInvoke(args);
                var interpretedResult = interpreted.Compile().DynamicInvoke(args);

                Assert.AreEqual(funcResult, interpretedResult);
            }

            if (TranslateToEdgeQL)
            {
                ExpressionTranslator.Translate(interpreted);
            }
        }
    }
}

