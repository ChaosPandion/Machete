using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PFunction : LObject, ICallable
    {
        public PFunction(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Function";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;
            DefineOwnProperty("constructor", Environment.CreateDataDescriptor(Environment.FunctionConstructor, true, false, true), false);
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(0), false, false, false), false);
            base.Initialize();
        }

        public IDynamic Call(IEnvironment environment, IDynamic thisBinding, IArgs args)
        {
            return environment.Undefined;
        }

        [BuiltinFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var func = environment.Context.ThisBinding as NFunction;
            var formalParameters = string.Join(", ", func.FormalParameters);
            return environment.CreateString(string.Format("function {0}({1}) {{\n\t[body]\n}}", environment.Context.CurrentFunction, formalParameters));
        }

        [BuiltinFunction("apply", "thisArg", "argArray"), DataDescriptor(true, false, true)]
        internal static IDynamic Apply(IEnvironment environment, IArgs args)
        {
            var callable = environment.Context.ThisBinding as ICallable;
            if (callable == null)
            {
                throw environment.CreateTypeError("");
            }
            var thisArg = args[0];
            var argArray = args[1];
            switch (argArray.TypeCode)
            {
                case LanguageTypeCode.Undefined:
                case LanguageTypeCode.Null:
                    return callable.Call(environment, thisArg, environment.EmptyArgs);
                case LanguageTypeCode.Object:
                    throw environment.CreateTypeError("");
            }
            var argList = new List<IDynamic>();
            var argArrayObj = argArray.ConvertToObject();
            var len = argArrayObj.Get("length");
            switch (len.TypeCode)
            {
                case LanguageTypeCode.Undefined:
                case LanguageTypeCode.Null:
                    throw environment.CreateTypeError("");
            }
            var n = (uint)len.ConvertToUInt32().BaseValue;
            var index = (uint)0;
            while (index < n)
            {
                argList.Add(argArrayObj.Get(index.ToString()));
                index++;
            }
            var callArgs = environment.CreateArgs(argList);
            return callable.Call(environment, thisArg, callArgs);
        }

        [BuiltinFunction("call", "thisArg"), DataDescriptor(true, false, true)]
        internal static IDynamic Call(IEnvironment environment, IArgs args)
        {
            var callable = environment.Context.ThisBinding as ICallable;
            if (callable == null)
            {
                throw environment.CreateTypeError("");
            }
            var thisArg = args[0];
            var callArgs = environment.EmptyArgs;
            if (args.Count > 1)
            {
                callArgs = environment.CreateArgs(args.Skip(1));
            }
            return callable.Call(environment, thisArg, callArgs);
        }

        [BuiltinFunction("bind", "thisArg"), DataDescriptor(true, false, true)]
        internal static IDynamic Bind(IEnvironment environment, IArgs args)
        {
            var callable = environment.Context.ThisBinding as ICallable;
            if (callable == null)
            {
                throw environment.CreateTypeError("");
            }

            var target = (IObject)callable;
            var thisArg = args[0];
            var callArgs = environment.EmptyArgs;
            var func = new NBoundFunction(environment);

            if (args.Count > 1)
            {
                callArgs = environment.CreateArgs(args.Skip(1));
            }

            func.Class = "Function";
            func.Extensible = true;
            func.Prototype = environment.FunctionPrototype;
            func.TargetFunction = target;
            func.BoundThis = thisArg;
            func.BoundArguments = callArgs;

            var length = 0.0;
            if (target.Class == "Function")
            {
                length = callArgs.Count - target.Get("length").ConvertToUInt32().BaseValue;
            }

            var lengthNum = environment.CreateNumber(length);
            var thrower = environment.ThrowTypeErrorFunction;
            var desc = environment.CreateDataDescriptor(lengthNum, false, false, false);

            func.DefineOwnProperty("length", desc, false);
            desc = environment.CreateAccessorDescriptor(thrower, thrower, false, false);
            func.DefineOwnProperty("caller", desc, false);
            func.DefineOwnProperty("arguments", desc, false);

            return func;
        }
    }
}