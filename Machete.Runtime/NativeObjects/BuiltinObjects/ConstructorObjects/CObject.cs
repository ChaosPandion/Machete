using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects
{
    public sealed class CObject : LObject, ICallable, IConstructable
    {
        public CObject(IEnvironment environment)
            : base(environment)
        {
            Class = "Function";
            Extensible = true;
            InitializeNativeFunctions();
        }

        IDynamic ICallable.Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
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

        IObject IConstructable.Construct(IEnvironment environment, IArgs args)
        {
            var obj = new LObject(environment);
            obj.Class = "Object";
            obj.Extensible = true;
            obj.Prototype = environment.ObjectPrototype;
            return obj;
        }

        [NativeFunction("getPrototypeOf", "O")]
        internal static IDynamic GetPrototypeOf(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }
            return obj.Prototype;
        }

        [NativeFunction("getOwnPropertyDescriptor", "O", "P")]
        internal static IDynamic GetOwnPropertyDescriptor(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }
            var p = args[1].ConvertToString().BaseValue;
            var desc = obj.GetOwnProperty(p);
            return environment.FromPropertyDescriptor(desc);
        }

        [NativeFunction("getOwnPropertyNames", "O")]
        internal static IDynamic GetOwnPropertyNames(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }

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

        [NativeFunction("create", "O", "Properties")]
        internal static IDynamic Create(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }

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

        [NativeFunction("defineProperty", "O", "P", "Attributes")]
        internal static IDynamic DefineProperty(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }

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
                    // Type Error
                }
                var desc = environment.ToPropertyDescriptor(attributes);
                obj.DefineOwnProperty(name, desc, true);
            }

            return obj;
        }

        [NativeFunction("defineProperties", "O", "Properties")]
        internal static IDynamic DefineProperties(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }

            var props = args[1].ConvertToObject();
            foreach (var name in props)
            {
                var desc = environment.ToPropertyDescriptor(props.Get(name).ConvertToObject());
                obj.DefineOwnProperty(name, desc, true);
            }
            return obj;
        }

        [NativeFunction("seal", "O")]
        internal static IDynamic Seal(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }

            foreach (var name in obj)
            {
                var desc = obj.GetOwnProperty(name);
                desc.Configurable = false;
                obj.DefineOwnProperty(name, desc, true);
            }

            obj.Extensible = false;
            return obj;
        }

        [NativeFunction("freeze", "O")]
        internal static IDynamic Freeze(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }

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

        [NativeFunction("preventExtensions", "O")]
        internal static IDynamic PreventExtensions(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }
            obj.Extensible = false;
            return obj;
        }

        [NativeFunction("isSealed", "O")]
        internal static IDynamic IsSealed(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }

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

        [NativeFunction("isFrozen", "O")]
        internal static IDynamic IsFrozen(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }

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

        [NativeFunction("isExtensible", "O")]
        internal static IDynamic IsExtensible(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }
            return environment.CreateBoolean(obj.Extensible);
        }

        [NativeFunction("keys", "O")]
        internal static IDynamic Keys(IEnvironment environment, IArgs args)
        {
            var obj = args[0] as IObject;
            if (obj == null)
            {
                // Type Error
            }

            var array = environment.CreateArray();
            var index = 0;

            foreach (var name in obj)
            {
                var desc = obj.GetOwnProperty(name);
                if (desc.Enumerable ?? false)
                {
                    array.DefineOwnProperty((index++).ToString(), desc, false);
                }
            }

            return array;
        }
    }
}