using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System;
using System.Text;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PArray : LObject
    {
        public BFunction ToStringBuiltinFunction { get; private set; }
        public BFunction ToLocaleStringBuiltinFunction { get; private set; }
        public BFunction ConcatBuiltinFunction { get; private set; }
        public BFunction JoinBuiltinFunction { get; private set; }
        public BFunction PopBuiltinFunction { get; private set; }
        public BFunction PushBuiltinFunction { get; private set; }
        public BFunction ReverseBuiltinFunction { get; private set; }
        public BFunction ShiftBuiltinFunction { get; private set; }
        public BFunction SliceBuiltinFunction { get; private set; }
        public BFunction SortBuiltinFunction { get; private set; }
        public BFunction SpliceBuiltinFunction { get; private set; }
        public BFunction UnshiftBuiltinFunction { get; private set; }
        public BFunction IndexOfBuiltinFunction { get; private set; }
        public BFunction LastIndexOfBuiltinFunction { get; private set; }
        public BFunction EveryBuiltinFunction { get; private set; }
        public BFunction SomeBuiltinFunction { get; private set; }
        public BFunction ForEachBuiltinFunction { get; private set; }
        public BFunction MapBuiltinFunction { get; private set; }
        public BFunction FilterBuiltinFunction { get; private set; }
        public BFunction ReduceBuiltinFunction { get; private set; }
        public BFunction ReduceRightBuiltinFunction { get; private set; }
        
        public PArray(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "Array";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;

            ToStringBuiltinFunction = new BFunction(Environment, ToString, ReadOnlyList<string>.Empty);
            ToLocaleStringBuiltinFunction = new BFunction(Environment, ToLocaleString, ReadOnlyList<string>.Empty);
            ConcatBuiltinFunction = new BFunction(Environment, Concat, new ReadOnlyList<string>("item1"));
            JoinBuiltinFunction = new BFunction(Environment, Join, new ReadOnlyList<string>("separator"));
            PopBuiltinFunction = new BFunction(Environment, Pop, ReadOnlyList<string>.Empty);
            PushBuiltinFunction = new BFunction(Environment, Push, new ReadOnlyList<string>("item1"));
            ReverseBuiltinFunction = new BFunction(Environment, Reverse, ReadOnlyList<string>.Empty);
            ShiftBuiltinFunction = new BFunction(Environment, Shift, ReadOnlyList<string>.Empty);
            SliceBuiltinFunction = new BFunction(Environment, Slice, new ReadOnlyList<string>("start", "end"));
            SortBuiltinFunction = new BFunction(Environment, Sort, new ReadOnlyList<string>("comparefn"));
            SpliceBuiltinFunction = new BFunction(Environment, Splice, new ReadOnlyList<string>("start", "deleteCount"));
            UnshiftBuiltinFunction = new BFunction(Environment, Unshift, new ReadOnlyList<string>("item1"));
            IndexOfBuiltinFunction = new BFunction(Environment, IndexOf, new ReadOnlyList<string>("searchElement"));
            LastIndexOfBuiltinFunction = new BFunction(Environment, LastIndexOf, new ReadOnlyList<string>("searchElement"));
            EveryBuiltinFunction = new BFunction(Environment, Every, new ReadOnlyList<string>("comparefn"));
            SomeBuiltinFunction = new BFunction(Environment, Some, new ReadOnlyList<string>("comparefn"));
            ForEachBuiltinFunction = new BFunction(Environment, ForEach, new ReadOnlyList<string>("comparefn"));
            MapBuiltinFunction = new BFunction(Environment, Map, new ReadOnlyList<string>("comparefn"));
            FilterBuiltinFunction = new BFunction(Environment, Filter, new ReadOnlyList<string>("comparefn"));
            ReduceBuiltinFunction = new BFunction(Environment, Reduce, new ReadOnlyList<string>("comparefn"));
            ReduceRightBuiltinFunction = new BFunction(Environment, ReduceRight, new ReadOnlyList<string>("comparefn"));

            new LObject.Builder(this)
            .SetAttributes(false, false, false)
            .AppendDataProperty("length", Environment.CreateNumber(0.0))
            .SetAttributes(true, false, true)
            .AppendDataProperty("constructor", Environment.ArrayConstructor)
            .AppendDataProperty("toString", ToStringBuiltinFunction)
            .AppendDataProperty("toLocaleString", ToLocaleStringBuiltinFunction)
            .AppendDataProperty("concat", ConcatBuiltinFunction)
            .AppendDataProperty("join", JoinBuiltinFunction)
            .AppendDataProperty("pop", PopBuiltinFunction)
            .AppendDataProperty("push", PushBuiltinFunction)
            .AppendDataProperty("reverse", ReverseBuiltinFunction)
            .AppendDataProperty("shift", ShiftBuiltinFunction)
            .AppendDataProperty("slice", SliceBuiltinFunction)
            .AppendDataProperty("sort", SortBuiltinFunction)
            .AppendDataProperty("splice", SpliceBuiltinFunction)
            .AppendDataProperty("unshift", UnshiftBuiltinFunction)
            .AppendDataProperty("indexOf", IndexOfBuiltinFunction)
            .AppendDataProperty("lastIndexOf", LastIndexOfBuiltinFunction)
            .AppendDataProperty("every", EveryBuiltinFunction)
            .AppendDataProperty("some", SomeBuiltinFunction)
            .AppendDataProperty("forEach", ForEachBuiltinFunction)
            .AppendDataProperty("map", MapBuiltinFunction)
            .AppendDataProperty("filter", FilterBuiltinFunction)
            .AppendDataProperty("reduce", ReduceBuiltinFunction)
            .AppendDataProperty("reduceRight", ReduceRightBuiltinFunction);
        }

        private static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            var array = environment.Context.ThisBinding.ConvertToObject();
            var func = array.Get("join") as ICallable;
            if (func == null)
            {
                func = environment.ObjectPrototype.Get("toString") as ICallable;
            }
            return func.Call(environment, array, args);
        }

        private static IDynamic ToLocaleString(IEnvironment environment, IArgs args)
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

        private static IDynamic Concat(IEnvironment environment, IArgs args)
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

        private static IDynamic Join(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var separator = ",";

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
                if (sb.Length > 0 || i == 1)
                {
                    sb.Append(separator);
                }
                var element = obj.Get(i.ToString()).Value;
                switch (element.TypeCode)
                {
                    case LanguageTypeCode.Undefined:
                    case LanguageTypeCode.Null:
                        break;
                    default:
                        sb.Append(element.ConvertToString().Value);
                        break;
                }
            }
            return environment.CreateString(sb.ToString());
        }

        private static IDynamic Pop(IEnvironment environment, IArgs args)
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

        private static IDynamic Push(IEnvironment environment, IArgs args)
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

        private static IDynamic Reverse(IEnvironment environment, IArgs args)
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

        private static IDynamic Shift(IEnvironment environment, IArgs args)
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

        private static IDynamic Slice(IEnvironment environment, IArgs args)
        {
            var result = ((IConstructable)environment.ArrayConstructor).Construct(environment, environment.EmptyArgs);
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = (uint)obj.Get("length").ConvertToNumber().BaseValue;
            var start = Math.Floor(args[0].ConvertToNumber().BaseValue);
            var end = args[1] is IUndefined ? length : Math.Floor(args[1].ConvertToNumber().BaseValue);
            var currentIndex = (int)(start < 0 ? Math.Max(length + start, 0) : Math.Min(start, length)) - 1;
            var endIndex = (int)(end < 0 ? Math.Max(length + end, 0) : Math.Min(end, length));
            var splicedIndex = 0;

            while (++currentIndex < endIndex)
            {
                if (obj.HasProperty(currentIndex.ToString()))
                {
                    var key = currentIndex.ToString();
                    var value = obj.Get(key);
                    var desc = environment.CreateDataDescriptor(value, true, true,true);
                    result.DefineOwnProperty((splicedIndex++).ToString(), desc, false);
                }
            }

            return result;
        }

        private static IDynamic Sort(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var len = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var comparefn = args[0];

            var sortCompare = new Func<uint, uint, IDynamic>((j, k) =>
            {
                var jString = j.ToString();
                var kString = k.ToString();
                var hasJ = obj.HasProperty(jString);
                var hasK = obj.HasProperty(kString);
                if (!hasJ || !hasK)
                {
                    if (!hasJ && !hasK) return environment.CreateNumber(0);
                    else if (!hasJ) return environment.CreateNumber(1);
                    else return environment.CreateNumber(-1);
                }
                var x = obj.Get(jString);
                var y = obj.Get(kString);
                if (x is IUndefined || y is IUndefined)
                {
                    if (x is IUndefined && y is IUndefined) return environment.CreateNumber(0);
                    else if (x is IUndefined) return environment.CreateNumber(1);
                    else return environment.CreateNumber(-1);
                }

                if (comparefn.TypeCode != LanguageTypeCode.Undefined)
                {
                    var callable = comparefn as ICallable;
                    if (callable == null)
                    {
                        throw environment.CreateTypeError("");
                    }
                    return callable.Call(environment, environment.Undefined, environment.CreateArgs(new[] { x, y }));
                }

                var xString = x.ConvertToString();
                var yString = y.ConvertToString();
                var xLess = xString.Op_Lessthan(yString);
                if (((IBoolean)xLess).BaseValue) return environment.CreateNumber(-1);
                var yLess = yString.Op_Lessthan(xString);
                if (((IBoolean)yLess).BaseValue) return environment.CreateNumber(1);
                return environment.CreateNumber(0);
            });

            var items = new List<IDynamic>();
            for (uint i = 0; i < len; i++)
            {
                var key = i.ToString();
                if (obj.HasProperty(key))
                {
                    var value = obj.Get(key);
                    items.Add(value);
                    obj.Delete(key, true);
                }
            }

            items.Sort((x, y) =>
            {
                if (x is IUndefined || y is IUndefined)
                {
                    if (x is IUndefined && y is IUndefined) return 0;
                    else if (x is IUndefined) return 1;
                    else return -1;
                }

                if (comparefn.TypeCode != LanguageTypeCode.Undefined)
                {
                    var callable = comparefn as ICallable;
                    if (callable == null)
                    {
                        throw environment.CreateTypeError("");
                    }
                    var result = callable.Call(environment, environment.Undefined, environment.CreateArgs(new[] { x, y }));
                    var number = result.ConvertToInteger();
                    if (double.IsNaN(number.BaseValue) || double.IsInfinity(number.BaseValue))
	                {
		                return 0;
	                }
                    return (int)number.BaseValue;
                }

                var xString = x.ConvertToString();
                var yString = y.ConvertToString();
                var xLess = xString.Op_Lessthan(yString);
                if (((IBoolean)xLess).BaseValue) return -1;
                var yLess = yString.Op_Lessthan(xString);
                if (((IBoolean)yLess).BaseValue) return 1;
                return 0;
            });

            for (int i = 0; i < items.Count; i++)
            {
                var key = i.ToString();
                obj.Put(key, items[i], true);
            }

            return obj;
        }

        private static IDynamic Splice(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var a = ((IConstructable)environment.ArrayConstructor).Construct(environment, environment.EmptyArgs);
            var len = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var start = args[0].ConvertToInteger().BaseValue;
            var deleteCount = args[1].ConvertToInteger().BaseValue;
            var actualStart = start < 0 ? (int)Math.Max(len + start, 0) : (int)Math.Min(start, len);
            var actualDeleteCount = (int)Math.Min(Math.Max(deleteCount, 0), len - actualStart);

            for (var k = 0; k < actualDeleteCount; k++)
            {
                var from = (actualStart + k).ToString();
                if (obj.HasProperty(from))
                {
                    var fromValue = obj.Get(from);
                    var desc = environment.CreateDataDescriptor(fromValue, true, true, true);
                    var key = k.ToString();
                    a.DefineOwnProperty(key, desc, false);
                }
            }

            var itemCount = 0;
            if (args.Count > 2)
            {
                itemCount = args.Count;
            }

            if (itemCount < actualDeleteCount)
            {
                var limit = len - actualDeleteCount;
                for (var k = 0; k < limit; k++)
                {
                    var from = (k + actualDeleteCount).ToString();
                    var to = (k + itemCount).ToString();
                    if (!obj.HasProperty(from))
                    {
                        obj.Delete(to, true);
                    }
                    else
                    {
                        var fromValue = obj.Get(from);
                        obj.Put(to, fromValue, true);
                    }
                }
                var deleteLimit = len - actualDeleteCount + itemCount;
                for (var k = len; k > deleteLimit; k--)
                {
                    obj.Delete((k - 1).ToString(), true);
                }
            }
            else if (itemCount > actualDeleteCount)
            {
                for (var k = len - actualDeleteCount; k > actualStart; k--)
                {
                    var from = (k + actualDeleteCount - 1).ToString();
                    var to = (k + itemCount - 1).ToString();
                    if (!obj.HasProperty(from))
                    {
                        obj.Delete(to, true);
                    }
                    else
                    {
                        var fromValue = obj.Get(from);
                        obj.Put(to, fromValue, true);
                    }
                }
            }

            for (int k = actualStart, i = 2; i < args.Count; k++, i++)
            {
                obj.Put(k.ToString(), args[i], true);
            }

            return a;
        }

        private static IDynamic Unshift(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var len = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var argCount = (uint)args.Count;
            var k = len;
            while (k > 0)
            {
                var from = (k - 1).ToString();
                var to = (k + argCount - 1).ToString();
                if (obj.HasProperty(from))
                {
                    var fromValue = obj.Get(from);
                    obj.Put(to, fromValue, true);
                }
                else
                {
                    obj.Delete(to, true);
                }
                k--;
            }
            for (int i = 0; i < argCount; i++)
            {
                obj.Put(i.ToString(), args[i], true);
            }
            var newLength = environment.CreateNumber(len + argCount);
            obj.Put("length", newLength, true);
            return newLength;
        }

        private static IDynamic IndexOf(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;

            if (length == 0)
            {
                return environment.CreateNumber(-1);
            }

            var searchElement = args[0];
            var fromIndex = (uint)Math.Max(Math.Floor(args[1].ConvertToNumber().BaseValue), 0D);

            if (fromIndex >= length)
            {
                return environment.CreateNumber(-1);
            }

            for (uint i = 0; i < length; i++)
            {
                var key = i.ToString();
                if (obj.HasProperty(key))
                {
                    if (obj.Get(key).Op_StrictEquals(searchElement).ConvertToBoolean().BaseValue)
                    {
                        return environment.CreateNumber(i);
                    }
                }
            }

            return environment.CreateNumber(-1);
        }

        private static IDynamic LastIndexOf(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;

            if (length == 0)
            {
                return environment.CreateNumber(-1);
            }

            var searchElement = args[0];
            var fromIndex = (uint)Math.Max(Math.Floor(args[1].ConvertToNumber().BaseValue), 0D);

            if (fromIndex >= length)
            {
                return environment.CreateNumber(-1);
            }
            else if (fromIndex == 0)
            {
                fromIndex = length - 1;
            }
            else if (fromIndex > 0)
            {
                fromIndex = Math.Min(fromIndex, (length - 1));
            }

            for (uint i = fromIndex; i >= 0; --i)
            {
                var key = i.ToString();
                if (obj.HasProperty(key))
                {
                    if (obj.Get(key).Op_StrictEquals(searchElement).ConvertToBoolean().BaseValue)
                    {
                        return environment.CreateNumber(i);
                    }
                }
            }

            return environment.CreateNumber(-1);
        }

        private static IDynamic Every(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var callArgs = new IDynamic[] { null, null, obj };
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var callbackfn = args[0] as ICallable;
            var thisArg = args[1];

            if (callbackfn == null)
            {
                throw environment.CreateTypeError("The first parameter 'callbackfn' must be callable.");
            }

            for (uint i = 0; i < length; i++)
            {
                var key = i.ToString();
                if (obj.HasProperty(key))
                {
                    callArgs[0] = obj.Get(key);
                    callArgs[1] = environment.CreateNumber(i);
                    if (!callbackfn.Call(environment, thisArg, environment.CreateArgs(callArgs)).ConvertToBoolean().BaseValue)
                    {
                        return environment.False;
                    }
                }
            }

            return environment.True;
        }

        private static IDynamic Some(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var callArgs = new IDynamic[] { null, null, obj };
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var callbackfn = args[0] as ICallable;
            var thisArg = args[1];

            if (callbackfn == null)
            {
                throw environment.CreateTypeError("The first parameter 'callbackfn' must be callable.");
            }

            for (uint i = 0; i < length; i++)
            {
                var key = i.ToString();
                if (obj.HasProperty(key))
                {
                    callArgs[0] = obj.Get(key);
                    callArgs[1] = environment.CreateNumber(i);
                    if (callbackfn.Call(environment, thisArg, environment.CreateArgs(callArgs)).ConvertToBoolean().BaseValue)
                    {
                        return environment.True;
                    }
                }
            }

            return environment.False;
        }

        private static IDynamic ForEach(IEnvironment environment, IArgs args)
        {
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var callArgs = new IDynamic[] { null, null, obj };
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var callbackfn = args[0] as ICallable;
            var thisArg = args[1];

            if (callbackfn == null)
            {
                throw environment.CreateTypeError("The first parameter 'callbackfn' must be callable.");
            }

            for (uint i = 0; i < length; i++)
            {
                var key = i.ToString();
                if (obj.HasProperty(key))
                {
                    callArgs[0] = obj.Get(key);
                    callArgs[1] = environment.CreateNumber(i);
                    callbackfn.Call(environment, thisArg, environment.CreateArgs(callArgs));
                }
            }

            return environment.Undefined;
        }

        private static IDynamic Map(IEnvironment environment, IArgs args)
        {
            var result = ((IConstructable)environment.ArrayConstructor).Construct(environment, environment.EmptyArgs);
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var callArgs = new IDynamic[] { null, null, obj };
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var callbackfn = args[0] as ICallable;
            var thisArg = args[1];

            if (callbackfn == null)
            {
                throw environment.CreateTypeError("The first parameter 'callbackfn' must be callable.");
            }

            for (uint i = 0; i < length; i++)
            {
                var key = i.ToString();
                if (obj.HasProperty(key))
                {
                    callArgs[0] = obj.Get(key);
                    callArgs[1] = environment.CreateNumber(i);
                    var value = callbackfn.Call(environment, thisArg, environment.CreateArgs(callArgs));
                    result.DefineOwnProperty(key, environment.CreateDataDescriptor(value, true, true, true), false);
                }
            }

            return result;
        }

        private static IDynamic Filter(IEnvironment environment, IArgs args)
        {
            var result = ((IConstructable)environment.ArrayConstructor).Construct(environment, environment.EmptyArgs);
            var obj = environment.Context.ThisBinding.ConvertToObject();
            var length = (uint)obj.Get("length").ConvertToUInt32().BaseValue;
            var callbackfn = args[0] as ICallable;
            var thisArg = args[1];

            if (callbackfn == null)
                throw environment.CreateTypeError("");

            for (uint i = 0; i < length; i++)
            {
                var key = i.ToString();
                if (obj.HasProperty(key))
                {
                    var value = obj.Get(key);
                    var callArgs = environment.CreateArgs(new[] { value, environment.CreateNumber(i), obj });
                    var include = callbackfn.Call(environment, thisArg, callArgs);
                    if (include.ConvertToBoolean().BaseValue)
                    {
                        result.DefineOwnProperty(key, environment.CreateDataDescriptor(value, true, true, true), false);
                    }
                }
            }

            return result;
        }

        private static IDynamic Reduce(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding.ConvertToObject();
            var len = (uint)o.Get("length").ConvertToUInt32().BaseValue;
            var callbackfn = args[0] as ICallable;

            if (callbackfn == null)
                throw environment.CreateTypeError("");
            if (len == 0 && args.Count < 2)
                throw environment.CreateTypeError("");

            uint k = 0;
            var accumulator = args[1];
            if (args.Count < 2)
            {
                var found = false;
                for (; k < len; k++)
                {
                    var key = k.ToString();
                    if (o.HasProperty(key))
                    {
                        found = true;
                        accumulator = o.Get(key);
                        k++;
                        break;
                    }
                }
                if (!found)
                    throw environment.CreateTypeError("");
            }
            for (; k < len; k++)
            {
                var key = k.ToString();
                if (o.HasProperty(key))
                {
                    var callArgs = environment.CreateArgs(new[] { accumulator, o.Get(key), environment.CreateNumber(k), o });
                    accumulator = callbackfn.Call(environment, environment.Undefined, callArgs);
                }
            }
            return accumulator;
        }

        private static IDynamic ReduceRight(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding.ConvertToObject();
            var len = (uint)o.Get("length").ConvertToUInt32().BaseValue;
            var callbackfn = args[0] as ICallable;

            if (callbackfn == null)
                throw environment.CreateTypeError("");
            if (len == 0 && args.Count < 2)
                throw environment.CreateTypeError("");

            uint k = len - 1;
            var accumulator = args[1];
            if (args.Count < 2)
            {
                var found = false;
                for (;; k--)
                {
                    var key = k.ToString();
                    if (o.HasProperty(key))
                    {
                        found = true;
                        accumulator = o.Get(key);
                        k--;
                        break;
                    }
                    if (k == 0) break;
                }
                if (!found)
                    throw environment.CreateTypeError("");
            }
            for (;; k--)
            {
                var key = k.ToString();
                if (o.HasProperty(key))
                {
                    var callArgs = environment.CreateArgs(new[] { accumulator, o.Get(key), environment.CreateNumber(k), o });
                    accumulator = callbackfn.Call(environment, environment.Undefined, callArgs);
                }
                if (k == 0) break;
            }
            return accumulator;
        }

        private static bool IsSparse(IObject o)
        {
            uint len = (uint)o.Get("length").ConvertToUInt32().BaseValue;
            for (uint i = 0; i < len; i++)
            {
                if (o.GetOwnProperty(i.ToString()) == null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}