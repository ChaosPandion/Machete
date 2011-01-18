using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CRegExp : BuiltinConstructor
    {
        public CRegExp(IEnvironment environment)
            : base(environment)
        {

        }

        public sealed override void Initialize()
        {
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(2.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.RegExpPrototype, false, false, false), false);
            base.Initialize();
        }

        protected sealed override IDynamic Call(IEnvironment environment, IArgs args)
        {
            var patternObject = args[0] as IObject;
            if (patternObject != null && patternObject.Class == "RegExp")
            {
                return patternObject;
            }
            return Construct(environment, args);
        }

        public sealed override IObject Construct(IEnvironment environment, IArgs args)
        {
            var patternArg = args[0];
            var flagsArg = args[1];
            var pattern = "";
            var flags = "";
            bool global, ignoreCase, multiline; 

            var patternObject = patternArg as IObject;
            if (patternObject != null && patternObject.Class == "RegExp")
            {
                if (flagsArg.TypeCode != LanguageTypeCode.Undefined)
                {
                    throw environment.CreateTypeError("");
                }
                global = ((IBoolean)patternObject.Get("global")).BaseValue;
                ignoreCase = ((IBoolean)patternObject.Get("ignoreCase")).BaseValue;
                multiline = ((IBoolean)patternObject.Get("multiline")).BaseValue;
                pattern = ((IString)patternObject.Get("source")).BaseValue;
                if (global) flags += "g";
                if (ignoreCase) flags += "i";
                if (multiline) flags += "m";
            }
            else
            {
                if (patternArg.TypeCode != LanguageTypeCode.Undefined)
                {
                    pattern = patternArg.ConvertToString().BaseValue;
                }
                if (flagsArg.TypeCode != LanguageTypeCode.Undefined)
                {
                    flags = flagsArg.ConvertToString().BaseValue;
                }
                global = flags.Contains("g");
                ignoreCase = flags.Contains("i");
                multiline = flags.Contains("m");
            }
            
            var regExpObj = new NRegExp(environment);
            regExpObj.Class = "RegExp";
            regExpObj.Extensible = true;
            regExpObj.Prototype = environment.RegExpPrototype;
            regExpObj.Body = pattern;
            regExpObj.Flags = flags;
            regExpObj.RegExpMatcher = Machete.Compiler.RegExpParser.Parse(Environment, pattern, flags);
            regExpObj.DefineOwnProperty("source", environment.CreateDataDescriptor(environment.CreateString(pattern), false, false, false), false);
            regExpObj.DefineOwnProperty("global", environment.CreateDataDescriptor(environment.CreateBoolean(global), false, false, false), false);
            regExpObj.DefineOwnProperty("ignoreCase", environment.CreateDataDescriptor(environment.CreateBoolean(ignoreCase), false, false, false), false);
            regExpObj.DefineOwnProperty("multiline", environment.CreateDataDescriptor(environment.CreateBoolean(multiline), false, false, false), false);
            regExpObj.DefineOwnProperty("lastIndex", environment.CreateDataDescriptor(environment.CreateNumber(0.0), true, false, false), false);
            return regExpObj;
        }
    }
}