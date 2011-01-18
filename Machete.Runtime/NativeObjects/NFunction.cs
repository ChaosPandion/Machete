using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using System.Diagnostics;
using Machete.Core;
using System.Linq.Expressions;
using System.Threading;
using System.Dynamic;

namespace Machete.Runtime.NativeObjects
{
    public class NFunction : LObject, ICallable, IConstructable, IHasInstance
    {
        public ILexicalEnvironment Scope { get; set; }
        public ExecutableCode ExecutableCode { get; set; }
        public ReadOnlyList<string> FormalParameters { get; set; }

        public NFunction(IEnvironment enviroment)
            : base(enviroment)
        {

        }

        public IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            var oldContext = environment.Context;
            var newEnvironment = Scope.NewDeclarativeEnvironment();
            using (var newContext = environment.EnterContext())
            {
                newContext.LexicalEnviroment = newEnvironment;
                newContext.VariableEnviroment = newEnvironment;
                newContext.Strict = ExecutableCode.Strict;
                if (ExecutableCode.Strict)
                {
                    newContext.ThisBinding = thisBinding;
                }
                else
                {
                    switch (thisBinding.TypeCode)
                    {
                        case LanguageTypeCode.Undefined:
                        case LanguageTypeCode.Null:
                            newContext.ThisBinding = Environment.GlobalObject;
                            break;
                        default:
                            newContext.ThisBinding = thisBinding.ConvertToObject();
                            break;
                    }
                }

                return Call(environment, args);
            }
        }

        protected virtual IDynamic Call(IEnvironment environment, IArgs args)
        {
            BindFormalParameters(args);
            Environment.BindFunctionDeclarations(ExecutableCode.FunctionDeclarations, ExecutableCode.Strict, true);
            BindArgumentsObject(args);
            Environment.BindVariableDeclarations(ExecutableCode.VariableDeclarations, ExecutableCode.Strict, true);
            return ExecutableCode.Code(environment, args);
        }

        public virtual IObject Construct(IEnvironment environment, IArgs args)
        {
            var obj = new LObject(environment);
            obj.Class = "Object";
            obj.Extensible = true;

            var proto = Get("prototype");
            if (proto.TypeCode == LanguageTypeCode.Object)
            {
                obj.Prototype = (IObject)proto;
            }
            else
            {
                obj.Prototype = environment.ObjectPrototype;
            }

            var result = ((ICallable)this).Call(environment, obj, args);
            if (result.TypeCode == LanguageTypeCode.Object)
            {
                return (IObject)result;
            }

            return obj;
        }

        public virtual bool HasInstance(IDynamic value)
        {
            return Environment.Instanceof(value, this);
        }

        public override IDynamic Get(string p)
        {
            if (this is NBoundFunction)
            {
                return base.Get(p);
            }
            var v = base.Get(p);
            if (p == "callee")
            {
                var vFunc = v as IFunction;
                if (vFunc != null && vFunc.Strict)
                {
                    throw Environment.CreateTypeError("");
                }
            }
            return v;
        }

        private void BindFormalParameters(IArgs args)
        {
            var record = (IDeclarativeEnvironmentRecord)Environment.Context.VariableEnviroment.Record;
            for (int i = 0; i < FormalParameters.Count; i++)
            {
                var name = FormalParameters[i];
                if (!record.HasBinding(name))
                {
                    record.CreateMutableBinding(name, false);
                }
                record.SetMutableBinding(name, args[i], ExecutableCode.Strict);
            }
        }

        private void BindArgumentsObject(IArgs args)
        {
            var record = (IDeclarativeEnvironmentRecord)Environment.Context.VariableEnviroment.Record;
            if (!record.HasBinding("arguments"))
            {
                var argumentsObj = CreateArgumentsObject(args);
                if (ExecutableCode.Strict)
                {
                    record.CreateImmutableBinding("arguments");
                    record.InitializeImmutableBinding("arguments", argumentsObj);
                }
                else
                {
                    record.CreateMutableBinding("arguments", false);
                    record.SetMutableBinding("arguments", argumentsObj, false);
                }
            }
        }

        private IObject CreateArgumentsObject(IArgs args)
        {
            var obj = new NArguments(Environment);
            var len = Environment.CreateNumber(args.Count);
            var lenDesc = Environment.CreateDataDescriptor(len, true, false, true);
            var map = Environment.ObjectConstructor.Op_Construct(Environment.EmptyArgs);
            var mappedNames = new List<string>();
            var index = args.Count - 1;

            obj.Class = "Arguments";
            obj.Extensible = true;
            obj.Prototype = Environment.ObjectPrototype;
            obj.DefineOwnProperty("length", lenDesc, false);

            while (--index >= 0)
            {
                var val = args[index];
                var valDesc = Environment.CreateDataDescriptor(val, true, true, true);

                obj.DefineOwnProperty(index.ToString(), valDesc, false);
                if (index < FormalParameters.Count)
                {
                    var name = FormalParameters[index];
                    if (!ExecutableCode.Strict)
                    {
                        var g = MakeArgGetter(name);
                        var p = MakeArgSetter(name);
                        var desc = Environment.CreateAccessorDescriptor(g, p, false, true);

                        map.DefineOwnProperty(name, desc, false);
                        mappedNames.Add(name);
                    }
                }
            }

            if (mappedNames.Count > 0)
            {
                obj.ParameterMap = map;
            }

            if (!ExecutableCode.Strict)
            {
                var desc = Environment.CreateDataDescriptor(this, true, false, true);
                obj.DefineOwnProperty("callee", desc, false);
            }
            else
            {
                var desc = Environment.CreateAccessorDescriptor(Environment.ThrowTypeErrorFunction, Environment.ThrowTypeErrorFunction, false, false);
                obj.DefineOwnProperty("caller", desc, false);
                obj.DefineOwnProperty("callee", desc, false);
            }

            return obj;
        }

        private IObject MakeArgGetter(string name)
        {
            return null;
            //var fpl = ReadOnlyList<string>.Empty;
            //var code = new Lazy<Code>(() => _compiler.CompileFunctionCode(fpl, "return " + name + ";"));
            //return Environment.CreateFunction(fpl, true, code, Environment.Context.VariableEnviroment);
        }

        private IObject MakeArgSetter(string name)
        {
            return null;
            //var param = name + "_arg";
            //var fpl = new ReadOnlyList<string>(param);
            //var code = new Lazy<Code>(() => _compiler.CompileFunctionCode(fpl, name + " = " + param + ";"));
            //return Environment.CreateFunction(fpl, true, code, Environment.Context.VariableEnviroment);
        }
    }
}