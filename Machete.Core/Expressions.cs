using System;
using System.Linq.Expressions;

namespace Machete.Core
{
    public static class Expressions
    {
        public static readonly ConstantExpression IDynamicNull = Expression.Constant(null, Reflection.IDynamicMemberInfo.Type);
        public static readonly ConstantExpression NullableBoolNull = Expression.Constant(null, typeof(bool?));
        public static readonly ConstantExpression NullableBoolTrue = Expression.Constant(true, typeof(bool?));
        public static readonly ConstantExpression NullableBoolFalse = Expression.Constant(false, typeof(bool?));

        public static readonly ParameterExpression Environment = Expression.Parameter(Reflection.IEnvironmentMemberInfo.Type, "environment");
        public static readonly ParameterExpression Args = Expression.Parameter(Reflection.IArgsMemberInfo.Type, "args");

        public static readonly MemberExpression Context = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.Context);
        public static readonly MemberExpression ThisBinding = Expression.Property(Context, Reflection.IExecutionContextMemberInfo.ThisBinding);
        public static readonly MemberExpression VariableEnviroment = Expression.Property(Context, Reflection.IExecutionContextMemberInfo.VariableEnviroment);
        public static readonly MemberExpression LexicalEnviroment = Expression.Property(Context, Reflection.IExecutionContextMemberInfo.LexicalEnviroment);
        public static readonly MemberExpression Strict = Expression.Property(Context, Reflection.IExecutionContextMemberInfo.Strict);
        public static readonly MemberExpression CurrentFunction = Expression.Property(Context, Reflection.IExecutionContextMemberInfo.CurrentFunction);

        public static readonly MemberExpression Output = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.Output);
        public static readonly MemberExpression GlobalEnvironment = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.GlobalEnvironment);
        public static readonly MemberExpression EmptyArgs = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.EmptyArgs);
        public static readonly MemberExpression Undefined = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.Undefined);
        public static readonly MemberExpression Null = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.Null);
        public static readonly MemberExpression True = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.True);
        public static readonly MemberExpression False = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.False);
        public static readonly MemberExpression GlobalObject = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.GlobalObject);
        public static readonly MemberExpression ObjectConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.ObjectConstructor);
        public static readonly MemberExpression ObjectPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.ObjectPrototype);
        public static readonly MemberExpression FunctionConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.FunctionConstructor);
        public static readonly MemberExpression FunctionPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.FunctionPrototype);
        public static readonly MemberExpression ArrayConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.ArrayConstructor);
        public static readonly MemberExpression ArrayPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.ArrayPrototype);
        public static readonly MemberExpression StringConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.StringConstructor);
        public static readonly MemberExpression StringPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.StringPrototype);
        public static readonly MemberExpression BooleanConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.BooleanConstructor);
        public static readonly MemberExpression BooleanPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.BooleanPrototype);
        public static readonly MemberExpression NumberConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.NumberConstructor);
        public static readonly MemberExpression NumberPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.NumberPrototype);
        public static readonly MemberExpression MathObject = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.MathObject);
        public static readonly MemberExpression DateConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.DateConstructor);
        public static readonly MemberExpression DatePrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.DatePrototype);
        public static readonly MemberExpression RegExpConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.RegExpConstructor);
        public static readonly MemberExpression RegExpPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.RegExpPrototype);
        public static readonly MemberExpression ErrorConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.ErrorConstructor);
        public static readonly MemberExpression ErrorPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.ErrorPrototype);
        public static readonly MemberExpression EvalErrorConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.EvalErrorConstructor);
        public static readonly MemberExpression EvalErrorPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.EvalErrorPrototype);
        public static readonly MemberExpression RangeErrorConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.RangeErrorConstructor);
        public static readonly MemberExpression RangeErrorPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.RangeErrorPrototype);
        public static readonly MemberExpression ReferenceErrorConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.ReferenceErrorConstructor);
        public static readonly MemberExpression ReferenceErrorPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.ReferenceErrorPrototype);
        public static readonly MemberExpression SyntaxErrorConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.SyntaxErrorConstructor);
        public static readonly MemberExpression SyntaxErrorPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.SyntaxErrorPrototype);
        public static readonly MemberExpression TypeErrorConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.TypeErrorConstructor);
        public static readonly MemberExpression TypeErrorPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.TypeErrorPrototype);
        public static readonly MemberExpression UriErrorConstructor = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.UriErrorConstructor);
        public static readonly MemberExpression UriErrorPrototype = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.UriErrorPrototype);
        public static readonly MemberExpression JsonObject = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.JsonObject);
        public static readonly MemberExpression ThrowTypeErrorFunction = Expression.Property(Environment, Reflection.IEnvironmentMemberInfo.ThrowTypeErrorFunction);
    }
}