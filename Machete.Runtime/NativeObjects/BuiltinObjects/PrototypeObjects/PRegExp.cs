using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System;
using MatchResult = Machete.Compiler.RegExpParser.MatchResult;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PRegExp : LObject
    {
        public PRegExp(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "RegExp";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;
            DefineOwnProperty("constructor", Environment.CreateDataDescriptor(Environment.ArrayConstructor, true, false, true), false);
            base.Initialize();
        }

        [BuiltinFunction("exec", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic Exec(IEnvironment environment, IArgs args)
        {
            var r = (NRegExp)environment.Context.ThisBinding;
            var s = args[0].ConvertToString().BaseValue;
            var length = s.Length;
            var global = r.Get("global").ConvertToBoolean().BaseValue;
            var i = !global ? 0 : (int)r.Get("lastIndex").ConvertToInteger().BaseValue;
            int beginIndex = i;
            MatchResult result;

            while (true)
            {
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

            var str = environment.CreateString(result.matchState.input.Substring(beginIndex, result.matchState.endIndex - beginIndex));
            var desc = environment.CreateDataDescriptor(str, true, true, true);
            array.DefineOwnProperty("0", desc, true);

            for (int index = 0; index < n; index++)
            {
                str = environment.CreateString(captures[index]);
                desc = environment.CreateDataDescriptor(str, true, true, true);
                array.DefineOwnProperty((index + 1).ToString(), desc, true);
            }

            return array;
        }

        [BuiltinFunction("test", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic Test(IEnvironment environment, IArgs args)
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

        [BuiltinFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var regExpObj = (NRegExp)environment.Context.ThisBinding;
            return environment.CreateString("/" + regExpObj.Body + "/" + regExpObj.Flags);
        }
    }
}
