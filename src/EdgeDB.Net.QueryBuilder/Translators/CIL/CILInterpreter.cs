using EdgeDB.CIL.Interpreters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CIL
{
    internal class CILInterpreter
    {
        public static ConcurrentDictionary<OpCodeType, BaseCILInterpreter> Interpreters;

        static CILInterpreter()
        {
            var interpreters = typeof(BaseCILInterpreter).Assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(BaseCILInterpreter)));
            var dict = new Dictionary<OpCodeType, BaseCILInterpreter>();

            foreach(var interpreter in interpreters)
            {
                if (interpreter.IsAbstract)
                    continue;

                var inst = (BaseCILInterpreter)Activator.CreateInstance(interpreter)!;
                foreach(var opcode in inst.SupportedCodes)
                {
                    if (!dict.TryAdd(opcode, inst))
                        throw new AmbiguousMatchException($"Duplicate interpreter found for {opcode}");
                }
            }

            Interpreters = new ConcurrentDictionary<OpCodeType, BaseCILInterpreter>(dict);
        }

        public static Expression<T> InterpretFunc<T>(T func)
            where T : Delegate
        {
            var reader = new ILReader(func.Method);
            var locals = reader.MethodBody.LocalVariables.Select(x => Expression.Variable(x.LocalType, $"local_{x.LocalIndex}")).ToArray();
            var functionParameters = func.Method.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            // TODO: static doesn't have instance?
            var interpreterParameters = functionParameters.Prepend(
                Expression.Parameter(func.Method.DeclaringType ?? throw new NotSupportedException("No type for 'this' could be found"))
            ).ToArray();

            var stack = new InterpreterStack();
            var context = new CILInterpreterContext(func.Target, reader, stack, locals, interpreterParameters);

            while(reader.ReadNext(out var instruction))
            {
                // interpret the raw instuction
                var expression = Interpret(instruction, context);

                if (expression is DefaultExpression d && d.Type == typeof(void))
                    continue;

                // refine the expression
                expression = ExpressionRefiner.RefineExpression(expression, new RefiningContext(context));

                // push to the stack
                stack.Push(expression);
            }

            // TODO: check stack for any remaining expressions that wern't consumed

            var tree = stack.GetTree();

            var body = tree.Count == 1
                ? tree.First()
                : Expression.Block(locals, tree.Reverse());

            try
            {
                return Expression.Lambda<T>(body, functionParameters);
            }
            catch(Exception x)
            {
                _ = x;
                throw;
            }
        }

        internal static Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            if(!Interpreters.TryGetValue(instruction.OpCodeType, out var interpreter))
                throw new NotSupportedException($"Failed to find interpreter for op code {instruction.OpCodeType}");

            return interpreter.Interpret(instruction, context);
        }
    }
}
