using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using System.Diagnostics;
using Machete.Interfaces;
using System.Linq.Expressions;
using System.Threading;

namespace Machete.Runtime.NativeObjects
{
    //public class NRuntimeFunction : LObject, ICallable, IConstructable, IHasInstance
    //{
    //    public ILexicalEnvironment Scope { get; set; }
    //    public ExecutableCode ExecutableCode { get; set; }
    //    public ReadOnlyList<string> FormalParameterList { get; set; }

    //    public NRuntimeFunction(IEnvironment enviroment)
    //        : base(enviroment)
    //    {

    //    }

    //    public IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
    //    {
    //        var oldContext = environment.Context;
    //        var newEnvironment = Scope.NewDeclarativeEnvironment();
    //        using (var newContext = environment.EnterContext())
    //        {
    //            newContext.LexicalEnviroment = newEnvironment;
    //            newContext.VariableEnviroment = newEnvironment;
    //            newContext.Strict = ExecutableCode.Strict;
    //            if (ExecutableCode.Strict)
    //            {
    //                Environment.Context.ThisBinding = thisBinding;
    //            }
    //            else
    //            {
    //                switch (thisBinding.TypeCode)
    //                {
    //                    case LanguageTypeCode.Undefined:
    //                    case LanguageTypeCode.Null:
    //                        Environment.Context.ThisBinding = Environment.GlobalObject;
    //                        break;
    //                    default:
    //                        Environment.Context.ThisBinding = thisBinding.ConvertToObject();
    //                        break;
    //                }
    //            }
    //            return Call(environment, args);
    //        }
    //    }

    //    protected virtual IDynamic Call(IEnvironment environment, IArgs args)
    //    {
    //        BindFormalParameters(args);
    //        Environment.BindFunctionDeclarations(ExecutableCode.FunctionDeclarations, ExecutableCode.Strict, true);
    //        BindArgumentsObject(args);
    //        Environment.BindVariableDeclarations(ExecutableCode.VariableDeclarations, ExecutableCode.Strict, true);
    //        return ExecutableCode.Code(environment, args);
    //    }

    //    public virtual IObject Construct(IEnvironment environment, IArgs args)
    //    {
    //        var obj = new LObject(environment);
    //        obj.Class = "Object";
    //        obj.Extensible = true;

    //        var proto = Get("prototype");
    //        if (proto.TypeCode == LanguageTypeCode.Object)
    //        {
    //            obj.Prototype = (IObject)proto;
    //        }
    //        else
    //        {
    //            obj.Prototype = environment.ObjectPrototype;
    //        }

    //        var result = ((ICallable)this).Call(environment, obj, args);
    //        if (result.TypeCode == LanguageTypeCode.Object)
    //        {
    //            return (IObject)result;
    //        }

    //        return obj;
    //    }

    //    public virtual bool HasInstance(IDynamic value)
    //    {
    //        return Environment.Instanceof(value, this);
    //    }

    //    public override IDynamic Get(string p)
    //    {
    //        if (this is NBoundFunction)
    //        {
    //            return base.Get(p);
    //        }
    //        var v = base.Get(p);
    //        if (p == "callee")
    //        {
    //            var vFunc = v as IFunction;
    //            if (vFunc != null && vFunc.Strict)
    //            {
    //                throw Environment.CreateTypeError("");
    //            }
    //        }
    //        return v;
    //    }

    //    private void BindFormalParameters(IArgs args)
    //    {
    //        var record = (IDeclarativeEnvironmentRecord)Environment.Context.VariableEnviroment.Record;
    //        for (int i = 0; i < FormalParameterList.Count; i++)
    //        {
    //            var name = FormalParameterList[i];
    //            if (!record.HasBinding(name))
    //            {
    //                record.CreateMutableBinding(name, false);
    //            }
    //            record.SetMutableBinding(name, args[i], ExecutableCode.Strict);
    //        }
    //    }

    //    private void BindArgumentsObject(IArgs args)
    //    {
    //        var record = (IDeclarativeEnvironmentRecord)Environment.Context.VariableEnviroment.Record;
    //        if (!record.HasBinding("arguments"))
    //        {
    //            var argumentsObj = CreateArgumentsObject(args);
    //            if (ExecutableCode.Strict)
    //            {
    //                record.CreateImmutableBinding("arguments");
    //                record.InitializeImmutableBinding("arguments", argumentsObj);
    //            }
    //            else
    //            {
    //                record.CreateMutableBinding("arguments", false);
    //                record.SetMutableBinding("arguments", argumentsObj, false);
    //            }
    //        }
    //    }

    //    private IObject CreateArgumentsObject(IArgs args)
    //    {
    //        var obj = new NArguments(Environment);
    //        var len = Environment.CreateNumber(args.Count);
    //        var lenDesc = Environment.CreateDataDescriptor(len, true, false, true);
    //        var map = Environment.ObjectConstructor.Op_Construct(Environment.EmptyArgs);
    //        var mappedNames = new List<string>();
    //        var index = args.Count - 1;

    //        obj.Class = "Arguments";
    //        obj.Extensible = true;
    //        obj.Prototype = Environment.ObjectPrototype;
    //        obj.DefineOwnProperty("length", lenDesc, false);

    //        while (--index >= 0)
    //        {
    //            var val = args[index];
    //            var valDesc = Environment.CreateDataDescriptor(val, true, true, true);

    //            obj.DefineOwnProperty(index.ToString(), valDesc, false);
    //            if (index < FormalParameterList.Count)
    //            {
    //                var name = FormalParameterList[index];
    //                if (!ExecutableCode.Strict)
    //                {
    //                    var g = MakeArgGetter(name);
    //                    var p = MakeArgSetter(name);
    //                    var desc = Environment.CreateAccessorDescriptor(g, p, false, true);

    //                    map.DefineOwnProperty(name, desc, false);
    //                    mappedNames.Add(name);
    //                }
    //            }
    //        }

    //        if (mappedNames.Count > 0)
    //        {
    //            obj.ParameterMap = map;
    //        }

    //        if (!ExecutableCode.Strict)
    //        {
    //            var desc = Environment.CreateDataDescriptor(this, true, false, true);
    //            obj.DefineOwnProperty("callee", desc, false);
    //        }
    //        else
    //        {
    //            var desc = Environment.CreateAccessorDescriptor(Environment.ThrowTypeErrorFunction, Environment.ThrowTypeErrorFunction, false, false);
    //            obj.DefineOwnProperty("caller", desc, false);
    //            obj.DefineOwnProperty("callee", desc, false);
    //        }

    //        return obj;
    //    }

    //    private IObject MakeArgGetter(string name)
    //    {
    //        return null;
    //        var fpl = ReadOnlyList<string>.Empty;
    //        var code = new Lazy<Code>(() => _compiler.CompileFunctionCode(fpl, "return " + name + ";"));
    //        return Environment.CreateFunction(fpl, true, code, Environment.Context.VariableEnviroment);
    //    }

    //    private IObject MakeArgSetter(string name)
    //    {
    //        return null;
    //        var param = name + "_arg";
    //        var fpl = new ReadOnlyList<string>(param);
    //        var code = new Lazy<Code>(() => _compiler.CompileFunctionCode(fpl, name + " = " + param + ";"));
    //        return Environment.CreateFunction(fpl, true, code, Environment.Context.VariableEnviroment);
    //    }
    //}

    //public class NBoundFunction : NRuntimeFunction
    //{
    //    public IObject TargetFunction { get; set; }
    //    public IDynamic BoundThis { get; set; }
    //    public IArgs BoundArguments { get; set; }

    //    public NBoundFunction(IEnvironment enviroment)
    //        : base(enviroment)
    //    {

    //    }

    //    public override IDynamic Call(IEnvironment environment, IArgs args)
    //    {
    //        var func = TargetFunction as NRuntimeFunction;
    //        return func.Call(environment, BoundThis, environment.ConcatArgs(BoundArguments, args));
    //    }

    //    public override IObject Construct(IEnvironment environment, IArgs args)
    //    {
    //        var func = TargetFunction as NRuntimeFunction;
    //        if (func == null)
    //        {
    //            throw Environment.CreateTypeError("");
    //        }
    //        return func.Construct(environment, environment.ConcatArgs(BoundArguments, args));
    //    }

    //    public override bool HasInstance(IDynamic value)
    //    {
    //        var func = TargetFunction as IHasInstance;
    //        if (func == null)
    //        {
    //            throw Environment.CreateTypeError("");
    //        }
    //        return func.HasInstance(value);
    //    }
    //}


    public class NFunction : LObject, IFunction
    {
        public ILexicalEnvironment Scope { get; set; }
        public ExecutableCode ExecutableCode { get; set; }
        public ReadOnlyList<string> FormalParameterList { get; set; }
        public Lazy<Code> Code { get; set; }
        public IObject TargetFunction { get; set; }
        public IDynamic BoundThis { get; set; }
        public IArgs BoundArguments { get; set; }
        public bool Strict { get; set; }
        public bool BindFunction { get; set; }


        public NFunction(IEnvironment enviroment)
            : base(enviroment)
        {

        }


        public override void Initialize()
        {
            Class = "Function";
            Extensible = true;
            Prototype = Environment.FunctionPrototype;

            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(FormalParameterList.Count)), false);

            var proto = Environment.ObjectConstructor.Op_Construct(Environment.EmptyArgs);
            DefineOwnProperty("constructor", Environment.CreateDataDescriptor(proto, true, false, true), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(proto, true, false, false), false);

            if (Strict)
            {
                var desc = Environment.CreateAccessorDescriptor(Environment.ThrowTypeErrorFunction, Environment.ThrowTypeErrorFunction, false, false);
                DefineOwnProperty("caller", desc, false);
                DefineOwnProperty("argument", desc, false);
            }

            if (this is IBuiltinFunction)
            {
                base.Initialize();
            }
        }

        public override IDynamic Get(string p)
        {
            // 15.3.5.4 [[Get]] (P) 

            if (BindFunction)
            {
                // NOTE Function objects created using Function.prototype.bind 
                // use the default [[Get]] internal method. 

                return base.Get(p);
            }
            else
            {
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
        }

        public virtual IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            if (BindFunction)
            {
                // 15.3.4.5.1 [[Call]] 
                var func = TargetFunction as IFunction;
                Debug.Assert(func != null);
                return func.Call(environment, BoundThis, environment.ConcatArgs(BoundArguments, args));
            }
            else
            {
                // 13.2.1 [[Call]] 
                using (var c = environment.EnterContext())
                {
                    PrepareContext(thisBinding);
                    BindFormalParameters(args);
                    Environment.BindFunctionDeclarations(ExecutableCode.FunctionDeclarations, ExecutableCode.Strict, true);
                    BindArgumentsObject(args);
                    Environment.BindVariableDeclarations(ExecutableCode.VariableDeclarations, ExecutableCode.Strict, true);
                    return ExecutableCode.Code(environment, args);
                }
            }
        }

        public virtual IObject Construct(IEnvironment environment, IArgs args)
        {
            if (BindFunction)
            {
                // 15.3.4.5.2 [[Construct]]

                var func = TargetFunction as IFunction;
                if (func == null)
                {
                    throw Environment.CreateTypeError("");
                }
                return func.Construct(environment, environment.ConcatArgs(BoundArguments, args));
            }
            else
            {
                // 13.2.2 [[Construct]]

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
        }

        public bool HasInstance(IDynamic value)
        {
            if (BindFunction)
            {
                // 15.3.4.5.3 [[HasInstance]] (V)

                var func = TargetFunction as IHasInstance;
                if (func == null)
                {
                    throw Environment.CreateTypeError("");
                }
                return func.HasInstance(value);
            }
            else
            {
                return Environment.Instanceof(value, this);
            }
        }

        private void PrepareContext(IDynamic thisBinding)
        {
            var scope = Scope.NewDeclarativeEnvironment();
            var context = Environment.Context;
            context.LexicalEnviroment = scope;
            context.VariableEnviroment = scope;
            context.Strict = ExecutableCode.Strict;

            if (Strict)
            {
                Environment.Context.ThisBinding = thisBinding;
            }
            else
            {
                switch (thisBinding.TypeCode)
                {
                    case LanguageTypeCode.Undefined:
                    case LanguageTypeCode.Null:
                        Environment.Context.ThisBinding = Environment.GlobalObject;
                        break;
                    default:
                        Environment.Context.ThisBinding = thisBinding.ConvertToObject();
                        break;
                }
            }
        }

        private void BindFormalParameters(IArgs args)
        {
            var record = (IDeclarativeEnvironmentRecord)Environment.Context.VariableEnviroment.Record;
            for (int i = 0; i < FormalParameterList.Count; i++)
            {
                var name = FormalParameterList[i];
                if (!record.HasBinding(name))
                {
                    record.CreateMutableBinding(name, false);
                }
                record.SetMutableBinding(name, args[i], Strict);
            }
        }

        private void BindArgumentsObject(IArgs args)
        {
            var record = (IDeclarativeEnvironmentRecord)Environment.Context.VariableEnviroment.Record;
            if (!record.HasBinding("arguments"))
            {
                var argumentsObj = CreateArgumentsObject(args);
                if (Strict)
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
                if (index < FormalParameterList.Count)
                {
                    var name = FormalParameterList[index];
                    if (!Strict)
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

            if (!Strict)
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