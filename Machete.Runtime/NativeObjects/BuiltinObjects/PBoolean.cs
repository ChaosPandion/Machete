using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    /// <summary>
    /// 15.6.4  Properties of the Boolean Prototype Object 
    /// </summary>
    public sealed class PBoolean : LObject
    {
        private BFunction _toString;
        private BFunction _valueOf;

        public PBoolean(IEnvironment environment)
            : base(environment)
        {

        }

        /// <summary>
        /// 15.6.4.2  Boolean.prototype.toString ( ) 
        /// </summary>
        public BFunction ToStringBuiltinFunction
        {
            get { return _toString; }
        }

        /// <summary>
        /// 15.6.4.3  Boolean.prototype.valueOf ( ) 
        /// </summary>
        public BFunction ValueOfBuiltinFunction
        {
            get { return _valueOf; }
        }

        public sealed override void Initialize()
        {
            Class = "Boolean"; 
            Extensible = true;
            Prototype = Environment.ObjectPrototype;

            _toString = new BFunction(Environment, ToString, ReadOnlyList<string>.Empty);
            _valueOf = new BFunction(Environment, ValueOf, ReadOnlyList<string>.Empty);

            new LObject.Builder(this)
            .SetAttributes(true, false, true)
            .AppendDataProperty("constructor", Environment.BooleanConstructor)
            .AppendDataProperty("toString", _toString)
            .AppendDataProperty("valueOf", _valueOf);
        }

        private static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var v = environment.Context.ThisBinding;
            switch (v.TypeCode)
            {
                case LanguageTypeCode.Boolean:
                    return environment.CreateString(((IBoolean)v).BaseValue ? "true" : "false");
                case LanguageTypeCode.Object:
                    var o = (IObject)v;
                    if (o.Class == "Boolean")
                    {
                        return environment.CreateString(((IBoolean)((IPrimitiveWrapper)o).PrimitiveValue).BaseValue ? "true" : "false");
                    }
                    break;
            }
            throw environment.CreateTypeError("");
        }

        private static IDynamic ValueOf(IEnvironment environment, IArgs args)
        {
            var v = environment.Context.ThisBinding;
            switch (v.TypeCode)
            {
                case LanguageTypeCode.Boolean:
                    return v;
                case LanguageTypeCode.Object:
                    var o = (IObject)v;
                    if (o.Class == "Boolean")
                    {
                        return ((IPrimitiveWrapper)o).PrimitiveValue;
                    }
                    break;
            }
            throw environment.CreateTypeError("");
        }
    }
}