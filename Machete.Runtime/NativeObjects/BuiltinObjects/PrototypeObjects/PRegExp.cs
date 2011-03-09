using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using MatchResult = Machete.Compiler.RegExpParser.MatchResult;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PRegExp : LObject
    {
        public BFunction ExecFunction { get; private set; }
        public BFunction TestFunction { get; private set; }
        public BFunction ToStringFunction { get; private set; }
        

        public PRegExp(IEnvironment environment)
            : base(environment)
        {

        }


        public override void Initialize()
        {
            Class = "RegExp";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;

            ExecFunction = new BFunction(Environment, Exec, new ReadOnlyList<string>("string"));
            TestFunction = new BFunction(Environment, Test, new ReadOnlyList<string>("string"));
            ToStringFunction = new BFunction(Environment, ToString, ReadOnlyList<string>.Empty);

            new LObject.Builder(this)
            .SetAttributes(true, false, true)
            .AppendDataProperty("constructor", Environment.RegExpConstructor)
            .AppendDataProperty("exec", ExecFunction)
            .AppendDataProperty("test", TestFunction)
            .AppendDataProperty("toString", ToStringFunction);
        }

        IDynamic Exec(IEnvironment environment, IArgs args)
        {
            var r = (NRegExp)environment.Context.ThisBinding;
            var s = args[0].ConvertToString().BaseValue;
            var length = s.Length;
            var global = r.Get("global").ConvertToBoolean().BaseValue;
            var i = !global ? 0 : (int)r.Get("lastIndex").ConvertToInteger().BaseValue;
            int beginIndex = 0;
            MatchResult result;

            while (true)
            {
                beginIndex = i;
                if (i < 0 || i > length)
                {
                    r.Put("length", environment.CreateNumber(0.0), true);
                    return environment.Null;
                }
                result = r.RegExpMatcher(s, i);
                if (result.success) break;
                i++;
            }

            if (global)
            {
                r.Put("length", environment.CreateNumber(result.matchState.endIndex), true);
            }

            var captures = result.matchState.captures;
            var n = captures.Length;
            var array = ((IConstructable)environment.ArrayConstructor).Construct(environment, environment.EmptyArgs);

            array.DefineOwnProperty("index", environment.CreateDataDescriptor(environment.CreateNumber(beginIndex), true, true, true), true);
            array.DefineOwnProperty("input", environment.CreateDataDescriptor(environment.CreateString(s), true, true, true), true);
            array.DefineOwnProperty("length", environment.CreateDataDescriptor(environment.CreateNumber(n + 1), null, null, null), true);

            IDynamic value = environment.CreateString(result.matchState.input.Substring(beginIndex, result.matchState.endIndex - beginIndex));
            var desc = environment.CreateDataDescriptor(value, true, true, true);
            array.DefineOwnProperty("0", desc, true);

            for (int index = 0; index < n; index++)
            {
                var v = captures[index];
                value = v == null ? (IDynamic)environment.Undefined : (IDynamic)environment.CreateString(v);
                desc = environment.CreateDataDescriptor(value, true, true, true);
                array.DefineOwnProperty((index + 1).ToString(), desc, true);
            }

            return array;
        }

        IDynamic Test(IEnvironment environment, IArgs args)
        {
            var regExpObj = (NRegExp)environment.Context.ThisBinding;
            var func = regExpObj.Get("exec") as ICallable;
            var result = func.Call(environment, regExpObj, args);
            switch (result.TypeCode)
            {
                case LanguageTypeCode.Null:
                    return environment.False;
                default:
                    return environment.True;
            }
        }

        IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var regExpObj = (NRegExp)environment.Context.ThisBinding;
            return environment.CreateString("/" + regExpObj.Body + "/" + regExpObj.Flags);
        }
    }
}