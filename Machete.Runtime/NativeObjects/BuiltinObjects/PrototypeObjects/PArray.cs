using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System;
using System.Text;
using System.Globalization;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PArray : LObject
    {
        public PArray(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Array";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;
            DefineOwnProperty("length", Environment.CreateDataDescriptor(Environment.CreateNumber(0.0), false, false, false), false);
            DefineOwnProperty("constructor", Environment.CreateDataDescriptor(Environment.ArrayConstructor, true, false, true), false);
            base.Initialize();
        }

        [NativeFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var array = environment.Context.ThisBinding.ConvertToObject();
            var func = array.Get("join") as ICallable;
            if (func == null)
            {
                func = environment.ObjectPrototype.Get("toString") as ICallable;
            }
            return func.Call(environment, array, args);
        }

        [NativeFunction("toLocaleString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToLocaleString(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding.ConvertToObject();
            var arrayLen = o.Get("length");
            var len = (uint)arrayLen.ConvertToUInt32().BaseValue;
            var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

            if (len == 0)
            {
                return environment.CreateString(string.Empty);
            }

            var sb = new StringBuilder();
            {
                for (uint i = 0; i < len; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(separator);
                    }

                    var nextElement = o.Get(i.ToString());
                    switch (nextElement.TypeCode)
                    {
                        case LanguageTypeCode.Boolean:
                        case LanguageTypeCode.String:
                        case LanguageTypeCode.Number:
                        case LanguageTypeCode.Object:
                            var elementObj = nextElement.ConvertToObject();
                            var func = elementObj.Get("toLocaleString") as ICallable;
                            if (func == null)
                            {
                                throw environment.CreateTypeError("");
                            }
                            sb.Append(func.Call(environment, elementObj, environment.EmptyArgs).ConvertToString().BaseValue);
                            break;
                    }
                }
            }
            return environment.CreateString(sb.ToString());
        }

        [NativeFunction("concat", "item1"), DataDescriptor(true, false, true)]
        internal static IDynamic Concat(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var array = environment.ArrayConstructor.Op_Construct(environment.EmptyArgs);
            var current = default(IDynamic);
            var count = 0;

            for (int i = -1; i < args.Count; i++)
            {
                current = i < 0 ? obj : args[i];
                if (current.TypeCode != LanguageTypeCode.Object)
                {
                    var desc = environment.CreateDataDescriptor(current, true, true, true);
                    array.DefineOwnProperty((count++).ToString(), desc, false);
                }
                else
                {
                    var inner = current as NArray;
                    if (inner != null)
                    {
                        var length = (uint)inner.Get("length").ConvertToUInt32().BaseValue;
                        for (int j = 0; j < length; j++)
                        {
                            var key = j.ToString();
                            if (inner.HasProperty(key))
                            {
                                var desc = environment.CreateDataDescriptor(inner.Get(key), true, true, true);
                                array.DefineOwnProperty((count++).ToString(), desc, false);
                            }
                        }
                    }
                }
            }
            return array;
        }

        [NativeFunction("join", "separator"), DataDescriptor(true, false, true)]
        internal static IDynamic Join(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var separator = ", ";

            if (length == 0)
            {
                return environment.CreateString(string.Empty);
            }

            if (args.Count > 0)
            {
                separator = args[0].ConvertToString().BaseValue;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }
                sb.Append(obj.Get(i.ToString()).Value.ConvertToString().Value);
            }
            return environment.CreateString(sb.ToString());
        }

        [NativeFunction("pop"), DataDescriptor(true, false, true)]
        internal static IDynamic Pop(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = obj.Get("length").ConvertToUInt32().BaseValue;
            if (length == 0.0) return environment.Undefined;
            var index = (length - 1).ToString();
            var element = obj.Get(index);
            obj.Delete(index, true);
            obj.Put("length", environment.CreateNumber(length - 1.0), true);
            return element;
        }

        [NativeFunction("push", "item1"), DataDescriptor(true, false, true)]
        internal static IDynamic Push(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = obj.Get("length").ConvertToUInt32().BaseValue;
            if (args.Count > 0)
            {
                for (int i = 0; i < args.Count; i++, length++)
                {
                    obj.Put(length.ToString(), args[i], true);
                }
                obj.Put("length", environment.CreateNumber(length), true);
            }
            return environment.CreateNumber(length);
        }

        [NativeFunction("reverse"), DataDescriptor(true, false, true)]
        internal static IDynamic Reverse(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var middle = Math.Floor(length / 2D);
            var lowerValue = default(IDynamic);
            var lowerExists = default(bool);
            var lowerKey = default(string);
            var lower = 0D;
            var upperValue = default(IDynamic);
            var upperExists = default(bool);
            var upperKey = default(string);
            var upper = 0D;

            while (lower != middle)
            {
                upper = length - lower - 1D;
                lowerKey = lower.ToString();
                upperKey = upper.ToString();
                lowerValue = obj.Get(lowerKey);
                upperValue = obj.Get(upperKey);
                lowerExists = obj.HasProperty(lowerKey);
                upperExists = obj.HasProperty(upperKey);
                if (lowerExists && upperExists)
                {
                    obj.Put(lowerKey, upperValue, true);
                    obj.Put(upperKey, lowerValue, true);
                }
                else if (!lowerExists && upperExists)
                {
                    obj.Put(lowerKey, upperValue, true);
                    obj.Delete(upperKey, true);
                }
                else if (lowerExists && !upperExists)
                {
                    obj.Delete(lowerKey, true);
                    obj.Put(upperKey, lowerValue, true);
                }
                lower++;
            }
            return obj;
        }

        [NativeFunction("shift"), DataDescriptor(true, false, true)]
        internal static IDynamic Shift(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = obj.Get("length").ConvertToUInt32().BaseValue;
            if (length == 0)
            {
                return environment.Undefined;
            }
            var first = obj.Get("0");
            var from = "";
            var to = "";
            for (int i = 1; i < length; i++)
            {
                from = i.ToString();
                to = (i - 1).ToString();
                if (obj.HasProperty(from))
                {
                    obj.Put(to, obj.Get(from), true);
                }
                else
                {
                    obj.Delete(to, true);
                }
            }
            obj.Delete((length - 1).ToString(), true);
            obj.Put("length", environment.CreateNumber(length - 1D), true);
            return first;
        }

        internal static IDynamic Slice(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    throw new NotImplementedException();
        //    //var result = (ArrayObject)ArrayConstructor.Instance.Value.Construct(Args.Empty);
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
        //    //var start = Math.Floor(args[0].ToNumberPrimitive().Value);
        //    //var end = args[1].IsUndefined ? length : Math.Floor(args[1].ToNumberPrimitive().Value);
        //    //var currentIndex = (int)(start < 0 ? Math.Max(length + start, 0) : Math.Min(start, length)) - 1;
        //    //var endIndex = (int)(end < 0 ? Math.Max(length + end, 0) : Math.Min(end, length));
        //    //var splicedIndex = 0;

        //    //while (++currentIndex < endIndex)
        //    //{
        //    //    if (obj.HasProperty(currentIndex.ToString()))
        //    //    {
        //    //        result.DefineOwnProperty((splicedIndex++).ToString(), Property.Create(obj.Get(currentIndex.ToString()), true, true, true), false);
        //    //    }
        //    //}

        //    //return result;
        }

        internal static IDynamic Sort(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();

        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
        //    //var callback = args[0]; // function(x, y) { }
        //    //var isSparse = false;
        //    //var hasNonConfigurable = false;
        //    //var hasAccessor = false;
        //    //var hasNonWritable = false;
        //    //var protoHasProp = false;
        //    //var defined = false;

        //    //for (int i = 0; i < length; i++)
        //    //{
        //    //    var prop = obj.GetOwnProperty(i.ToString());
        //    //    if (prop.IsUndefined)
        //    //    {
        //    //        isSparse = true;
        //    //    }
        //    //    else
        //    //    {
        //    //        if (prop.IsData)
        //    //        {
        //    //            if (!prop.Configurable.ToBooleanPrimitive())
        //    //            {
        //    //                hasNonConfigurable = true;
        //    //            }
        //    //            if (!prop.Writable.ToBooleanPrimitive())
        //    //            {
        //    //                hasNonWritable = true;
        //    //            }
        //    //        }
        //    //        else if (prop.IsAccessor)
        //    //        {
        //    //            hasAccessor = true;
        //    //        }
        //    //    }
        //    //}

        //    //if (!callback.IsUndefined)
        //    //{
        //    //    var proto = obj.Prototype;
        //    //    if (proto != null)
        //    //    {
        //    //        for (int i = 0; i < length; i++)
        //    //        {
        //    //            if (proto.HasProperty(i.ToString()))
        //    //            {
        //    //                protoHasProp = true;
        //    //            }
        //    //        }
        //    //    }
        //    //}

        //    //defined = (isSparse && (hasNonConfigurable || hasNonWritable || hasAccessor || protoHasProp));
        }

        internal static IDynamic Splice(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        internal static IDynamic Unshift(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        }

        internal static IDynamic IndexOf(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;

        //    //if (length == 0)
        //    //{
        //    //    return new NumberPrimitive(-1D);
        //    //}

        //    //var searchElement = args[0];
        //    //var fromIndex = Math.Max(Math.Floor(args[1].ToNumberPrimitive().Value), 0D);

        //    //if (fromIndex >= length)
        //    //{
        //    //    return new NumberPrimitive(-1D);
        //    //}

        //    //for (int i = (int)fromIndex; i < length; i++)
        //    //{
        //    //    if (obj.HasProperty(i.ToString()))
        //    //    {
        //    //        if (obj.Get(i.ToString()).Execute(BinaryOp.StrictEqual, searchElement).ToBooleanPrimitive())
        //    //        {
        //    //            return new NumberPrimitive(i);
        //    //        }
        //    //    }
        //    //}

        //    //return new NumberPrimitive(-1D);
        }

        internal static IDynamic LastIndexOf(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;

        //    //if (length == 0)
        //    //{
        //    //    return new NumberPrimitive(-1D);
        //    //}

        //    //var searchElement = args[0];
        //    //var fromIndex = Math.Max(Math.Floor(args[1].ToNumberPrimitive().Value), 0D);

        //    //if (fromIndex >= length)
        //    //{
        //    //    return new NumberPrimitive(-1D);
        //    //}
        //    //else if (fromIndex == 0D)
        //    //{
        //    //    fromIndex = (double)(length - 1);
        //    //}
        //    //else if (fromIndex > 0)
        //    //{
        //    //    fromIndex = Math.Min(fromIndex, (double)(length - 1));
        //    //}

        //    //for (int i = (int)fromIndex; i >= 0; --i)
        //    //{
        //    //    if (obj.HasProperty(i.ToString()))
        //    //    {
        //    //        if (obj.Get(i.ToString()).Execute(BinaryOp.StrictEqual, searchElement).ToBooleanPrimitive())
        //    //        {
        //    //            return new NumberPrimitive(i);
        //    //        }
        //    //    }
        //    //}

        //    //return new NumberPrimitive(-1D);
        }

        internal static IDynamic Every(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var callback = args.Get<ICallable>(0);
        //    //var thisArg = args[1];

        //    //if (callback == null)
        //    //{
        //    //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
        //    //}

        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
        //    //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
        //    //for (int i = 0; i < length; i++)
        //    //{
        //    //    if (obj.HasProperty(i.ToString()))
        //    //    {
        //    //        callbackArgs[0] = obj.Get(i.ToString());
        //    //        callbackArgs[1] = new NumberPrimitive(i);
        //    //        if (!callback.Call(thisArg, callbackArgs).ToBooleanPrimitive())
        //    //        {
        //    //            return new BooleanPrimitive(false);
        //    //        }
        //    //    }
        //    //}
        //    //return new BooleanPrimitive(true);
        }

        internal static IDynamic Some(IEnvironment environment, IArgs args)
        {
           throw new NotImplementedException();
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var callback = args.Get<ICallable>(0);
        //    //var thisArg = args[1];

        //    //if (callback == null)
        //    //{
        //    //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
        //    //}

        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
        //    //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
        //    //var key = default(string);
        //    //for (int i = 0; i < length; i++)
        //    //{
        //    //    key = i.ToString();
        //    //    if (obj.HasProperty(key))
        //    //    {
        //    //        callbackArgs[0] = obj.Get(key);
        //    //        callbackArgs[1] = new NumberPrimitive(i);
        //    //        if (callback.Call(thisArg, callbackArgs).ToBooleanPrimitive())
        //    //        {
        //    //            return new BooleanPrimitive(true);
        //    //        }
        //    //    }
        //    //}
        //    //return new BooleanPrimitive(false);
        }

        internal static IDynamic ForEach(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var callback = args.Get<ICallable>(0);
        //    //var thisArg = args[1];

        //    //if (callback == null)
        //    //{
        //    //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
        //    //}

        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
        //    //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
        //    //var key = default(string);
        //    //for (int i = 0; i < length; i++)
        //    //{
        //    //    key = i.ToString();
        //    //    if (obj.HasProperty(key))
        //    //    {
        //    //        callbackArgs[0] = obj.Get(key);
        //    //        callbackArgs[1] = new NumberPrimitive(i);
        //    //        callback.Call(thisArg, callbackArgs);
        //    //    }
        //    //}
        //    //return new UndefinedPrimitive();
        }

        internal static IDynamic Map(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var result = (ArrayObject)ArrayConstructor.Instance.Value.Construct(Args.Empty);
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var callback = args.Get<ICallable>(0);
        //    //var thisArg = args[1];

        //    //if (callback == null)
        //    //{
        //    //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
        //    //}

        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
        //    //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
        //    //var key = default(string);
        //    //for (int i = 0; i < length; i++)
        //    //{
        //    //    key = i.ToString();
        //    //    if (obj.HasProperty(key))
        //    //    {
        //    //        callbackArgs[0] = obj.Get(key);
        //    //        callbackArgs[1] = new NumberPrimitive(i);
        //    //        result.DefineOwnProperty(key, Property.Create(callback.Call(thisArg, callbackArgs), true, true, true), false);
        //    //    }
        //    //}
        //    //return result;
        }

        internal static IDynamic Filter(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var result = (ArrayObject)ArrayConstructor.Instance.Value.Construct(Args.Empty);
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var callback = args.Get<ICallable>(0);
        //    //var thisArg = args[1];

        //    //if (callback == null)
        //    //{
        //    //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
        //    //}

        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
        //    //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
        //    //var key = default(string);
        //    //var to = 0D;
        //    //for (int i = 0; i < length; i++)
        //    //{
        //    //    key = i.ToString();
        //    //    if (obj.HasProperty(key))
        //    //    {
        //    //        callbackArgs[0] = obj.Get(key);
        //    //        callbackArgs[1] = new NumberPrimitive(i);
        //    //        if (callback.Call(thisArg, callbackArgs).ToBooleanPrimitive())
        //    //        {
        //    //            result.DefineOwnProperty(to.ToString(), Property.Create(callbackArgs[0], true, true, true), false);
        //    //            to++;
        //    //        }
        //    //    }
        //    //}
        //    //return result;
        }

        internal static IDynamic Reduce(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
        //    //var callback = args.Get<ICallable>(0);
        //    //var initialValue = args[1];

        //    //if (callback == null)
        //    //{
        //    //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
        //    //}
        //    //else if (length == 0 && args.Count == 1)
        //    //{
        //    //    throw new TypeError("For a zero length array an initial value is required.");
        //    //}

        //    //var callArgs = new Args(new IDynamic[] { null, null, null, obj });
        //    //var accumulator = args.Count == 1 ? default(IDynamic) : initialValue;
        //    //var index = -1;
        //    //var present = false;

        //    //while (++index < length)
        //    //{
        //    //    if ((present = obj.HasProperty(index.ToString())))
        //    //    {
        //    //        accumulator = obj.Get(index.ToString());
        //    //        break;
        //    //    }
        //    //}

        //    //if (!present)
        //    //{
        //    //    throw new TypeError("An accumulator value could not be found.");
        //    //}

        //    //while (++index < length)
        //    //{
        //    //    if (obj.HasProperty(index.ToString()))
        //    //    {
        //    //        callArgs[0] = accumulator;
        //    //        callArgs[1] = obj.Get(index.ToString());
        //    //        callArgs[2] = new NumberPrimitive(index);
        //    //        callArgs[3] = obj;
        //    //        accumulator = callback.Call(new UndefinedPrimitive(), callArgs);
        //    //    }
        //    //}

        //    //return accumulator;
        }

        internal static IDynamic ReduceRight(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
        //    //var obj = engine.Context.ThisBinding.ToObject();
        //    //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
        //    //var callback = args.Get<ICallable>(0);
        //    //var initialValue = args[1];

        //    //if (callback == null)
        //    //{
        //    //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
        //    //}
        //    //else if (length == 0 && args.Count == 1)
        //    //{
        //    //    throw new TypeError("For a zero length array an initial value is required.");
        //    //}

        //    //var callArgs = new Args(new IDynamic[] { null, null, null, obj });
        //    //var accumulator = args.Count == 1 ? default(IDynamic) : initialValue;
        //    //var index = length;
        //    //var present = false;

        //    //while (--index >= 0)
        //    //{
        //    //    if ((present = obj.HasProperty(index.ToString())))
        //    //    {
        //    //        accumulator = obj.Get(index.ToString());
        //    //        break;
        //    //    }
        //    //}

        //    //if (!present)
        //    //{
        //    //    throw new TypeError("An accumulator value could not be found.");
        //    //}

        //    //while (--index >= 0)
        //    //{
        //    //    if (obj.HasProperty(index.ToString()))
        //    //    {
        //    //        callArgs[0] = accumulator;
        //    //        callArgs[1] = obj.Get(index.ToString());
        //    //        callArgs[2] = new NumberPrimitive(index);
        //    //        callArgs[3] = obj;
        //    //        accumulator = callback.Call(new UndefinedPrimitive(), callArgs);
        //    //    }
        //    //}

        //    //return accumulator;
        }

        internal static bool IsSparse(LObject obj, int length)
        {
            throw new NotImplementedException();
        //    //for (int i = 0; i < length; i++)
        //    //{
        //    //    if (obj.GetOwnProperty(i.ToString()).IsUndefined)
        //    //    {
        //    //        return true;
        //    //    }
        //    //}
        //    //return false;
        }
        
        //private static bool IsSparse(IObject o)
        //{
        //    uint len = (uint)o.Get("length").ConvertToUInt32().BaseValue;
        //    for (uint i = 0; i < len; i++)
        //    {
        //        if (o.GetOwnProperty(i.ToString()) == null)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}