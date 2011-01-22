using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    /// <summary>
    /// 15.2.4  Properties of the Object Prototype Object 
    /// </summary>
    public sealed class PObject : LObject
    {
        private BFunction _toString;
        private BFunction _toLocaleString;
        private BFunction _valueOf;
        private BFunction _hasOwnProperty;
        private BFunction _isPrototypeOf;
        private BFunction _propertyIsEnumerable;

        public PObject(IEnvironment environment)
            : base(environment)
        {

        }

        /// <summary>
        /// 15.2.4.2  Object.prototype.toString ( )  
        /// </summary>
        public BFunction ToStringBuiltinFunction
        {
            get { return _toString; }
        }

        /// <summary>
        /// 15.2.4.3  Object.prototype.toLocaleString ( )  
        /// </summary>
        public BFunction ToLocaleStringBuiltinFunction
        {
            get { return _toLocaleString; }
        }

        /// <summary>
        /// 15.2.4.4  Object.prototype.valueOf ( ) 
        /// </summary>
        public BFunction ValueOfBuiltinFunction
        {
            get { return _valueOf; }
        }

        /// <summary>
        /// 15.2.4.5  Object.prototype.hasOwnProperty (V) 
        /// </summary>
        public BFunction HasOwnPropertyBuiltinFunction
        {
            get { return _hasOwnProperty; }
        }

        /// <summary>
        /// 15.2.4.6  Object.prototype.isPrototypeOf (V) 
        /// </summary>
        public BFunction IsPrototypeOfBuiltinFunction
        {
            get { return _isPrototypeOf; }
        }

        /// <summary>
        /// 15.2.4.7  Object.prototype.propertyIsEnumerable (V) 
        /// </summary>
        public BFunction PropertyIsEnumerableBuiltinFunction
        {
            get { return _propertyIsEnumerable; }
        }

        public sealed override void Initialize()
        {
            Class = "Object";
            Extensible = true;
            Prototype = null;

            _toString = new BFunction(Environment, ToString, ReadOnlyList<string>.Empty);
            _toLocaleString = new BFunction(Environment, ToLocaleString, ReadOnlyList<string>.Empty);
            _valueOf = new BFunction(Environment, ValueOf, ReadOnlyList<string>.Empty);
            _hasOwnProperty = new BFunction(Environment, HasOwnProperty, new ReadOnlyList<string>("V"));
            _isPrototypeOf = new BFunction(Environment, IsPrototypeOf, new ReadOnlyList<string>("V"));
            _propertyIsEnumerable = new BFunction(Environment, PropertyIsEnumerable, new ReadOnlyList<string>("V"));

            new LObject.Builder(this)
            .SetAttributes(true, false, true)
            .AppendDataProperty("constructor", Environment.ObjectConstructor)
            .AppendDataProperty("toString", _toString)
            .AppendDataProperty("toLocaleString", _toLocaleString)
            .AppendDataProperty("valueOf", _valueOf)
            .AppendDataProperty("hasOwnProperty", _hasOwnProperty)
            .AppendDataProperty("isPrototypeOf", _isPrototypeOf)
            .AppendDataProperty("propertyIsEnumerable", _propertyIsEnumerable);
        }

        private static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            switch (o.TypeCode)
            {
                case LanguageTypeCode.Undefined:
                    return environment.CreateString("[object, Undefined]");
                case LanguageTypeCode.Null:
                    return environment.CreateString("[object, Null]");
                default:
                    return environment.CreateString(string.Format("[object, {0}]", o.ConvertToObject().Class));
            }
        }

        private static IDynamic ToLocaleString(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var func = obj.Get("toString") as ICallable;
            if (func == null)
            {
                throw environment.CreateTypeError("This object does not contain a 'toString' method.");
            }
            return func.Call(environment, obj, environment.EmptyArgs);
        }

        private static IDynamic ValueOf(IEnvironment environment, IArgs args)
        {
            return environment.Context.ThisBinding.ConvertToObject();
        }

        private static IDynamic HasOwnProperty(IEnvironment environment, IArgs args)
        {
            if (args.IsEmpty) return environment.False;
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty(args[0].ConvertToString().BaseValue);
            return environment.CreateBoolean(desc != null);
        }

        private static IDynamic IsPrototypeOf(IEnvironment environment, IArgs args)
        {
            var value = args[0] as LObject;
            var thisObj = environment.Context.ThisBinding.ConvertToObject();
            return environment.CreateBoolean(value != null && value.Prototype != null && value.Prototype == thisObj);
        }

        private static IDynamic PropertyIsEnumerable(IEnvironment environment, IArgs args)
        {
            if (args.IsEmpty) return environment.False;
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var desc = obj.GetOwnProperty(args[0].ConvertToString().ToString());
            return environment.CreateBoolean(desc != null && (desc.Enumerable ?? false));
        }
    }
}