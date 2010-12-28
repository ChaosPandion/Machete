using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.RegExp;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CRegExp : LObject, IBuiltinFunction
    {
        public CRegExp(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Function";
            Extensible = true;
            Prototype = Environment.FunctionPrototype;
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(2.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.RegExpPrototype, false, false, false), false);
            base.Initialize();
        }

        public IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            var patternObject = args[0] as IObject;
            if (patternObject != null && patternObject.Class == "RegExp")
            {
                return patternObject;
            }
            return Construct(environment, args);
        }

        public IObject Construct(IEnvironment environment, IArgs args)
        {
            var pattern = "";
            var flags = "";
            var global = false;
            var ignoreCase = false;
            var multiline = false;
            var patternArg = args[0];
            var flagsArg = args[1];
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
                if (flags != "")
                {
                    if (flags.Length > 3)
                    {
                        throw environment.CreateSyntaxError("There cannot be more than 3 characters in the flags parameter. Flags='" + flags + "'");
                    }
                    int gCount = 0, iCount = 0, mCount = 0;
                    foreach (var c in flags)
                    {
                        switch (c)
                        {
                            case 'g':
                                gCount++;
                                break;
                            case 'i':
                                iCount++;
                                break;
                            case 'm':
                                mCount++;
                                break;
                            default:
                                throw environment.CreateSyntaxError("The character '" + c + "' is not a valid flag. Flags='" + flags + "'");
                        }
                    }
                    if (gCount > 1)
                    {
                        throw environment.CreateSyntaxError("The 'g' flag cannot be specified more than once. Flags='" + flags + "'");
                    }
                    if (iCount > 1)
                    {
                        throw environment.CreateSyntaxError("The 'i' flag cannot be specified more than once. Flags='" + flags + "'");
                    }
                    if (mCount > 1)
                    {
                        throw environment.CreateSyntaxError("The 'm' flag cannot be specified more than once. Flags='" + flags + "'");
                    }
                    global = gCount == 1;
                    ignoreCase = iCount == 1;
                    multiline = mCount == 1;
                }
            }

            var options = RegExpOptions.None;
            options |= global ? RegExpOptions.Global : RegExpOptions.None;
            options |= ignoreCase ? RegExpOptions.IgnoreCase : RegExpOptions.None;
            options |= multiline ? RegExpOptions.Multiline : RegExpOptions.None;
            
            var regExpObj = new NRegExp(environment);
            regExpObj.Class = "RegExp";
            regExpObj.Extensible = true;
            regExpObj.Prototype = environment.RegExpPrototype;
            regExpObj.RegExp = new RegExp.RegExp(pattern, options);
            regExpObj.DefineOwnProperty("source", environment.CreateDataDescriptor(environment.CreateString(pattern), false, false, false), false);
            regExpObj.DefineOwnProperty("global", environment.CreateDataDescriptor(environment.CreateBoolean(global), false, false, false), false);
            regExpObj.DefineOwnProperty("ignoreCase", environment.CreateDataDescriptor(environment.CreateBoolean(ignoreCase), false, false, false), false);
            regExpObj.DefineOwnProperty("multiline", environment.CreateDataDescriptor(environment.CreateBoolean(multiline), false, false, false), false);
            regExpObj.DefineOwnProperty("lastIndex", environment.CreateDataDescriptor(environment.CreateNumber(0.0), true, false, false), false);
            return regExpObj;
        }

        public bool HasInstance(IDynamic value)
        {
            return Environment.Instanceof(value, this);
        }
    }
}
