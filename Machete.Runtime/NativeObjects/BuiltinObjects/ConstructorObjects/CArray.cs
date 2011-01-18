using System;
using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CArray : BuiltinConstructor
    {
        public CArray(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.ArrayPrototype, false, false, false), false);
        }

        protected override IDynamic Call(IEnvironment environment, IArgs args)
        {
            // 15.4.1.1 Array ( [ item1 [ , item2 [ , … ] ] ] ) 

            return Construct(environment, args);
        }

        public override IObject Construct(IEnvironment environment, IArgs args)
        {
            var array = new NArray(Environment);
            array.Initialize();
            if (args.Count != 1)
            {
                // 15.4.2.1 new Array ( [ item0 [ , item1 [ , … ] ] ] )

                array.DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(args.Count), true, false, false), false);
                for (int i = 0; i < args.Count; i++)
                {
                    array.DefineOwnProperty(i.ToString(), Environment.CreateDataDescriptor(args[i], true, true, true), false);
                }
            }
            else
            {
                // 15.4.2.2 new Array (len) 

                var arg0 = args[0];
                if (arg0.TypeCode != LanguageTypeCode.Number)
                {
                    array.DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), true, false, false), false);
                    array.DefineOwnProperty("0", Environment.CreateDataDescriptor(arg0, true, true, true), false);
                }
                else
                {
                    var len = ((INumber)arg0).BaseValue;
                    var uint32 = arg0.ConvertToUInt32().BaseValue;
                    if (len != uint32)
                    {
                        throw Environment.CreateRangeError("The supplied length " + len + " does not fall into the unsigned 32-bit integer range.");
                    }
                    array.DefineOwnProperty("length", Environment.CreateDataDescriptor(arg0, true, false, false), false);
                }
            }

            return array;
        }
                
        [BuiltinFunction("isArray", "arg"), DataDescriptor(true, false, true)]
        internal static IDynamic IsArray(IEnvironment environment, IArgs args)
        {
            // 15.4.3.2 Array.isArray ( arg ) 

            var arg = args[0] as IObject;
            return environment.CreateBoolean(arg != null && arg.Class == "Array");
        }
    }
}