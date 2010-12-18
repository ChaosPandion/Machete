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

            ObjectConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(ObjectPrototype), false);
            FunctionConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(FunctionPrototype), false);
            ArrayConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(ArrayPrototype), false);
            StringConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(StringPrototype), false);
            BooleanConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(BooleanPrototype), false);
            NumberConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(NumberPrototype), false);
            DateConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(DatePrototype), false);
            RegExpConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(RegExpPrototype), false);
            ErrorConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(ErrorPrototype), false);
            EvalErrorConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(EvalErrorPrototype), false);
            RangeErrorConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(RangeErrorPrototype), false);
            ReferenceErrorConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(ReferenceErrorPrototype), false);
            SyntaxErrorConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(SyntaxErrorPrototype), false);
            TypeErrorConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(TypeErrorPrototype), false);
            UriErrorConstructor.DefineOwnProperty("prototype", new SPropertyDescriptor(UriErrorPrototype), false);

            ObjectPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(ObjectConstructor), false);
            FunctionPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(FunctionConstructor), false);
            ArrayPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(ArrayConstructor), false);
            StringPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(StringConstructor), false);
            BooleanPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(BooleanConstructor), false);
            NumberPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(NumberConstructor), false);
            DatePrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(DateConstructor), false);
            RegExpPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(RegExpConstructor), false);
            ErrorPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(ErrorConstructor), false);
            EvalErrorPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(EvalErrorConstructor), false);
            RangeErrorPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(RangeErrorConstructor), false);
            ReferenceErrorPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(ReferenceErrorConstructor), false);
            SyntaxErrorPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(SyntaxErrorConstructor), false);
            TypeErrorPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(TypeErrorConstructor), false);
            UriErrorPrototype.DefineOwnProperty("constructor", new SPropertyDescriptor(UriErrorConstructor), false);

            GlobalObject.DefineOwnProperty("Object", new SPropertyDescriptor(ObjectConstructor), false);
            GlobalObject.DefineOwnProperty("Function", new SPropertyDescriptor(FunctionConstructor), false);
            GlobalObject.DefineOwnProperty("Array", new SPropertyDescriptor(ArrayConstructor), false);
            GlobalObject.DefineOwnProperty("String", new SPropertyDescriptor(StringConstructor), false);
            GlobalObject.DefineOwnProperty("Boolean", new SPropertyDescriptor(BooleanConstructor), false);
            GlobalObject.DefineOwnProperty("Number", new SPropertyDescriptor(NumberConstructor), false);
            GlobalObject.DefineOwnProperty("Date", new SPropertyDescriptor(DateConstructor), false);
            GlobalObject.DefineOwnProperty("RegExp", new SPropertyDescriptor(RegExpConstructor), false);
            GlobalObject.DefineOwnProperty("Error", new SPropertyDescriptor(ErrorConstructor), false);
            GlobalObject.DefineOwnProperty("EvalError", new SPropertyDescriptor(EvalErrorConstructor), false);
            GlobalObject.DefineOwnProperty("RangeError", new SPropertyDescriptor(RangeErrorConstructor), false);
            GlobalObject.DefineOwnProperty("ReferenceError", new SPropertyDescriptor(ReferenceErrorConstructor), false);
            GlobalObject.DefineOwnProperty("SyntaxError", new SPropertyDescriptor(SyntaxErrorConstructor), false);
            GlobalObject.DefineOwnProperty("TypeError", new SPropertyDescriptor(TypeErrorConstructor), false);
            GlobalObject.DefineOwnProperty("URIError", new SPropertyDescriptor(UriErrorConstructor), false);
            GlobalObject.DefineOwnProperty("Math", new SPropertyDescriptor(MathObject), false);
            GlobalObject.DefineOwnProperty("JSON", new SPropertyDescriptor(JsonObject), false);
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

        public IObject CreateReferenceError()
        {
            throw new NotImplementedException();
        }

        public IObject CreateTypeError()
        {
            throw new NotImplementedException();
        }

        public IObject CreateSyntaxError()
        {
            throw new NotImplementedException();
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
    }
}
