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

namespace Machete.Runtime
{
    public sealed class Engine
    {
        internal static readonly ThreadLocal<Engine> Instance = new ThreadLocal<Engine>();
        private readonly ConcurrentQueue<Tuple<string, BlockingCollection<object>>> _messageQueue;
        private readonly Thread _backingThread;


        internal BGlobal GlobalObject { get; private set; }
        internal CObject ObjectConstructor { get; private set; }
        internal PObject ObjectPrototype { get; private set; }
        internal CFunction FunctionConstructor { get; private set; }
        internal PFunction FunctionPrototype { get; private set; }
        internal CArray ArrayConstructor { get; private set; }
        internal PArray ArrayPrototype { get; private set; }
        internal CString StringConstructor { get; private set; }
        internal PString StringPrototype { get; private set; }
        internal CBoolean BooleanConstructor { get; private set; }
        internal PBoolean BooleanPrototype { get; private set; }
        internal CNumber NumberConstructor { get; private set; }
        internal PNumber NumberPrototype { get; private set; }
        internal BMath MathObject { get; private set; }
        internal CDate DateConstructor { get; private set; }
        internal PDate DatePrototype { get; private set; }
        internal CRegExp RegExpConstructor { get; private set; }
        internal PRegExp RegExpPrototype { get; private set; }
        internal CError ErrorConstructor { get; private set; }
        internal PError ErrorPrototype { get; private set; }
        internal CEvalError EvalErrorConstructor { get; private set; }
        internal PEvalError EvalErrorPrototype { get; private set; }
        internal CRangeError RangeErrorConstructor { get; private set; }
        internal PRangeError RangeErrorPrototype { get; private set; }
        internal CReferenceError ReferenceErrorConstructor { get; private set; }
        internal PReferenceError ReferenceErrorPrototype { get; private set; }
        internal CSyntaxError SyntaxErrorConstructor { get; private set; }
        internal PSyntaxError SyntaxErrorPrototype { get; private set; }
        internal CTypeError TypeErrorConstructor { get; private set; }
        internal PTypeError TypeErrorPrototype { get; private set; }
        internal CUriError UriErrorConstructor { get; private set; }
        internal PUriError UriErrorPrototype { get; private set; }
        internal BJson JsonObject { get; private set; }
        internal SLexicalEnvironment GlobalEnvironment { get; private set; }


        private Engine()
        {
            _messageQueue = new ConcurrentQueue<Tuple<string, BlockingCollection<object>>>();
            _backingThread = new Thread(ProcessScripts);
            _backingThread.IsBackground = true;
            _backingThread.Start();
        }


        public object ExecuteScript(string script)
        {
            using (var channel = new BlockingCollection<object>())
            {
                var message = Tuple.Create(script, channel);
                _messageQueue.Enqueue(message);
                return channel.Take();
            }
        }
        
        private void ProcessScripts()
        {
            Tuple<string, BlockingCollection<object>> message;
            while (_messageQueue.TryDequeue(out message))
            {
                ProcessScript(message);
            }
        }

        private void ProcessScript(Tuple<string, BlockingCollection<object>> message)
        {

        }

        private void Initalize()
        {
            Instance.Value = this;

            GlobalObject = new BGlobal();
            MathObject = new BMath();
            JsonObject = new BJson();
            GlobalEnvironment = new SLexicalEnvironment(new SObjectEnvironmentRecord(GlobalObject, false), null);

            ObjectConstructor = new CObject();
            FunctionConstructor = new CFunction();
            ArrayConstructor = new CArray();
            StringConstructor = new CString();
            BooleanConstructor = new CBoolean();
            NumberConstructor = new CNumber();
            DateConstructor = new CDate();
            RegExpConstructor = new CRegExp();
            ErrorConstructor = new CError();
            EvalErrorConstructor = new CEvalError();
            RangeErrorConstructor = new CRangeError();
            ReferenceErrorConstructor = new CReferenceError();
            SyntaxErrorConstructor = new CSyntaxError();
            TypeErrorConstructor = new CTypeError();
            UriErrorConstructor = new CUriError();

            ObjectPrototype = new PObject();
            FunctionPrototype = new PFunction();
            ArrayPrototype = new PArray();
            StringPrototype = new PString();
            BooleanPrototype = new PBoolean();
            NumberPrototype = new PNumber();
            DatePrototype = new PDate();
            RegExpPrototype = new PRegExp();
            ErrorPrototype = new PError();
            EvalErrorPrototype = new PEvalError();
            RangeErrorPrototype = new PRangeError();
            ReferenceErrorPrototype = new PReferenceError();
            SyntaxErrorPrototype = new PSyntaxError();
            TypeErrorPrototype = new PTypeError();
            UriErrorPrototype = new PUriError();

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
        
        internal static NArray ConstructArray()
        {
            return (NArray)Instance.Value.ArrayConstructor.Op_Construct(SList.Empty);
        }


        internal static Exception ThrowEvalError()
        {
            Instance.Value.EvalErrorConstructor.Op_Construct(SList.Empty).Op_Throw();
            return null;
        }

        internal static Exception ThrowRangeError()
        {
            Instance.Value.RangeErrorConstructor.Op_Construct(SList.Empty).Op_Throw();
            return null;
        }

        internal static Exception ThrowReferenceError()
        {
            Instance.Value.ReferenceErrorConstructor.Op_Construct(SList.Empty).Op_Throw();
            return null;
        }

        internal static Exception ThrowSyntaxError()
        {
            Instance.Value.SyntaxErrorConstructor.Op_Construct(SList.Empty).Op_Throw();
            return null;
        }

        internal static Exception ThrowTypeError()
        {
            Instance.Value.TypeErrorConstructor.Op_Construct(SList.Empty).Op_Throw();
            return null;
        }

        internal static Exception ThrowUriError()
        {
            Instance.Value.UriErrorConstructor.Op_Construct(SList.Empty).Op_Throw();
            return null;
        }
    }
}
