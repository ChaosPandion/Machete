using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Runtime.RuntimeTypes.SpecificationTypes;
using Machete.Runtime.NativeObjects.BuiltinObjects.ConstructorObjects;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PArray : LObject
    {
        internal PArray()
        {
            Class = "Array";
            Extensible = true;
            //DefineOwnProperty("length", SPropertyDescriptor.Create(LNumber.Zero), false);
            //DefineOwnProperty("toString", SPropertyDescriptor.Create(new NFunction(null, ToString)), false);
            //DefineOwnProperty("toLocaleString", SPropertyDescriptor.Create(new NFunction(null, ToLocaleString)), false);
            //DefineOwnProperty("concat", SPropertyDescriptor.Create(new NFunction(new[] { "item1" }, Concat)), false);
            //DefineOwnProperty("join", SPropertyDescriptor.Create(new NFunction(new[] { "separator" }, Join)), false);
            //DefineOwnProperty("pop", SPropertyDescriptor.Create(new NFunction(null, Pop)), false);
            //DefineOwnProperty("push", SPropertyDescriptor.Create(new NFunction(new[] { "item1" }, Push)), false);
            //DefineOwnProperty("reverse", SPropertyDescriptor.Create(new NFunction(null, Reverse)), false);
            //DefineOwnProperty("shift", SPropertyDescriptor.Create(new NFunction(null, Shift)), false);
            //DefineOwnProperty("slice", SPropertyDescriptor.Create(new FunctionObject("slice", new[] { "start", "end" }, Slice)), false);
            //DefineOwnProperty("sort", SPropertyDescriptor.Create(new FunctionObject("sort", new[] { "comparefn" }, Sort)), false);
            //DefineOwnProperty("splice", SPropertyDescriptor.Create(new FunctionObject("splice", new[] { "start", "deleteCount" }, Splice)), false);
            //DefineOwnProperty("unshift", SPropertyDescriptor.Create(new FunctionObject("unshift", new[] { "item1" }, Unshift)), false);
            //DefineOwnProperty("indexOf", SPropertyDescriptor.Create(new FunctionObject("indexOf", new[] { "searchElement" }, IndexOf)), false);
            //DefineOwnProperty("lastIndexOf", SPropertyDescriptor.Create(new FunctionObject("lastIndexOf", new[] { "searchElement" }, LastIndexOf)), false);
            //DefineOwnProperty("every", SPropertyDescriptor.Create(new FunctionObject("every", new[] { "callbackfn" }, Every)), false);
            //DefineOwnProperty("some", SPropertyDescriptor.Create(new FunctionObject("some", new[] { "callbackfn" }, Some)), false);
            //DefineOwnProperty("forEach", SPropertyDescriptor.Create(new FunctionObject("forEach", new[] { "callbackfn" }, ForEach)), false);
            //DefineOwnProperty("map", SPropertyDescriptor.Create(new FunctionObject("map", new[] { "callbackfn" }, Map)), false);
            //DefineOwnProperty("filter", SPropertyDescriptor.Create(new FunctionObject("filter", new[] { "callbackfn" }, Filter)), false);
            //DefineOwnProperty("reduce", SPropertyDescriptor.Create(new FunctionObject("reduce", new[] { "callbackfn" }, Reduce)), false);
            //DefineOwnProperty("reduceRight", SPropertyDescriptor.Create(new FunctionObject("filter", new[] { "reduceRight" }, ReduceRight)), false);
            //DefineOwnProperty("toString", SPropertyDescriptor.Create(new NFunction(null, () => ToString)), false);
            //DefineOwnProperty("valueOf", SPropertyDescriptor.Create(new NFunction(null, () => ValueOf)), false);
        }

        private LType ToString(ExecutionContext context, SList args)
        {
            var obj = context.ThisBinding.ConvertToObject();
            var func = obj.Get("join") as NFunction;
            if (func == null) return new LString(string.Format("[object, {0}]", obj.Class));
            return func.Call(obj, SList.Empty);  
        }

        private LType ToLocaleString(ExecutionContext context, SList args)
        {
            var obj = context.ThisBinding.ConvertToObject();
            var func = obj.Get("toString") as NFunction;
            if (func == null) Engine.ThrowTypeError(); 
            return func.Call(obj, SList.Empty);
        }

        private LType Concat(ExecutionContext context, SList args)
        {
            var obj = context.ThisBinding.ConvertToObject();
            var array = Engine.ConstructArray();
            var current = default(LType);
            var count = 0;

            for (int i = -1; i < args.Count; i++)
            {
                current = i < 0 ? obj : args[i];
                if (!(current is LObject))
                {
                    var desc = new SPropertyDescriptor(current, true, true, true);
                    array.DefineOwnProperty((count++).ToString(), desc, false);
                }
                else
                {
                    var inner = current as NArray;
                    if (inner != null)
                    {
                        var length = (uint)inner.Get("length").ConvertToUInt32().Value;
                        for (int j = 0; j < length; j++)
                        {
                            var key = j.ToString();
                            if (inner.HasProperty(key))
                            {
                                var desc = new SPropertyDescriptor(inner.Get(key), true, true, true);
                                array.DefineOwnProperty((count++).ToString(), desc, false);
                            }
                        }
                    }
                }
            }
            return array;
        }

        private LType Join(ExecutionContext context, SList args)
        {
            var obj = context.ThisBinding.ConvertToObject();
            var length = (uint)obj.Get("length").ConvertToUInt32().Value;
            var separator = ", ";

            if (length == 0)
            {
                return LString.Empty;
            }

            if (args.Count > 0)
            {
                separator = args[0].ConvertToString().Value;
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
            return new LString(sb.ToString());
        }
                
        private LType Pop(ExecutionContext context, SList args)
        {
            var obj = context.ThisBinding.ConvertToObject();
            var length = obj.Get("length").ConvertToUInt32();
            if (length.Value == 0.0) return LUndefined.Value;
            var index = (length.Value - 1).ToString();
            var element = obj.Get(index);
            obj.Delete(index, true);
            obj.Put("length", new LNumber(length.Value - 1.0), true);
            return element;
        }

        private LType Push(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //if (args.Count > 0)
            //{
            //    for (int i = 0; i < args.Count; i++, length++)
            //    {
            //        obj.Put(length.ToString(), args[i], true);
            //    }
            //    obj.Put("length", new NumberPrimitive(length), true);
            //}
            //return new NumberPrimitive(length);
        }

        private LType Reverse(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var middle = Math.Floor(length / 2D);
            //var lowerValue = default(IDynamic);
            //var lowerExists = default(bool);
            //var lowerKey = default(string);
            //var lower = 0D;
            //var upperValue = default(IDynamic);
            //var upperExists = default(bool);
            //var upperKey = default(string);
            //var upper = 0D;

            //while (lower != middle)
            //{
            //    upper = length - lower - 1D;
            //    lowerKey = lower.ToString();
            //    upperKey = upper.ToString();
            //    lowerValue = obj.Get(lowerKey);
            //    upperValue = obj.Get(upperKey);
            //    lowerExists = obj.HasProperty(lowerKey);
            //    upperExists = obj.HasProperty(upperKey);
            //    if (lowerExists && upperExists)
            //    {
            //        obj.Put(lowerKey, upperValue, true);
            //        obj.Put(upperKey, lowerValue, true);
            //    }
            //    else if (!lowerExists && upperExists)
            //    {
            //        obj.Put(lowerKey, upperValue, true);
            //        obj.Delete(upperKey, true);
            //    }
            //    else if (lowerExists && !upperExists)
            //    {
            //        obj.Delete(lowerKey, true);
            //        obj.Put(upperKey, lowerValue, true);
            //    }
            //    lower++;
            //}
            //return obj;
        }

        private LType Shift(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //if (length == 0)
            //{
            //    return new UndefinedPrimitive();
            //}
            //var first = obj.Get("0");
            //var from = "";
            //var to = "";
            //for (int i = 1; i < length; i++)
            //{
            //    from = i.ToString();
            //    to = (i - 1).ToString();
            //    if (obj.HasProperty(from))
            //    {
            //        obj.Put(to, obj.Get(from), true);
            //    }
            //    else
            //    {
            //        obj.Delete(to, true);
            //    }
            //}
            //obj.Delete((length - 1).ToString(), true);
            //obj.Put("length", new NumberPrimitive(length - 1D), true);
            //return first;
        }

        private LType Slice(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var result = (ArrayObject)ArrayConstructor.Instance.Value.Construct(Args.Empty);
            //var obj = engine.Context.ThisBinding.ToObject();
            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var start = Math.Floor(args[0].ToNumberPrimitive().Value);
            //var end = args[1].IsUndefined ? length : Math.Floor(args[1].ToNumberPrimitive().Value);
            //var currentIndex = (int)(start < 0 ? Math.Max(length + start, 0) : Math.Min(start, length)) - 1;
            //var endIndex = (int)(end < 0 ? Math.Max(length + end, 0) : Math.Min(end, length));
            //var splicedIndex = 0;

            //while (++currentIndex < endIndex)
            //{
            //    if (obj.HasProperty(currentIndex.ToString()))
            //    {
            //        result.DefineOwnProperty((splicedIndex++).ToString(), Property.Create(obj.Get(currentIndex.ToString()), true, true, true), false);
            //    }
            //}

            //return result;
        }

        private LType Sort(ExecutionContext context, SList args)
        {

            throw new NotImplementedException();

            //var obj = engine.Context.ThisBinding.ToObject();
            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var callback = args[0]; // function(x, y) { }
            //var isSparse = false;
            //var hasNonConfigurable = false;
            //var hasAccessor = false;
            //var hasNonWritable = false;
            //var protoHasProp = false;
            //var defined = false;

            //for (int i = 0; i < length; i++)
            //{
            //    var prop = obj.GetOwnProperty(i.ToString());
            //    if (prop.IsUndefined)
            //    {
            //        isSparse = true;
            //    }
            //    else
            //    {
            //        if (prop.IsData)
            //        {
            //            if (!prop.Configurable.ToBooleanPrimitive())
            //            {
            //                hasNonConfigurable = true;
            //            }
            //            if (!prop.Writable.ToBooleanPrimitive())
            //            {
            //                hasNonWritable = true;
            //            }
            //        }
            //        else if (prop.IsAccessor)
            //        {
            //            hasAccessor = true;
            //        }
            //    }
            //}

            //if (!callback.IsUndefined)
            //{
            //    var proto = obj.Prototype;
            //    if (proto != null)
            //    {
            //        for (int i = 0; i < length; i++)
            //        {
            //            if (proto.HasProperty(i.ToString()))
            //            {
            //                protoHasProp = true;
            //            }
            //        }
            //    }
            //}

            //defined = (isSparse && (hasNonConfigurable || hasNonWritable || hasAccessor || protoHasProp));
        }

        private LType Splice(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
        }

        private LType Unshift(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
        }

        private LType IndexOf(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;

            //if (length == 0)
            //{
            //    return new NumberPrimitive(-1D);
            //}

            //var searchElement = args[0];
            //var fromIndex = Math.Max(Math.Floor(args[1].ToNumberPrimitive().Value), 0D);

            //if (fromIndex >= length)
            //{
            //    return new NumberPrimitive(-1D);
            //}

            //for (int i = (int)fromIndex; i < length; i++)
            //{
            //    if (obj.HasProperty(i.ToString()))
            //    {
            //        if (obj.Get(i.ToString()).Execute(BinaryOp.StrictEqual, searchElement).ToBooleanPrimitive())
            //        {
            //            return new NumberPrimitive(i);
            //        }
            //    }
            //}

            //return new NumberPrimitive(-1D);
        }

        private LType LastIndexOf(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;

            //if (length == 0)
            //{
            //    return new NumberPrimitive(-1D);
            //}

            //var searchElement = args[0];
            //var fromIndex = Math.Max(Math.Floor(args[1].ToNumberPrimitive().Value), 0D);

            //if (fromIndex >= length)
            //{
            //    return new NumberPrimitive(-1D);
            //}
            //else if (fromIndex == 0D)
            //{
            //    fromIndex = (double)(length - 1);
            //}
            //else if (fromIndex > 0)
            //{
            //    fromIndex = Math.Min(fromIndex, (double)(length - 1));
            //}

            //for (int i = (int)fromIndex; i >= 0; --i)
            //{
            //    if (obj.HasProperty(i.ToString()))
            //    {
            //        if (obj.Get(i.ToString()).Execute(BinaryOp.StrictEqual, searchElement).ToBooleanPrimitive())
            //        {
            //            return new NumberPrimitive(i);
            //        }
            //    }
            //}

            //return new NumberPrimitive(-1D);
        }

        private LType Every(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var callback = args.Get<ICallable>(0);
            //var thisArg = args[1];

            //if (callback == null)
            //{
            //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
            //}

            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
            //for (int i = 0; i < length; i++)
            //{
            //    if (obj.HasProperty(i.ToString()))
            //    {
            //        callbackArgs[0] = obj.Get(i.ToString());
            //        callbackArgs[1] = new NumberPrimitive(i);
            //        if (!callback.Call(thisArg, callbackArgs).ToBooleanPrimitive())
            //        {
            //            return new BooleanPrimitive(false);
            //        }
            //    }
            //}
            //return new BooleanPrimitive(true);
        }

        private LType Some(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var callback = args.Get<ICallable>(0);
            //var thisArg = args[1];

            //if (callback == null)
            //{
            //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
            //}

            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
            //var key = default(string);
            //for (int i = 0; i < length; i++)
            //{
            //    key = i.ToString();
            //    if (obj.HasProperty(key))
            //    {
            //        callbackArgs[0] = obj.Get(key);
            //        callbackArgs[1] = new NumberPrimitive(i);
            //        if (callback.Call(thisArg, callbackArgs).ToBooleanPrimitive())
            //        {
            //            return new BooleanPrimitive(true);
            //        }
            //    }
            //}
            //return new BooleanPrimitive(false);
        }

        private LType ForEach(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var callback = args.Get<ICallable>(0);
            //var thisArg = args[1];

            //if (callback == null)
            //{
            //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
            //}

            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
            //var key = default(string);
            //for (int i = 0; i < length; i++)
            //{
            //    key = i.ToString();
            //    if (obj.HasProperty(key))
            //    {
            //        callbackArgs[0] = obj.Get(key);
            //        callbackArgs[1] = new NumberPrimitive(i);
            //        callback.Call(thisArg, callbackArgs);
            //    }
            //}
            //return new UndefinedPrimitive();
        }

        private LType Map(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var result = (ArrayObject)ArrayConstructor.Instance.Value.Construct(Args.Empty);
            //var obj = engine.Context.ThisBinding.ToObject();
            //var callback = args.Get<ICallable>(0);
            //var thisArg = args[1];

            //if (callback == null)
            //{
            //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
            //}

            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
            //var key = default(string);
            //for (int i = 0; i < length; i++)
            //{
            //    key = i.ToString();
            //    if (obj.HasProperty(key))
            //    {
            //        callbackArgs[0] = obj.Get(key);
            //        callbackArgs[1] = new NumberPrimitive(i);
            //        result.DefineOwnProperty(key, Property.Create(callback.Call(thisArg, callbackArgs), true, true, true), false);
            //    }
            //}
            //return result;
        }

        private LType Filter(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var result = (ArrayObject)ArrayConstructor.Instance.Value.Construct(Args.Empty);
            //var obj = engine.Context.ThisBinding.ToObject();
            //var callback = args.Get<ICallable>(0);
            //var thisArg = args[1];

            //if (callback == null)
            //{
            //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
            //}

            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var callbackArgs = new Args(new IDynamic[] { null, null, obj });
            //var key = default(string);
            //var to = 0D;
            //for (int i = 0; i < length; i++)
            //{
            //    key = i.ToString();
            //    if (obj.HasProperty(key))
            //    {
            //        callbackArgs[0] = obj.Get(key);
            //        callbackArgs[1] = new NumberPrimitive(i);
            //        if (callback.Call(thisArg, callbackArgs).ToBooleanPrimitive())
            //        {
            //            result.DefineOwnProperty(to.ToString(), Property.Create(callbackArgs[0], true, true, true), false);
            //            to++;
            //        }
            //    }
            //}
            //return result;
        }

        private LType Reduce(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var callback = args.Get<ICallable>(0);
            //var initialValue = args[1];

            //if (callback == null)
            //{
            //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
            //}
            //else if (length == 0 && args.Count == 1)
            //{
            //    throw new TypeError("For a zero length array an initial value is required.");
            //}

            //var callArgs = new Args(new IDynamic[] { null, null, null, obj });
            //var accumulator = args.Count == 1 ? default(IDynamic) : initialValue;
            //var index = -1;
            //var present = false;

            //while (++index < length)
            //{
            //    if ((present = obj.HasProperty(index.ToString())))
            //    {
            //        accumulator = obj.Get(index.ToString());
            //        break;
            //    }
            //}

            //if (!present)
            //{
            //    throw new TypeError("An accumulator value could not be found.");
            //}

            //while (++index < length)
            //{
            //    if (obj.HasProperty(index.ToString()))
            //    {
            //        callArgs[0] = accumulator;
            //        callArgs[1] = obj.Get(index.ToString());
            //        callArgs[2] = new NumberPrimitive(index);
            //        callArgs[3] = obj;
            //        accumulator = callback.Call(new UndefinedPrimitive(), callArgs);
            //    }
            //}

            //return accumulator;
        }

        private LType ReduceRight(ExecutionContext context, SList args)
        {
            throw new NotImplementedException();
            //var obj = engine.Context.ThisBinding.ToObject();
            //var length = (uint)obj.Get("length").ToNumberPrimitive().Value;
            //var callback = args.Get<ICallable>(0);
            //var initialValue = args[1];

            //if (callback == null)
            //{
            //    throw new TypeError("The first parameter 'callbackfn' must be callable.");
            //}
            //else if (length == 0 && args.Count == 1)
            //{
            //    throw new TypeError("For a zero length array an initial value is required.");
            //}

            //var callArgs = new Args(new IDynamic[] { null, null, null, obj });
            //var accumulator = args.Count == 1 ? default(IDynamic) : initialValue;
            //var index = length;
            //var present = false;

            //while (--index >= 0)
            //{
            //    if ((present = obj.HasProperty(index.ToString())))
            //    {
            //        accumulator = obj.Get(index.ToString());
            //        break;
            //    }
            //}

            //if (!present)
            //{
            //    throw new TypeError("An accumulator value could not be found.");
            //}

            //while (--index >= 0)
            //{
            //    if (obj.HasProperty(index.ToString()))
            //    {
            //        callArgs[0] = accumulator;
            //        callArgs[1] = obj.Get(index.ToString());
            //        callArgs[2] = new NumberPrimitive(index);
            //        callArgs[3] = obj;
            //        accumulator = callback.Call(new UndefinedPrimitive(), callArgs);
            //    }
            //}

            //return accumulator;
        }

        private bool IsSparse(LObject obj, int length)
        {
            throw new NotImplementedException();
            //for (int i = 0; i < length; i++)
            //{
            //    if (obj.GetOwnProperty(i.ToString()).IsUndefined)
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }
    }
}