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
            var interpreters = typeof(BaseCILInterpreter).Assembly.GetTypes().Where(x => x.BaseType == typeof(BaseCILInterpreter));
            var dict = new Dictionary<OpCodeType, BaseCILInterpreter>();

            foreach(var interpreter in interpreters)
            {
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

            var tree = new Stack<Expression>();
            var members = new Stack<MemberInfo>();
            var context = new CILInterpreterContext(reader, tree, members, locals, functionParameters);

            while(reader.ReadNext(out var instruction))
            {
                var expression = Interpret(instruction, context);
                if (expression is DefaultExpression d && d.Type == typeof(void))
                    continue;
                tree.Push(expression);
            }

            // TODO: check stack for any remaining expressions that wern't consumed

            var body = tree.Count == 1
                ? tree.Pop()
                : Expression.Block(tree);

            return Expression.Lambda<T>(body, context.IsTailCall, functionParameters);
        }

        internal static Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            if(!Interpreters.TryGetValue(instruction.OpCodeType, out var interpreter))
                throw new NotSupportedException($"Failed to find interpreter for op code {instruction.OpCodeType}");

            return interpreter.Interpret(instruction, context);
        }

        //public static Expression<T> Parse<T>(Delegate func)
        //{
        //    var reader = new ILReader(func.Method);
        //    var locals = reader.MethodBody.LocalVariables.Select(x => Expression.Variable(x.LocalType, $"local_{x.LocalIndex}")).ToArray();
        //    var builderArgs = func.Method.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name));
        //    var funcArgs = builderArgs.ToArray();
        //    if (!func.Method.IsStatic)
        //        builderArgs = builderArgs.Prepend(Expression.Parameter(func.Method.DeclaringType!, "this"));
        //    return Expression.Lambda<T>(BuildExpression(ref reader, builderArgs.ToArray(), locals), funcArgs); // TODO: parameters
        //}

        //private static Expression BuildExpression(ref ILReader reader, ParameterExpression[] arguments, ParameterExpression[] locals)
        //{
        //    var expressionStack = new Stack<Expression>();
        //    var referenceStack = new Stack<object?>();
        //    var branchs = new Dictionary<Label, Expression>();
        //    bool isTailCall = false;
            
        //    while (reader.ReadNext(out var instruction))
        //    {
        //        switch ((OpCodeType)instruction.OpCode.Value)
        //        {
        //            #region Numerical
        //            case OpCodeType.Add_ovf or OpCodeType.Add_ovf_un:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.AddChecked(left, right));
        //                }
        //                break;
        //            case OpCodeType.Sub_ovf or OpCodeType.Sub_ovf_un:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.SubtractChecked(left, right));
        //                }
        //                break;
        //            case OpCodeType.Mul_ovf or OpCodeType.Mul_ovf_un:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.MultiplyChecked(left, right));
        //                }
        //                break;
        //            case OpCodeType.Div or OpCodeType.Div_un:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Divide(left, right));
        //                }
        //                break;
        //            case OpCodeType.Add:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Add(left, right));
        //                }
        //                break;
        //            case OpCodeType.Sub:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Subtract(left, right));
        //                }
        //                break;
        //            case OpCodeType.Mul:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Multiply(left, right));
        //                }
        //                break;
        //            case OpCodeType.Neg:
        //                {
        //                    var value = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Negate(value));
        //                }
        //                break;
        //            case OpCodeType.Rem or OpCodeType.Rem_un:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Modulo(left, right));
        //                }
        //                break;
        //            #endregion
        //            #region Bitwise
        //            case OpCodeType.And:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.And(left, right));
        //                }
        //                break;
        //            case OpCodeType.Or:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Or(left, right));
        //                }
        //                break;
        //            case OpCodeType.Xor:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.ExclusiveOr(left, right));
        //                }
        //                break;
        //            case OpCodeType.Not:
        //                {
        //                    var value = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Not(value));
        //                }
        //                break;
        //            case OpCodeType.Shl:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.LeftShift(left, right));
        //                }
        //                break;
        //            case OpCodeType.Shr or OpCodeType.Shr_un:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.RightShift(left, right));
        //                }
        //                break;
        //            case OpCodeType.Box:
        //                {
        //                    expressionStack.Push(expressionStack.Pop());
        //                }
        //                break;
        //            #endregion
        //            #region Branching
        //            case OpCodeType.Br or OpCodeType.Br_s:
        //                {
        //                    throw new Exception("TODO: BR(_S)");
        //                }
        //            case OpCodeType.Brtrue:
        //            case OpCodeType.Brtrue_s:
        //                {
        //                    var condition = expressionStack.Pop();
        //                    var position = reader.MarkPosition();
        //                    var trueLabel = (Label)instruction.Oprand!;

        //                    if (!reader.ReadNext(out var target))
        //                        throw new Exception("No instruction after a branch");

        //                    if (!branchs.TryGetValue(position, out var elseExpression))
        //                        elseExpression = branchs[position] = BuildExpression(ref reader, arguments, locals);
        //                    if(!branchs.TryGetValue(trueLabel, out var trueExpression))
        //                    {
        //                        reader.Seek(trueLabel);
        //                        trueExpression = branchs[trueLabel] = BuildExpression(ref reader, arguments, locals);
        //                    }

        //                    expressionStack.Push(Expression.Condition(condition, trueExpression, elseExpression));
        //                }
        //                break;
        //            case OpCodeType.Switch:
        //                {
        //                    var value = expressionStack.Pop();
        //                    if (!reader.ReadNext(out _))
        //                        throw new Exception("no switch body");

        //                    var switchLocation = reader.MarkPosition();

        //                    var branches = GetBranches((int)instruction.Oprand!, ref reader);
        //                    SwitchCase[] cases = new SwitchCase[branches.Count];
        //                    for(int i = 0; i != branches.Count; i++)
        //                    {
        //                        var kvp = branches.ElementAt(i);
        //                        if(!branchs.TryGetValue(kvp.Value, out var caseExpression))
        //                        {
        //                            reader.Seek(kvp.Value);
        //                            caseExpression = BuildExpression(ref reader, arguments, locals);
        //                        }
        //                        cases[i] = Expression.SwitchCase(caseExpression, Expression.Constant(kvp.Key));
        //                    }

        //                    Expression.Switch(value, cases);
        //                    reader.Seek(switchLocation);
        //                }
        //                break;
        //            case OpCodeType.Leave or OpCodeType.Leave_s:
        //                {
        //                    throw new Exception("TODO: leave labels");
        //                    //reader.Seek(instruction.OprandAs<Label>()!);
        //                    //expressionStack.Push(Expression.Goto(Expression.Label()));
        //                }
        //            case OpCodeType.Endfilter:
        //                {
        //                    throw new Exception("TODO: endfilter");
        //                }
        //            case OpCodeType.Endfinally:
        //                {
        //                    throw new Exception("TODO: endfinally");
        //                }
        //            #endregion
        //            #region Method calls
        //            case OpCodeType.Ldftn:
        //                referenceStack.Push(instruction.OprandAsMethod());
        //                break;
        //            case OpCodeType.Ldvirtftn:
        //                throw new Exception("TODO: ldvirtfnt");
        //            case OpCodeType.Calli:
        //                throw new Exception("TODO: Calli");
        //            case OpCodeType.Call or OpCodeType.Callvirt:
        //                {
        //                    var method = instruction.OprandAsMethod();
        //                    var args = method.GetParameters().Select((p, i) =>
        //                    {
        //                        var arg = expressionStack.Pop();
        //                        if (arg.Type != p.ParameterType)
        //                            return Expression.TypeAs(arg, p.ParameterType);
        //                        return arg;
        //                    }).Reverse().ToArray();

        //                    var inst = method.IsStatic ? null : expressionStack.Pop();

        //                    if(method.GetCustomAttribute<CompilerGeneratedAttribute>() != null && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
        //                    {
        //                        // property get/set
        //                        var propName = method.Name[..4];
        //                        var prop = method.DeclaringType!.GetProperty(propName)!;
        //                        expressionStack.Push(args.Any() ? Expression.Property(inst, prop, args) : Expression.Property(inst, prop));
        //                    }
        //                    else
        //                        expressionStack.Push(Expression.Call(inst, (MethodInfo)method, args));
        //                }
        //                break;
        //            case OpCodeType.Jmp:
        //                throw new Exception("TODO: jmp");
        //            case OpCodeType.Cpblk:
        //                throw new Exception("TODO: cpblk");
        //            case OpCodeType.Cpobj:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Assign(left, right));
        //                }
        //                break;
        //            case OpCodeType.Initblk:
        //                throw new Exception("TODO: initblk");
        //            case OpCodeType.Initobj:
        //                throw new Exception("TODO: initobj");
        //            #endregion
        //            #region Load/Store
        //            case OpCodeType.Sizeof:
        //                {
        //                    var type = instruction.OprandAsType();
        //                    expressionStack.Push(Expression.Constant(Marshal.SizeOf(type)));
        //                }
        //                break;
        //            case OpCodeType.Localloc:
        //                throw new Exception("TODO: localloc");
        //            case OpCodeType.Ldsflda or OpCodeType.Ldflda:
        //                throw new Exception("TODO: ldsflda & ldflda");
        //            case OpCodeType.Ldloca or OpCodeType.Ldloca_s:
        //                throw new Exception("TODO: ldloca & ldloca_s");
        //            case OpCodeType.Ldarga or OpCodeType.Ldarga_s:
        //                throw new Exception("TODO: ldarga & ldarga_s");
        //            case OpCodeType.Ldarg_0:
        //                expressionStack.Push(arguments[0]);
        //                break;
        //            case OpCodeType.Ldarg_1:
        //                expressionStack.Push(arguments[1]);
        //                break;
        //            case OpCodeType.Ldarg_2:
        //                expressionStack.Push(arguments[2]);
        //                break;
        //            case OpCodeType.Ldarg_3:
        //                expressionStack.Push(arguments[3]);
        //                break;
        //            case OpCodeType.Ldc_i4_m1:
        //                expressionStack.Push(Expression.Constant(-1));
        //                break;
        //            case OpCodeType.Ldc_i4_0:
        //                expressionStack.Push(Expression.Constant(0));
        //                break;
        //            case OpCodeType.Ldc_i4_1:
        //                expressionStack.Push(Expression.Constant(1));
        //                break;
        //            case OpCodeType.Ldc_i4_2:
        //                expressionStack.Push(Expression.Constant(2));
        //                break;
        //            case OpCodeType.Ldc_i4_3:
        //                expressionStack.Push(Expression.Constant(3));
        //                break;
        //            case OpCodeType.Ldc_i4_4:
        //                expressionStack.Push(Expression.Constant(4));
        //                break;
        //            case OpCodeType.Ldc_i4_5:
        //                expressionStack.Push(Expression.Constant(5));
        //                break;
        //            case OpCodeType.Ldc_i4_6:
        //                expressionStack.Push(Expression.Constant(6));
        //                break;
        //            case OpCodeType.Ldc_i4_7:
        //                expressionStack.Push(Expression.Constant(7));
        //                break;
        //            case OpCodeType.Ldc_i4_8:
        //                expressionStack.Push(Expression.Constant(8));
        //                break;
        //            case OpCodeType.Ldc_i4 or OpCodeType.Ldc_i4_s:
        //                {
        //                    var value = (int)instruction.Oprand!;
        //                    expressionStack.Push(Expression.Constant(value));
        //                }
        //                break;
        //            case OpCodeType.Ldc_i8:
        //                {
        //                    var value = (long)instruction.Oprand!;
        //                    expressionStack.Push(Expression.Constant(value));
        //                }
        //                break;
        //            case OpCodeType.Ldc_r8:
        //                {
        //                    var value = (double)instruction.Oprand!;
        //                    expressionStack.Push(Expression.Constant(value));
        //                }
        //                break;
        //            case OpCodeType.Ldc_r4:
        //                {
        //                    var value = (float)instruction.Oprand!;
        //                    expressionStack.Push(Expression.Constant(value));
        //                }
        //                break;
        //            case OpCodeType.Ldfld or OpCodeType.Ldsfld:
        //                {
        //                    var field = instruction.OprandAsField();
        //                    var instance = field.IsStatic ? null : expressionStack.Pop();
        //                    expressionStack.Push(Expression.Field(instance, field));
        //                }
        //                break;
        //            case OpCodeType.Ldlen:
        //                {
        //                    var arr = expressionStack.Pop();
        //                    expressionStack.Push(Expression.ArrayLength(arr));
        //                }
        //                break;
        //            case OpCodeType.Ldloc_0:
        //                expressionStack.Push(locals[0]);

        //                break;
        //            case OpCodeType.Ldloc_1:
        //                expressionStack.Push(locals[1]);

        //                break;
        //            case OpCodeType.Ldloc_2:
        //                expressionStack.Push(locals[2]);

        //                break;
        //            case OpCodeType.Ldloc_3:
        //                expressionStack.Push(locals[3]);

        //                break;
        //            case OpCodeType.Ldloc or OpCodeType.Ldloc_s:
        //                {
        //                    var local = reader.MethodBody.LocalVariables[(int)instruction.Oprand!];
        //                    expressionStack.Push(Expression.Variable(local.LocalType, $"local_{local.LocalIndex}"));
        //                }
        //                break;
        //            case OpCodeType.Stloc_0:
        //                {
        //                    var val = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Assign(locals[0], val));
        //                }
        //                break;
        //            case OpCodeType.Stloc_1:
        //                {
        //                    var val = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Assign(locals[1], val));
        //                }
        //                break;
        //            case OpCodeType.Stloc_2:
        //                {
        //                    var val = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Assign(locals[2], val));
        //                }
        //                break;
        //            case OpCodeType.Stloc_3:
        //                {
        //                    var val = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Assign(locals[3], val));
        //                }
        //                break;
        //            case OpCodeType.Stloc:
        //                {
        //                    var val = expressionStack.Pop();
        //                    var indx = (short)instruction.Oprand!;
        //                    expressionStack.Push(Expression.Assign(locals[indx], val));
        //                }
        //                break;
        //            case OpCodeType.Starg or OpCodeType.Starg_s:
        //                {
        //                    var val = expressionStack.Pop();
        //                    var indx = (short)instruction.Oprand!;
        //                    expressionStack.Push(Expression.Assign(arguments[indx], val));
        //                }
        //                break;
        //            case OpCodeType.Stobj:
        //                {
        //                    var val = expressionStack.Pop();
        //                    var target = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Assign(target, val));
        //                }
        //                break;
        //            case OpCodeType.Stsfld or OpCodeType.Stfld:
        //                {
        //                    var field = instruction.OprandAsField();
        //                    var val = expressionStack.Pop();
        //                    var inst = field.IsStatic ? null : expressionStack.Pop();
        //                    expressionStack.Push(Expression.Assign(Expression.Field(inst, field), val));
        //                }
        //                break;
        //            case OpCodeType.Ldnull:
        //                expressionStack.Push(Expression.Constant(null));
        //                break;
        //            case OpCodeType.Ldstr:
        //                expressionStack.Push(Expression.Constant(instruction.OprandAsString()));
        //                break;
        //            case OpCodeType.Ldtoken:
        //                expressionStack.Push(Expression.Constant(instruction.ParseOprand()));
        //                break;
        //            case OpCodeType.Nop:
        //                break;
        //            case OpCodeType.Pop:
        //                expressionStack.Pop();
        //                break;
        //            case OpCodeType.Ret:
        //                break;
        //                //throw new Exception("TODO: ret");
        //            case OpCodeType.Ldelem
        //                or OpCodeType.Ldelem_ref
        //                or OpCodeType.Ldelem_i
        //                or OpCodeType.Ldelem_i1
        //                or OpCodeType.Ldelem_i2
        //                or OpCodeType.Ldelem_i4
        //                or OpCodeType.Ldelem_i8
        //                or OpCodeType.Ldelem_u1
        //                or OpCodeType.Ldelem_u2
        //                or OpCodeType.Ldelem_u4
        //                or OpCodeType.Ldelem_r4
        //                or OpCodeType.Ldelem_r8:
        //                {
        //                    var indx = expressionStack.Pop();
        //                    var arr = expressionStack.Pop();
        //                    expressionStack.Push(Expression.ArrayIndex(arr, indx));
        //                }
        //                break;
        //            case OpCodeType.Stelem
        //                or OpCodeType.Stelem_ref
        //                or OpCodeType.Stelem_i
        //                or OpCodeType.Stelem_i1
        //                or OpCodeType.Stelem_i2
        //                or OpCodeType.Stelem_i4
        //                or OpCodeType.Stelem_i8
        //                or OpCodeType.Stelem_r4
        //                or OpCodeType.Stelem_r8:
        //                {
        //                    var val = expressionStack.Pop();
        //                    var indx = expressionStack.Pop();
        //                    var arr = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Assign(Expression.ArrayAccess(arr, indx), val));
        //                }
        //                break;
        //            case OpCodeType.Ldind_ref
        //                or OpCodeType.Ldind_i
        //                or OpCodeType.Ldind_i1
        //                or OpCodeType.Ldind_i2
        //                or OpCodeType.Ldind_i4
        //                or OpCodeType.Ldind_i8
        //                or OpCodeType.Ldind_r4
        //                or OpCodeType.Ldind_r8
        //                or OpCodeType.Ldind_u1
        //                or OpCodeType.Ldind_u2
        //                or OpCodeType.Ldind_u4:
        //                break; // don't do anything for address getting
        //            case OpCodeType.Stind_ref
        //                or OpCodeType.Stind_i
        //                or OpCodeType.Stind_i1
        //                or OpCodeType.Stind_i2
        //                or OpCodeType.Stind_i4
        //                or OpCodeType.Stind_i8
        //                or OpCodeType.Stind_r4
        //                or OpCodeType.Stind_r8:
        //                {
        //                    var val = expressionStack.Pop();
        //                    var target = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Assign(target, val));
        //                }
        //                break;
        //            case OpCodeType.Arglist:
        //                throw new Exception("TODO: arglist");
        //            #endregion
        //            #region Comparison
        //            case OpCodeType.Isinst:
        //                expressionStack.Push(Expression.TypeIs(expressionStack.Pop(), instruction.OprandAsType()));
        //                break;
        //            case OpCodeType.Ceq:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Equal(left, right));
        //                }
        //                break;
        //            case OpCodeType.Beq or OpCodeType.Beq_s:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Equal(left, right));
        //                    goto case OpCodeType.Brtrue;
        //                }
        //            case OpCodeType.Bge or OpCodeType.Bge_un or OpCodeType.Bge_un_s or OpCodeType.Bge_s:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.GreaterThanOrEqual(left, right));
        //                    goto case OpCodeType.Brtrue;
        //                }
        //            case OpCodeType.Ble_s or OpCodeType.Ble or OpCodeType.Ble_un or OpCodeType.Ble_un_s:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.LessThanOrEqual(left, right));
        //                    goto case OpCodeType.Brtrue;
        //                }
        //            case OpCodeType.Blt or OpCodeType.Blt_s or OpCodeType.Blt_un or OpCodeType.Blt_un_s:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.LessThan(left, right));
        //                    goto case OpCodeType.Brtrue;
        //                }
        //            case OpCodeType.Bgt or OpCodeType.Bgt_s or OpCodeType.Bgt_un or OpCodeType.Bgt_un_s:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.GreaterThan(left, right));
        //                    goto case OpCodeType.Brtrue;
        //                }
        //                break;
        //            case OpCodeType.Bne_un or OpCodeType.Bne_un_s:
        //                {
        //                    var right = expressionStack.Pop();
        //                    var left = expressionStack.Pop();
        //                    expressionStack.Push(Expression.NotEqual(left, right));
        //                    goto case OpCodeType.Brtrue;
        //                }
        //                break;
        //            case OpCodeType.Brfalse or OpCodeType.Brfalse_s:
        //                {
        //                    var left = expressionStack.Pop();
        //                    // TODO: value type defaults?
        //                    expressionStack.Push(Expression.Equal(left, Expression.Constant(left.Type.IsValueType ? default : null, left.Type)));
        //                }
        //                break;
        //            case OpCodeType.Ckfinite:
        //                {
        //                    var arg = expressionStack.Pop();
        //                    var checker = arg.Type == typeof(double)
        //                        ? typeof(double).GetMethod("IsInfinity")
        //                        : typeof(float).GetMethod("IsInfinity");

        //                    var body = Expression.Throw(
        //                        Expression.New(
        //                            typeof(ArithmeticException).GetConstructor(new[] { typeof(string) })!, 
        //                            Expression.Constant("Value is not finite")));

        //                    expressionStack.Push(Expression.IfThen(Expression.Call(null, checker!, arg), body));
        //                }
        //                break;
        //            case OpCodeType.Constrained_:
        //                throw new Exception("TODO: constrained.");
        //            #endregion
        //            #region Conversion
        //            case OpCodeType.Castclass:
        //                {
        //                    var value = expressionStack.Pop();
        //                    var targetType = instruction.OprandAsType();
        //                    if (value.Type == targetType)
        //                        expressionStack.Push(value);

        //                    expressionStack.Push(CastTo(value, targetType));
        //                }
        //                break;
        //            case OpCodeType.Conv_i:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), UIntPtr.Size == 4 ? typeof(int) : typeof(long)));
        //                break;
        //            case OpCodeType.Conv_i1:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(sbyte)));
        //                break;
        //            case OpCodeType.Conv_i2:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(short)));
        //                break;
        //            case OpCodeType.Conv_i4:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(int)));
        //                break;
        //            case OpCodeType.Conv_i8:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(long)));
        //                break;
        //            case OpCodeType.Conv_ovf_i or OpCodeType.Conv_ovf_i_un:
        //                expressionStack.Push(CastToChecked(expressionStack.Pop(), UIntPtr.Size == 4 ? typeof(int) : typeof(long)));
        //                break;
        //            case OpCodeType.Conv_ovf_i1 or OpCodeType.Conv_ovf_i1_un:
        //                expressionStack.Push(CastToChecked(expressionStack.Pop(), typeof(sbyte)));
        //                break;
        //            case OpCodeType.Conv_ovf_i2 or OpCodeType.Conv_ovf_i2_un:
        //                expressionStack.Push(CastToChecked(expressionStack.Pop(), typeof(short)));
        //                break;
        //            case OpCodeType.Conv_ovf_i4 or OpCodeType.Conv_ovf_i4_un:
        //                expressionStack.Push(CastToChecked(expressionStack.Pop(), typeof(int)));
        //                break;
        //            case OpCodeType.Conv_ovf_i8 or OpCodeType.Conv_ovf_i8_un:
        //                expressionStack.Push(CastToChecked(expressionStack.Pop(), typeof(long)));
        //                break;
        //            case OpCodeType.Conv_r4 or OpCodeType.Conv_r_un:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(float)));
        //                break;
        //            case OpCodeType.Conv_r8:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(double)));
        //                break;
        //            case OpCodeType.Conv_u:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(nuint)));
        //                break;
        //            case OpCodeType.Conv_u1:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(byte)));
        //                break;
        //            case OpCodeType.Conv_u2:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(ushort)));
        //                break;
        //            case OpCodeType.Conv_u4:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(uint)));
        //                break;
        //            case OpCodeType.Conv_u8:
        //                expressionStack.Push(CastTo(expressionStack.Pop(), typeof(ulong)));
        //                break;
        //            case OpCodeType.Dup:
        //                expressionStack.Push(expressionStack.Peek());
        //                break;
        //            case OpCodeType.Newarr:
        //                {
        //                    var bounds = expressionStack.Pop();
        //                    var type = instruction.OprandAsType();
        //                    expressionStack.Push(Expression.NewArrayBounds(type, new[] { bounds }));
        //                }
        //                break;
        //            case OpCodeType.Newobj:
        //                {
        //                    var constructor = (ConstructorInfo)instruction.OprandAsMethod();
        //                    Expression[] args = new Expression[constructor.GetParameters().Length];
        //                    for(int i = args.Length - 1; i >= 0; i--)
        //                        args[i] = expressionStack.Pop();
        //                    expressionStack.Push(Expression.New(constructor, args));
        //                }
        //                break;
        //            case OpCodeType.Rethrow:
        //                expressionStack.Push(Expression.Rethrow());
        //                break;
        //            case OpCodeType.Tail_:
        //                isTailCall = true;
        //                break;
        //            case OpCodeType.Unaligned_:
        //                throw new Exception("TODO: unaligned");
        //            case OpCodeType.Unbox or OpCodeType.Unbox_any:
        //                {
        //                    var type = instruction.OprandAsType();
        //                    var value = expressionStack.Pop();
        //                    expressionStack.Push(Expression.Unbox(value, type));
        //                }
        //                break;
        //            default:
        //                throw new Exception($"Could not find parser for {(OpCodeType)instruction.OpCode.Value}");
        //                #endregion
        //        }
        //    }

        //    return Expression.Block(expressionStack.Reverse());
        //}

        //private static Expression CastTo(Expression value, Type targetType)
        //{
        //    return value is ConstantExpression constantExpression
        //        ? Expression.Constant(Convert.ChangeType(constantExpression.Value, targetType))
        //        : Expression.Convert(value, targetType);
        //}

        //private static Expression CastToChecked(Expression value, Type targetType)
        //{
        //    return value is ConstantExpression constantExpression
        //        ? Expression.Constant(Convert.ChangeType(constantExpression.Value, targetType))
        //        : Expression.Convert(value, targetType);
        //}

        //private static Dictionary<int, Label> GetBranches(int position, ref ILReader reader)
        //{
        //    var count = BitConverter.ToInt32(reader.PeekBytes(position, 4));
        //    var branches = new Dictionary<int, Label>();
        //    for(int i = 0; i < count; i++)
        //    {
        //        var offset = BitConverter.ToInt32(reader.PeekBytes(position + 4 + i * 4, 4));
        //        branches.Add(offset, new(position + 4 * count + offset));
        //    }
        //    return branches;
        //}
    }
}
