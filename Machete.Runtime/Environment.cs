using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects;
using Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects;
using Machete.Runtime.NativeObjects;
using Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Core;
using Machete.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using Machete.Core.Generators;
using Machete.Runtime.HostObjects;
using Machete.Runtime.HostObjects.Iterables;

namespace Machete.Runtime
{
    public sealed class Environment : IEnvironment
    {
        private readonly Stack<IExecutionContext> _contextStack = new Stack<IExecutionContext>();
        private readonly Stopwatch _stopWatch = new Stopwatch();
    

        public Environment()
        {
            Output = new Output();
            EmptyArgs = new SArgs(this);
            True = new LBoolean(this, true);
            False = new LBoolean(this, false);
            Undefined = new LUndefined(this);
            Null = new LNull(this);

            GlobalObject = new BGlobal(this);
            GlobalEnvironment = new SLexicalEnvironment(this, new SObjectEnvironmentRecord(this, GlobalObject, false), null);
            MathObject = new BMath(this);
            JsonObject = new BJson(this);
            ObjectConstructor = new CObject(this);
            FunctionConstructor = new CFunction(this);
            ArrayConstructor = new CArray(this);
            StringConstructor = new CString(this);
            BooleanConstructor = new CBoolean(this);
            NumberConstructor = new CNumber(this);
            DateConstructor = new CDate(this);
            RegExpConstructor = new CRegExp(this);
            ErrorConstructor = new CError(this);
            EvalErrorConstructor = new CEvalError(this);
            RangeErrorConstructor = new CRangeError(this);
            ReferenceErrorConstructor = new CReferenceError(this);
            SyntaxErrorConstructor = new CSyntaxError(this);
            TypeErrorConstructor = new CTypeError(this);
            UriErrorConstructor = new CUriError(this);
            ObjectPrototype = new PObject(this);
            FunctionPrototype = new PFunction(this);
            ArrayPrototype = new PArray(this);
            StringPrototype = new PString(this);
            BooleanPrototype = new PBoolean(this);
            NumberPrototype = new PNumber(this);
            DatePrototype = new PDate(this);
            RegExpPrototype = new PRegExp(this);
            ErrorPrototype = new PError(this);
            EvalErrorPrototype = new PEvalError(this);
            RangeErrorPrototype = new PRangeError(this);
            ReferenceErrorPrototype = new PReferenceError(this);
            SyntaxErrorPrototype = new PSyntaxError(this);
            TypeErrorPrototype = new PTypeError(this);
            UriErrorPrototype = new PUriError(this);

            GlobalObject.Initialize();
            MathObject.Initialize();
            JsonObject.Initialize();
            ObjectConstructor.Initialize();
            FunctionConstructor.Initialize();
            ArrayConstructor.Initialize();
            StringConstructor.Initialize();
            BooleanConstructor.Initialize();
            NumberConstructor.Initialize();
            DateConstructor.Initialize();
            RegExpConstructor.Initialize();
            ErrorConstructor.Initialize();
            EvalErrorConstructor.Initialize();
            RangeErrorConstructor.Initialize();
            ReferenceErrorConstructor.Initialize();
            SyntaxErrorConstructor.Initialize();
            TypeErrorConstructor.Initialize();
            UriErrorConstructor.Initialize();
            ObjectPrototype.Initialize();
            FunctionPrototype.Initialize();
            ArrayPrototype.Initialize();
            StringPrototype.Initialize();
            BooleanPrototype.Initialize();
            NumberPrototype.Initialize();
            DatePrototype.Initialize();
            RegExpPrototype.Initialize();
            ErrorPrototype.Initialize();
            EvalErrorPrototype.Initialize();
            RangeErrorPrototype.Initialize();
            ReferenceErrorPrototype.Initialize();
            SyntaxErrorPrototype.Initialize();
            TypeErrorPrototype.Initialize();
            UriErrorPrototype.Initialize();
        }





        public Output Output { get; private set; }
        public IExecutionContext Context { get; private set; }
        public IArgs EmptyArgs { get; private set; }
        public IUndefined Undefined { get; private set; }
        public IBoolean True { get; private set; }
        public IBoolean False { get; private set; }
        public INull Null { get; private set; }
        public ILexicalEnvironment GlobalEnvironment { get; private set; }
        public IObject GlobalObject { get; private set; }
        public IObject ObjectConstructor { get; private set; }
        public IObject ObjectPrototype { get; private set; }
        public IObject FunctionConstructor { get; private set; }
        public IObject FunctionPrototype { get; private set; }
        public IObject ArrayConstructor { get; private set; }
        public IObject ArrayPrototype { get; private set; }
        public IObject StringConstructor { get; private set; }
        public IObject StringPrototype { get; private set; }
        public IObject BooleanConstructor { get; private set; }
        public IObject BooleanPrototype { get; private set; }
        public IObject NumberConstructor { get; private set; }
        public IObject NumberPrototype { get; private set; }
        public IObject MathObject { get; private set; }
        public IObject DateConstructor { get; private set; }
        public IObject DatePrototype { get; private set; }
        public IObject RegExpConstructor { get; private set; }
        public IObject RegExpPrototype { get; private set; }
        public IObject ErrorConstructor { get; private set; }
        public IObject ErrorPrototype { get; private set; }
        public IObject EvalErrorConstructor { get; private set; }
        public IObject EvalErrorPrototype { get; private set; }
        public IObject RangeErrorConstructor { get; private set; }
        public IObject RangeErrorPrototype { get; private set; }
        public IObject ReferenceErrorConstructor { get; private set; }
        public IObject ReferenceErrorPrototype { get; private set; }
        public IObject SyntaxErrorConstructor { get; private set; }
        public IObject SyntaxErrorPrototype { get; private set; }
        public IObject TypeErrorConstructor { get; private set; }
        public IObject TypeErrorPrototype { get; private set; }
        public IObject UriErrorConstructor { get; private set; }
        public IObject UriErrorPrototype { get; private set; }
        public IObject JsonObject { get; private set; }

        public IFunction ThrowTypeErrorFunction { get; private set; }


        public IDynamic Execute(ExecutableCode executableCode)
        {
            _stopWatch.Restart();
            using (var context = EnterContext())
            {
                context.ThisBinding = GlobalObject;
                context.VariableEnviroment = GlobalEnvironment;
                context.LexicalEnviroment = GlobalEnvironment;
                context.Strict = executableCode.Strict;
                BindFunctionDeclarations(executableCode.FunctionDeclarations, executableCode.Strict, true);
                BindVariableDeclarations(executableCode.VariableDeclarations, executableCode.Strict, true);
                var result = executableCode.Code(this, EmptyArgs);
                Output.Write("Execution Time: " + _stopWatch.Elapsed);
                return result;
            }
        }

        public void Unwind()
        {
            foreach (var context in _contextStack)
            {
                Output.Write(context.CurrentFunction);
            }
        }
        
        public IBoolean CreateBoolean(bool value)
        {
            return value ? True : False;
        }

        public IString CreateString(string value)
        {
            return new LString(this, value);
        }

        public INumber CreateNumber(double value)
        {
            return new LNumber(this, value);
        }

        public IArgs CreateArgs(IEnumerable<IDynamic> values)
        {
            return new SArgs(this, values);
        }

        public IArgs ConcatArgs(IArgs first, IArgs second)
        {
            return new SArgs(this, first, second);
        }
        
        public IObject CreateArray()
        {
            return ArrayConstructor.Op_Construct(EmptyArgs);
        }

        public IObject CreateObject()
        {
            return ObjectConstructor.Op_Construct(EmptyArgs);
        }

        public IObject CreateRegExp(string pattern, string flags)
        {
            var args = CreateArgs(new IDynamic[] { CreateString(pattern), CreateString(flags) });
            return ((IConstructable)RegExpConstructor).Construct(this, args);
        }
        
        public IReference CreateReference(string name, IReferenceBase @base, bool strict)
        {
            return new SReference(this, @base, name, strict);
        }

        public IPropertyDescriptor CreateGenericDescriptor(bool? enumerable, bool? configurable)
        {
            throw new NotImplementedException();
        }

        public IPropertyDescriptor CreateDataDescriptor(IDynamic value, bool? writable, bool? enumerable, bool? configurable)
        {
            return new SPropertyDescriptor()
            {
                Value = value,
                Writable = writable,
                Enumerable = enumerable,
                Configurable = configurable
            };
        }

        public IPropertyDescriptor CreateAccessorDescriptor(IDynamic get, IDynamic set, bool? enumerable, bool? configurable)
        {
            return new SPropertyDescriptor()
            {
                Get = get,
                Set = set,
                Enumerable = enumerable,
                Configurable = configurable
            };
        }

        public IExecutionContext EnterContext()
        {
            if (Context == null)
            {
                return Context = new ExecutionContext(() => { });
            }
            _contextStack.Push(Context);
            return Context = new ExecutionContext(() => Context = _contextStack.Pop());
        }


        public void ThrowRuntimeException(IDynamic thrown)
        {
            throw new MacheteRuntimeException(thrown);
        }

        public IDynamic FromPropertyDescriptor(IPropertyDescriptor desc)
        {
            // 8.10.4 FromPropertyDescriptor ( Desc ) 

            if (desc == null) // Property descriptors use null rather than undefined to simplify interaction.
            {
                return Undefined;
            }

            var obj = ObjectConstructor.Op_Construct(EmptyArgs);

            if (desc.IsDataDescriptor)
            {
                var value = CreateDataDescriptor(desc.Value, true, true, true);
                var writable = CreateDataDescriptor(CreateBoolean(desc.Writable.Value), true, true, true);

                obj.DefineOwnProperty("value", value, false);
                obj.DefineOwnProperty("writable", writable, false);
            }
            else
            {
                Debug.Assert(desc.IsAccessorDescriptor);

                var get = CreateDataDescriptor(desc.Get, true, true, true);
                var set = CreateDataDescriptor(desc.Set, true, true, true);

                obj.DefineOwnProperty("get", get, false);
                obj.DefineOwnProperty("set", set, false);
            }

            var enumerable = CreateDataDescriptor(CreateBoolean(desc.Enumerable.Value), true, true, true);
            var configurable = CreateDataDescriptor(CreateBoolean(desc.Configurable.Value), true, true, true);

            obj.DefineOwnProperty("enumerable", enumerable, false);
            obj.DefineOwnProperty("configurable", configurable, false);

            return obj;
        }

        public IPropertyDescriptor ToPropertyDescriptor(IObject obj)
        {
            // 8.10.5 ToPropertyDescriptor ( Obj ) 

            Debug.Assert(obj != null);

            var desc = new SPropertyDescriptor();

            if (obj.HasProperty("enumerable"))
            {
                desc.Enumerable = obj.Get("enumerable").ConvertToBoolean().BaseValue;
            }

            if (obj.HasProperty("configurable"))
            {
                desc.Enumerable = obj.Get("configurable").ConvertToBoolean().BaseValue;
            }

            if (obj.HasProperty("value"))
            {
                desc.Value = obj.Get("value");
            }

            if (obj.HasProperty("writable"))
            {
                desc.Writable = obj.Get("writable").ConvertToBoolean().BaseValue;
            }

            if (obj.HasProperty("get"))
            {
                var getter = obj.Get("get");
                if (getter.TypeCode != LanguageTypeCode.Undefined && !(getter is ICallable))
                {
                    throw CreateTypeError("");
                }
                desc.Get = getter;
            }

            if (obj.HasProperty("set"))
            {
                var setter = obj.Get("set");
                if (setter.TypeCode != LanguageTypeCode.Undefined && !(setter is ICallable))
                {
                    throw CreateTypeError("");
                }
                desc.Get = setter;
            }

            if (desc.Get != null || desc.Set != null)
            {
                if (desc.Value != null || desc.Writable != null)
                {
                    throw CreateTypeError("");
                }
            }

            return desc;
        }


        public MacheteRuntimeException CreateError(string message)
        {
            var args = CreateArgs(new[] { CreateString(message) });
            var error = ErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateEvalError(string message)
        {
            var args = CreateArgs(new [] { CreateString(message) });
            var error = EvalErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateRangeError(string message)
        {
            var args = CreateArgs(new [] { CreateString(message) });
            var error = RangeErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateReferenceError(string message)
        {
            var args = CreateArgs(new [] { CreateString(message) });
            var error = ReferenceErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateSyntaxError(string message)
        {
            var args = CreateArgs(new [] { CreateString(message) });
            var error = SyntaxErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateTypeError(string message)
        {
            var args = CreateArgs(new [] { CreateString(message) });
            var error = TypeErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateUriError(string message)
        {
            var args = CreateArgs(new [] { CreateString(message) });
            var error = UriErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }


        public bool Instanceof(IDynamic left, IDynamic right)
        {
            // 15.3.5.3 [[HasInstance]] (V)

            var lObj = left as IObject;
            var rObj = right as IObject;

            if (lObj == null)
            {
                return false;
            }

            var oPrototype = rObj.Get("prototype") as IObject;
            if (oPrototype == null)
            {
                throw CreateTypeError("");
            }

            do
            {
                lObj = lObj.Prototype;
                if (lObj == oPrototype)
                {
                    return true;
                }
            }
            while (lObj.Prototype != null);

            return false;
        }


        public void CheckObjectCoercible(IDynamic value)
        {
            switch (value.TypeCode)
            {
                case LanguageTypeCode.Undefined:
                case LanguageTypeCode.Null:
                    throw CreateTypeError("'" + value + "' cannot be coerced into an object.");
            }
        }

        public void BindFunctionDeclarations(ReadOnlyList<FunctionDeclaration> functionDeclarations, bool strict, bool configurableBindings)
        {
            var record = Context.VariableEnviroment.Record;
            foreach (var functionDeclaration in functionDeclarations)
            {
                if (!record.HasBinding(functionDeclaration.Identifier))
                {
                    record.CreateMutableBinding(functionDeclaration.Identifier, configurableBindings);
                }
                var func = CreateFunction(functionDeclaration.ExecutableCode, functionDeclaration.FormalParameterList, Context.LexicalEnviroment);
                record.SetMutableBinding(functionDeclaration.Identifier, func, strict);
            }
        }

        public void BindVariableDeclarations(ReadOnlyList<string> variableDeclarations, bool strict, bool configurableBindings)
        {
            var record = Context.VariableEnviroment.Record;
            foreach (var variableDeclaration in variableDeclarations)
            {
                if (!record.HasBinding(variableDeclaration))
                {
                    record.CreateMutableBinding(variableDeclaration, configurableBindings);
                }
                record.SetMutableBinding(variableDeclaration, Undefined, strict);
            }
        }


        public IObject CreateFunction(ExecutableCode executableCode, ReadOnlyList<string> formalParameters, ILexicalEnvironment scope)
        {
            // 13.2 Creating Function Objects 

            var f = new NFunction(this);
            {
                f.Class = "Function";
                f.Extensible = true;
                f.Prototype = FunctionPrototype;
                f.ExecutableCode = executableCode;
                f.FormalParameters = formalParameters;
                f.Scope = scope;

                f.DefineOwnProperty("length", CreateDataDescriptor(CreateNumber(f.FormalParameters.Count), false, false ,false), false);
                f.DefineOwnProperty("constructor", CreateDataDescriptor(f, true, false, true), false);
                f.DefineOwnProperty("prototype", CreateDataDescriptor(ObjectConstructor.Op_Construct(EmptyArgs), true, false, false), false);

                if (executableCode.Strict)
                {
                    var desc = CreateAccessorDescriptor(ThrowTypeErrorFunction, ThrowTypeErrorFunction, false, false);
                    f.DefineOwnProperty("caller", desc, false);
                    f.DefineOwnProperty("arguments", desc, false);
                }
            }
            return f;
        }


        public IObjectBuilder CreateObjectBuilder(IObject o)
        {
            return new LObject.Builder(o);
        }


        public IObject CreateIterableFromGenerator(ReadOnlyList<GeneratorStep> steps, ReadOnlyList<string> variableDeclarations, ILexicalEnvironment scope)
        {
            return new HGeneratorIterable(this, steps, variableDeclarations, scope);
        }

        public bool CombineGeneratorWithIterator(Generator generator, IDynamic other)
        {
            var iterable = other.ConvertToObject();
            var createIterator = iterable.Get("createIterator") as ICallable;
            if (createIterator == null)
                throw CreateTypeError("");
            var iterator = createIterator.Call(this, iterable, EmptyArgs).ConvertToObject();
            if (!iterator.HasProperty("current"))
                throw CreateTypeError("");
            var next = iterator.Get("next") as ICallable;
            if (next == null)
                throw CreateTypeError("");
            GeneratorStep step = null; step = (_e, _g) =>
            {
                if (!next.Call(this, Undefined, EmptyArgs).ConvertToBoolean().BaseValue)
                    return false;
                generator.Current = iterator.Get("current");
                generator.Steps.Enqueue(step);
                return true;
            };
            return step(this, generator);
        }

        public void ForeachLoop(string identifier, IDynamic iterable, Code loopBodyCode)
        {
            var emptyArgs = EmptyArgs;
            var iterator = new Iterator(this, iterable);
            var oldEnv = Context.LexicalEnviroment;
            var newEnv = oldEnv.NewDeclarativeEnvironment();
            var newRec = newEnv.Record;
 
            newRec.CreateMutableBinding(identifier, false);
            Context.LexicalEnviroment = newEnv;

            try
            {
                while (iterator.Next())
                {
                    newRec.SetMutableBinding(identifier, iterator.Current, true);
                    loopBodyCode(this, emptyArgs);
                }
            }
            finally
            {
                Context.LexicalEnviroment = oldEnv;
            }
        }
    }
}
