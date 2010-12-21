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
using Machete.Interfaces;

namespace Machete.Runtime
{
    public sealed class Environment : IEnvironment
    {
        private readonly Stack<IExecutionContext> _contextStack;
    

        public Environment()
        {
            _contextStack = new Stack<IExecutionContext>();

            EmptyArgs = new SArgs(this);
            True = new LBoolean(this, true);
            False = new LBoolean(this, false);
            Undefined = new LUndefined(this);

            GlobalObject = new BGlobal(this);
            MathObject = new BMath(this);
            JsonObject = new BJson(this);

            var record = new SObjectEnvironmentRecord(this, GlobalObject, false);
            var environment = new SLexicalEnvironment(this, record, null);
            Context = new ExecutionContext(() => { });
            Context.LexicalEnviroment = Context.VariableEnviroment = environment;
            Context.ThisBinding = GlobalObject;

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

            ObjectConstructor.Prototype = FunctionPrototype;
            FunctionConstructor.Prototype = FunctionPrototype;
            ArrayConstructor.Prototype = FunctionPrototype;
            StringConstructor.Prototype = FunctionPrototype;
            BooleanConstructor.Prototype = FunctionPrototype;
            NumberConstructor.Prototype = FunctionPrototype;
            DateConstructor.Prototype = FunctionPrototype;
            RegExpConstructor.Prototype = FunctionPrototype;
            ErrorConstructor.Prototype = FunctionPrototype;
            EvalErrorConstructor.Prototype = FunctionPrototype;
            RangeErrorConstructor.Prototype = FunctionPrototype;
            ReferenceErrorConstructor.Prototype = FunctionPrototype;
            SyntaxErrorConstructor.Prototype = FunctionPrototype;
            TypeErrorConstructor.Prototype = FunctionPrototype;
            UriErrorConstructor.Prototype = FunctionPrototype;

            ObjectPrototype.Prototype = null;
            FunctionPrototype.Prototype = ObjectPrototype;
            ArrayPrototype.Prototype = ObjectPrototype;
            StringPrototype.Prototype = ObjectPrototype;
            BooleanPrototype.Prototype = ObjectPrototype;
            NumberPrototype.Prototype = ObjectPrototype;
            DatePrototype.Prototype = ObjectPrototype;
            RegExpPrototype.Prototype = ObjectPrototype;
            ErrorPrototype.Prototype = ObjectPrototype;
            EvalErrorPrototype.Prototype = ErrorPrototype;
            RangeErrorPrototype.Prototype = ErrorPrototype;
            ReferenceErrorPrototype.Prototype = ErrorPrototype;
            SyntaxErrorPrototype.Prototype = ErrorPrototype;
            TypeErrorPrototype.Prototype = ErrorPrototype;
            UriErrorPrototype.Prototype = ErrorPrototype;

            ObjectConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(ObjectPrototype), false);
            FunctionConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(FunctionPrototype), false);
            ArrayConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(ArrayPrototype), false);
            StringConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(StringPrototype), false);
            BooleanConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(BooleanPrototype), false);
            NumberConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(NumberPrototype), false);
            DateConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(DatePrototype), false);
            RegExpConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(RegExpPrototype), false);
            ErrorConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(ErrorPrototype), false);
            EvalErrorConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(EvalErrorPrototype), false);
            RangeErrorConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(RangeErrorPrototype), false);
            ReferenceErrorConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(ReferenceErrorPrototype), false);
            SyntaxErrorConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(SyntaxErrorPrototype), false);
            TypeErrorConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(TypeErrorPrototype), false);
            UriErrorConstructor.DefineOwnProperty("prototype", CreateDataDescriptor(UriErrorPrototype), false);

            ObjectPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(ObjectConstructor), false);
            FunctionPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(FunctionConstructor), false);
            ArrayPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(ArrayConstructor), false);
            StringPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(StringConstructor), false);
            BooleanPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(BooleanConstructor), false);
            NumberPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(NumberConstructor), false);
            DatePrototype.DefineOwnProperty("constructor", CreateDataDescriptor(DateConstructor), false);
            RegExpPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(RegExpConstructor), false);
            ErrorPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(ErrorConstructor), false);
            EvalErrorPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(EvalErrorConstructor), false);
            RangeErrorPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(RangeErrorConstructor), false);
            ReferenceErrorPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(ReferenceErrorConstructor), false);
            SyntaxErrorPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(SyntaxErrorConstructor), false);
            TypeErrorPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(TypeErrorConstructor), false);
            UriErrorPrototype.DefineOwnProperty("constructor", CreateDataDescriptor(UriErrorConstructor), false);

            GlobalObject.DefineOwnProperty("Object", CreateDataDescriptor(ObjectConstructor), false);
            GlobalObject.DefineOwnProperty("Function", CreateDataDescriptor(FunctionConstructor), false);
            GlobalObject.DefineOwnProperty("Array", CreateDataDescriptor(ArrayConstructor), false);
            GlobalObject.DefineOwnProperty("String", CreateDataDescriptor(StringConstructor), false);
            GlobalObject.DefineOwnProperty("Boolean", CreateDataDescriptor(BooleanConstructor), false);
            GlobalObject.DefineOwnProperty("Number", CreateDataDescriptor(NumberConstructor), false);
            GlobalObject.DefineOwnProperty("Date", CreateDataDescriptor(DateConstructor), false);
            GlobalObject.DefineOwnProperty("RegExp", CreateDataDescriptor(RegExpConstructor), false);
            GlobalObject.DefineOwnProperty("Error", CreateDataDescriptor(ErrorConstructor), false);
            GlobalObject.DefineOwnProperty("EvalError", CreateDataDescriptor(EvalErrorConstructor), false);
            GlobalObject.DefineOwnProperty("RangeError", CreateDataDescriptor(RangeErrorConstructor), false);
            GlobalObject.DefineOwnProperty("ReferenceError", CreateDataDescriptor(ReferenceErrorConstructor), false);
            GlobalObject.DefineOwnProperty("SyntaxError", CreateDataDescriptor(SyntaxErrorConstructor), false);
            GlobalObject.DefineOwnProperty("TypeError", CreateDataDescriptor(TypeErrorConstructor), false);
            GlobalObject.DefineOwnProperty("URIError", CreateDataDescriptor(UriErrorConstructor), false);
            GlobalObject.DefineOwnProperty("Math", CreateDataDescriptor(MathObject), false);
            GlobalObject.DefineOwnProperty("JSON", CreateDataDescriptor(JsonObject), false);
        }


        public IExecutionContext Context { get; private set; }
        public IArgs EmptyArgs { get; private set; }
        public IUndefined Undefined { get; private set; }
        public IBoolean True { get; private set; }
        public IBoolean False { get; private set; }
        public INull Null { get; private set; }
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
        
        public IArgs CreateArgs(IDynamic value)
        {
            return new SArgs(this, value);
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
        
        public IReference CreateReference(string name, IReferenceBase @base, bool strict)
        {
            return new SReference(this, @base, name, strict);
        }

        
        public IObject CreateFunction(string[] formalParameterList, bool strict, Lazy<Code> code)
        {
            return new NFunction(this, formalParameterList, strict, code, Context.VariableEnviroment);
        }

        public IObject CreateFunction(string[] formalParameterList, bool strict, Lazy<Code> code, ILexicalEnvironment scope)
        {
            return new NFunction(this, formalParameterList, strict, code, scope);
        }





        public IPropertyDescriptor CreateGenericDescriptor()
        {
            throw new NotImplementedException();
        }

        public IPropertyDescriptor CreateGenericDescriptor(bool? enumerable)
        {
            throw new NotImplementedException();
        }

        public IPropertyDescriptor CreateGenericDescriptor(bool? enumerable, bool? configurable)
        {
            throw new NotImplementedException();
        }


        public IPropertyDescriptor CreateDataDescriptor(IDynamic value)
        {
            return CreateDataDescriptor(value, false, false, false);
        }

        public IPropertyDescriptor CreateDataDescriptor(IDynamic value, bool? writable)
        {
            return CreateDataDescriptor(value, writable, false, false);
        }

        public IPropertyDescriptor CreateDataDescriptor(IDynamic value, bool? writable, bool? enumerable)
        {
            return CreateDataDescriptor(value, writable, enumerable, false);
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


        public IPropertyDescriptor CreateAccessorDescriptor(IDynamic get, IDynamic set)
        {
            return CreateAccessorDescriptor(get, set, false, false);
        }

        public IPropertyDescriptor CreateAccessorDescriptor(IDynamic get, IDynamic set, bool? enumerable)
        {
            return CreateAccessorDescriptor(get, set, enumerable, false);
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
            _contextStack.Push(Context);
            Context = new ExecutionContext(() => Context = _contextStack.Pop());
            return Context;
        }


        public void ThrowRuntimeException(IDynamic thrown)
        {
            throw new MacheteRuntimeException(thrown);
        }


        public IObject CreateArguments(string[] formalParameterList, IArgs args)
        {
            throw new NotImplementedException();
        }


        public IObject CreateArguments(string[] formalParameterList, IArgs args, bool strict)
        {
            throw new NotImplementedException();
        }


        public IObject FromPropertyDescriptor(IPropertyDescriptor desc)
        {
            throw new NotImplementedException();
        }

        public IPropertyDescriptor ToPropertyDescriptor(IObject obj)
        {
            throw new NotImplementedException();
        }


        public MacheteRuntimeException CreateError(string message)
        {
            var args = CreateArgs(CreateString(message));
            var error = ErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateEvalError(string message)
        {
            var args = CreateArgs(CreateString(message));
            var error = EvalErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateRangeError(string message)
        {
            var args = CreateArgs(CreateString(message));
            var error = RangeErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateReferenceError(string message)
        {
            var args = CreateArgs(CreateString(message));
            var error = ReferenceErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateSyntaxError(string message)
        {
            var args = CreateArgs(CreateString(message));
            var error = SyntaxErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateTypeError(string message)
        {
            var args = CreateArgs(CreateString(message));
            var error = TypeErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }

        public MacheteRuntimeException CreateUriError(string message)
        {
            var args = CreateArgs(CreateString(message));
            var error = UriErrorConstructor.Op_Construct(args);
            return new MacheteRuntimeException(error);
        }
    }
}
