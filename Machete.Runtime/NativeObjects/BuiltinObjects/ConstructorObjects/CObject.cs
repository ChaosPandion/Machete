using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CObject : BuiltinConstructor
    {
        public CObject(IEnvironment environment)
            : base(environment)
        {

        }

        public sealed override void Initialize()
        {
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(1.0), false, false, false), false);
            DefineOwnProperty("prototype", Environment.CreateDataDescriptor(Environment.ObjectPrototype, false, false, false), false);
            base.Initialize();
        }

        protected sealed override IDynamic Call(IEnvironment environment, IArgs args)
        {
            var value = args[0];
            switch (value.TypeCode)
            {
                case LanguageTypeCode.Undefined:
                case LanguageTypeCode.Null:
                    return ((IConstructable)this).Construct(environment, args);
                default:
                    return value.ConvertToObject();
            }
        }

        public sealed override IObject Construct(IEnvironment environment, IArgs args)
        {
            if (args.Count > 0)
            {
                var arg = args[0];
                switch (arg.TypeCode)
                {
                    case LanguageTypeCode.Boolean:
                    case LanguageTypeCode.String:
                    case LanguageTypeCode.Number:
                    case LanguageTypeCode.Object:
                        return arg.ConvertToObject();
                }
            }
            var obj = new LObject(environment);
            obj.Class = "Object";
            obj.Extensible = true;
            obj.Prototype = environment.ObjectPrototype;
            return obj;
        }

        [NativeFunction("getPrototypeOf", "O"), DataDescriptor(true, false, true)]
        internal static IDynamic GetPrototypeOf(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            return obj.Prototype;
        }

        [NativeFunction("getOwnPropertyDescriptor", "O", "P"), DataDescriptor(true, false, true)]
        internal static IDynamic GetOwnPropertyDescriptor(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            var p = args[1].ConvertToString().BaseValue;
            var desc = obj.GetOwnProperty(p);
            return environment.FromPropertyDescriptor(desc);
        }

        [NativeFunction("getOwnPropertyNames", "O"), DataDescriptor(true, false, true)]
        internal static IDynamic GetOwnPropertyNames(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            var array = environment.CreateArray();
            var index = 0;

            foreach (var name in obj)
            {
                var value = environment.CreateString(name);
                var desc = environment.CreateDataDescriptor(value, true, true, true);
                array.DefineOwnProperty((index++).ToString(), desc, false);
            }

            return array;
        }

        [NativeFunction("create", "O", "Properties"), DataDescriptor(true, false, true)]
        internal static IDynamic Create(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            var newObj = environment.CreateObject();
            newObj.Prototype = obj;
            if (args.Count > 1)
            {
                var props = args[1];
                if (props.TypeCode != LanguageTypeCode.Undefined)
                {
                    return DefineProperties(environment, environment.CreateArgs(new [] { newObj, props }));
                }
            }
            return newObj;
        }

        [NativeFunction("defineProperty", "O", "P", "Attributes"), DataDescriptor(true, false, true)]
        internal static IDynamic DefineProperty(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            var name = args[1].ConvertToString().BaseValue;

            if (args.Count < 2)
            {
                var desc = environment.CreateGenericDescriptor();
                obj.DefineOwnProperty(name, desc, true);
            }
            else  
            {
                var attributes = args[2] as IObject;
                if (attributes == null)
                {
                    throw environment.CreateTypeError("");
                }
                var desc = environment.ToPropertyDescriptor(attributes);
                obj.DefineOwnProperty(name, desc, true);
            }

            return obj;
        }

        [NativeFunction("defineProperties", "O", "Properties"), DataDescriptor(true, false, true)]
        internal static IDynamic DefineProperties(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            var props = args[1].ConvertToObject();
            foreach (var name in props)
            {
                var desc = environment.ToPropertyDescriptor(props.Get(name).ConvertToObject());
                obj.DefineOwnProperty(name, desc, true);
            }
            return obj;
        }

        [NativeFunction("seal", "O"), DataDescriptor(true, false, true)]
        internal static IDynamic Seal(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            foreach (var name in obj)
            {
                var desc = obj.GetOwnProperty(name);
                desc.Configurable = false;
                obj.DefineOwnProperty(name, desc, true);
            }

            obj.Extensible = false;
            return obj;
        }

        [NativeFunction("freeze", "O"), DataDescriptor(true, false, true)]
        internal static IDynamic Freeze(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            foreach (var name in obj)
            {
                var desc = obj.GetOwnProperty(name);
                desc.Configurable = false;
                if (desc.IsDataDescriptor)
                {
                    desc.Writable = false;
                }
                obj.DefineOwnProperty(name, desc, true);
            }

            obj.Extensible = false;
            return obj;
        }

        [NativeFunction("preventExtensions", "O"), DataDescriptor(true, false, true)]
        internal static IDynamic PreventExtensions(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            obj.Extensible = false;
            return obj;
        }

        [NativeFunction("isSealed", "O"), DataDescriptor(true, false, true)]
        internal static IDynamic IsSealed(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            foreach (var name in obj)
            {
                var desc = obj.GetOwnProperty(name);
                if (desc.Configurable ?? false)
                {
                    return environment.False;
                }
            }

            return environment.CreateBoolean(!obj.Extensible);
        }

        [NativeFunction("isFrozen", "O"), DataDescriptor(true, false, true)]
        internal static IDynamic IsFrozen(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            foreach (var name in obj)
            {
                var desc = obj.GetOwnProperty(name);
                if (desc.Configurable ?? false)
                {
                    return environment.False;
                }
                if (desc.IsDataDescriptor)
                {
                    if (desc.Writable ?? false)
                    {
                        return environment.False;
                    }
                }
                obj.DefineOwnProperty(name, desc, true);
            }

            return environment.CreateBoolean(!obj.Extensible);
        }

        [NativeFunction("isExtensible", "O"), DataDescriptor(true, false, true)]
        internal static IDynamic IsExtensible(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            return environment.CreateBoolean(obj.Extensible);
        }

        [NativeFunction("keys", "O"), DataDescriptor(true, false, true)]
        internal static IDynamic Keys(IEnvironment environment, IArgs args)
        {
            if (args[0].TypeCode != LanguageTypeCode.Object)
            {
                throw environment.CreateTypeError("");
            }
            var obj = args[0].ConvertToObject();
            var array = environment.CreateArray();
            var index = 0;

            foreach (var name in obj)
            {
                var desc = obj.GetOwnProperty(name);
                if (desc.Enumerable ?? false)
                {
                    desc = environment.CreateDataDescriptor(environment.CreateString(name), true, true, true);
                    array.DefineOwnProperty((index++).ToString(), desc, false);
                }
            }

            return array;
        }
    }
}