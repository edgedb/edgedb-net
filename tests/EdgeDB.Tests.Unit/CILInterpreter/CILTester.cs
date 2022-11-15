using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

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

                if(funcResult is Array array && interpretedResult is Array otherArr)
                {
                    Assert.AreEqual(array.Length, otherArr.Length);
                    for(int i = 0; i != array.Length; i++)
                    {
                        Assert.AreEqual(array.GetValue(i), otherArr.GetValue(i));
                    }
                }
                else
                    Assert.AreEqual(funcResult, interpretedResult);
            }

            if (TranslateToEdgeQL)
            {
                ExpressionTranslator.Translate(interpreted);
            }
        }
    }
}

