using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System;

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

        [NativeFunction("exec", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic Exec(IEnvironment environment, IArgs args)
        {
            var regExpObj = (NRegExp)environment.Context.ThisBinding;
            var result = regExpObj.RegExp.Exec(args[0].ConvertToString().BaseValue);
            if (regExpObj.RegExp.Global)
            {
                regExpObj.Put("lastIndex", environment.CreateNumber(regExpObj.RegExp.LastIndex), true);
            }
            var array = ((IConstructable)environment.ArrayConstructor).Construct(environment, environment.EmptyArgs);
            array.DefineOwnProperty("index", environment.CreateDataDescriptor(environment.CreateNumber(result.Index), true, true, true), true);
            array.DefineOwnProperty("input", environment.CreateDataDescriptor(environment.CreateString(result.Input), true, true, true), true);
            array.DefineOwnProperty("length", environment.CreateDataDescriptor(environment.CreateNumber(result.Length)), true);
            for (int i = 0; i < result.Length; i++)
            {
                array.DefineOwnProperty(i.ToString(), environment.CreateDataDescriptor(environment.CreateString(result[i]), true, true, true), true);
            }
            return array;
        }

        [NativeFunction("test", "string"), DataDescriptor(true, false, true)]
        internal static IDynamic Test(IEnvironment environment, IArgs args)
        {
            var regExpObj = (NRegExp)environment.Context.ThisBinding;
            return environment.CreateBoolean(regExpObj.RegExp.Test(args[0].ConvertToString().BaseValue));
        }

        [NativeFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var regExpObj = (NRegExp)environment.Context.ThisBinding;
            return environment.CreateString(regExpObj.RegExp.ToString());
        }
    }
}
