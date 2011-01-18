using System;
using System.Linq;
using System.Text;
using Machete.Core;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Collections.Generic;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PString : LObject
    {
        public PString(IEnvironment environment)
            : base(environment)
        {

        }

        public override void Initialize()
        {
            Class = "String";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;
            DefineOwnProperty("constructor", Environment.CreateDataDescriptor(Environment.StringConstructor, true, false, true), false);
            base.Initialize();
        }

        [BuiltinFunction("toString"), DataDescriptor(true, false, true)]
        internal static IDynamic ToString(IEnvironment environment, IArgs args)
        {
            return ValueOf(environment, args);
        }

        [BuiltinFunction("valueOf"), DataDescriptor(true, false, true)]
        internal static IDynamic ValueOf(IEnvironment environment, IArgs args)
        {
            var v = environment.Context.ThisBinding;
            switch (v.TypeCode)
            {
                case LanguageTypeCode.String:
                    return v;
                case LanguageTypeCode.Object:
                    var o = (IObject)v;
                    if (o.Class == "String")
                    {
                        return ((IPrimitiveWrapper)o).PrimitiveValue;
                    }
                    break;
            }
            throw environment.CreateTypeError("");
        }

        [BuiltinFunction("charAt", "pos"), DataDescriptor(true, false, true)]
        internal static IDynamic CharAt(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var position = (int)args[0].ConvertToInteger().BaseValue;
            if (position < 0 || position >= s.Length)
            {
                return environment.CreateString(string.Empty);
            }
            return environment.CreateString(s.Substring(position, 1));
        }

        [BuiltinFunction("charCodeAt", "pos"), DataDescriptor(true, false, true)]
        internal static IDynamic CharCodeAt(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var position = (int)args[0].ConvertToInteger().BaseValue;
            if (position < 0 || position >= s.Length)
            {
                return environment.CreateNumber(double.NaN);
            }
            return environment.CreateNumber((double)s[position]);
        }

        [BuiltinFunction("concat", "string1"), DataDescriptor(true, false, true)]
        internal static IDynamic Concat(IEnvironment environment, IArgs args)
        {
            var sb = new StringBuilder();
            {
                var o = environment.Context.ThisBinding;
                environment.CheckObjectCoercible(o);
                sb.Append(o.ConvertToString().BaseValue);
                foreach (var arg in args)
                {
                    sb.Append(arg.ConvertToString().BaseValue);
                }
            }
            return environment.CreateString(sb.ToString());
        }

        [BuiltinFunction("indexOf", "searchString"), DataDescriptor(true, false, true)]
        internal static IDynamic IndexOf(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var searchString = args[0].ConvertToString().BaseValue;
            var position = args.Count > 1 ? (int)args[1].ConvertToInteger().BaseValue : 0;
            var start = Math.Min(Math.Max(position, 0), s.Length);
            return environment.CreateNumber((double)s.IndexOf(searchString, start));
        }

        [BuiltinFunction("lastIndexOf", "searchString"), DataDescriptor(true, false, true)]
        internal static IDynamic LastIndexOf(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var searchString = args[0].ConvertToString().BaseValue;
            var position = args.Count > 1 ? (int)args[1].ConvertToInteger().BaseValue : int.MaxValue;
            var start = Math.Min(Math.Max(position, 0), s.Length);
            return environment.CreateNumber((double)s.LastIndexOf(searchString, start));
        }

        [BuiltinFunction("localeCompare", "that"), DataDescriptor(true, false, true)]
        internal static IDynamic LocaleCompare(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var that = args[0].ConvertToString().BaseValue;
            return environment.CreateNumber((double)s.CompareTo(that));
        }

        [BuiltinFunction("match", "regexp"), DataDescriptor(true, false, true)]
        internal static IDynamic Match(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var dynS = o.ConvertToString();
            var s = dynS.BaseValue;
            var regexpArg = args[0];
            var regexpObj = regexpArg as NRegExp;
            if (regexpObj == null)
            {
                var constructor = (IConstructable)environment.RegExpConstructor;
                var pattern = regexpArg.ConvertToString();
                regexpObj = (NRegExp)constructor.Construct(environment, environment.CreateArgs(pattern));
            }
            var exec = ((ICallable)regexpObj.Get("exec"));
            var execArgs = environment.CreateArgs(dynS);
            if (!regexpObj.Get("global").ConvertToBoolean().BaseValue)
            {
                return exec.Call(environment, regexpObj, execArgs);
            }
            else
            {
                var constructor = (IConstructable)environment.ArrayConstructor;
                var array = constructor.Construct(environment, environment.EmptyArgs);
                var previousLastIndex = 0;
                var lastMatch = true;
                var n = 0;
                while (lastMatch)
                {
                    var result = exec.Call(environment, regexpObj, execArgs);
                    switch (result.TypeCode)
                    {
                        case LanguageTypeCode.Null:
                            lastMatch = false;
                            break;
                        default:
                            var thisIndex = (int)regexpObj.Get("lastIndex").ConvertToNumber().BaseValue;
                            if (thisIndex == previousLastIndex)
                            {
                                thisIndex += 1;
                                regexpObj.Put("lastIndex", environment.CreateNumber(thisIndex), false);
                                previousLastIndex = thisIndex;
                            }
                            else
                            {
                                previousLastIndex = thisIndex;
                            }
                            var matchStr = ((IObject)result).Get("0");
                            var desc = environment.CreateDataDescriptor(matchStr, true, true, true);
                            array.DefineOwnProperty(n.ToString(), desc, true);
                            n++;
                            break;
                    }
                }
                if (n == 0)
                {
                    return environment.Null;
                }
                return array;
            }
        }

        [BuiltinFunction("replace", "searchValue", "replaceValue"), DataDescriptor(true, false, true)]
        internal static IDynamic Replace(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
            //var o = environment.Context.ThisBinding;
            //environment.CheckObjectCoercible(o);
            //var dynS = o.ConvertToString();
            //var s = dynS.BaseValue;
            //var searchValueArg = args[0];
            //var replaceValueArg = args[1];

            //var regExpObj = searchValueArg as NRegExp;
            //if (regExpObj != null)
            //{
            //    regExpObj.Put("lastIndex", environment.CreateNumber(0), false);
            //    var func = replaceValueArg as ICallable;
            //    if (func == null)
            //    {
            //        var replaceValue = replaceValueArg.ConvertToString().BaseValue;
            //        var index = 0;
            //        var rStr = s;
            //        do
            //        {
            //            var result = regExpObj.RegExp.Exec(s);
            //            if (!result.Succeeded) break;
            //            rStr = RegExp.Replacer.Replace(rStr, result, replaceValue);
            //            index = result.Index;
            //        } while (index < s.Length);
            //        return environment.CreateString(rStr);
            //    }
            //    else
            //    {
            //        string start, middle, end;
            //        var funcArgs = new List<IDynamic>();
            //        var index = 0;
            //        var rStr = s;
            //        do
            //        {
            //            var result = regExpObj.RegExp.Exec(s);
            //            if (!result.Succeeded) break;
            //            funcArgs.Clear();
            //            foreach (var item in result)
            //            {
            //                funcArgs.Add(environment.CreateString(item));
            //            }
            //            funcArgs.Add(environment.CreateNumber(result.Index));
            //            funcArgs.Add(dynS);
            //            var callResult = func.Call(environment, o, environment.CreateArgs(funcArgs));
            //            start = rStr.Substring(0, Math.Max(result.Index, 0));
            //            middle = callResult.ConvertToString().BaseValue;
            //            end = rStr.Substring(result.Index + result[0].Length);
            //            rStr = start + middle + end;
            //            index = result.Index;
            //        } while (++index < s.Length);
            //        return environment.CreateString(rStr);
            //    }
            //}
            //var dynSearchValue = searchValueArg.ConvertToString();
            //var searchValue = dynSearchValue.BaseValue;
            //var matchIndex = s.IndexOf(searchValue);
            //if (matchIndex > -1)
            //{
            //    string start, middle, end;
            //    {
            //        start = s.Substring(0, Math.Max(matchIndex, 0));
            //        var func = replaceValueArg as ICallable;
            //        if (func == null)
            //        {
            //            middle = replaceValueArg.ConvertToString().BaseValue;
            //        }
            //        else
            //        {
            //            var dynMatchIndex = environment.CreateNumber(matchIndex);
            //            var funcArgs = new IDynamic[] { dynSearchValue, dynMatchIndex, dynS };
            //            var callResult = func.Call(environment, o, environment.CreateArgs(funcArgs));
            //            middle = callResult.ConvertToString().BaseValue;
            //        }
            //        end = s.Substring(matchIndex + searchValue.Length);
            //    }
            //    return environment.CreateString(start + middle + end);
            //}
            //return dynS;
        }

        [BuiltinFunction("search", "regexp"), DataDescriptor(true, false, true)]
        internal static IDynamic Search(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
            //var o = environment.Context.ThisBinding;
            //environment.CheckObjectCoercible(o);
            //var s = o.ConvertToString().BaseValue;
            //var regexpArg = args[0];
            //var regexpObj = regexpArg as NRegExp;
            //if (regexpObj == null)
            //{
            //    var constructor = (IConstructable)environment.RegExpConstructor;
            //    var pattern = regexpArg.ConvertToString();
            //    regexpObj = (NRegExp)constructor.Construct(environment, environment.CreateArgs(pattern));
            //}
            //var regExp = regexpObj.RegExp;
            //var index = 0;
            //do
            //{
            //    var result = regExp.Match(s, index);
            //    if (result.Succeeded)
            //    {
            //        return environment.CreateNumber(index);
            //    }
            //} while (++index < s.Length);
            //return environment.CreateNumber(-1);
        }

        [BuiltinFunction("slice", "start", "end"), DataDescriptor(true, false, true)]
        internal static IDynamic Slice(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var start = args[0];
            var end = args[1];
            var intStart = (int)start.ConvertToInteger().BaseValue;
            var intEnd = end is IUndefined ? s.Length : (int)end.ConvertToInteger().BaseValue;
            var from = intStart < 0 ? Math.Max(s.Length + intStart, 0) : Math.Min(intStart, s.Length);
            var to = intStart < 0 ? Math.Max(s.Length + intEnd, 0) : Math.Min(intEnd, s.Length);
            var span = Math.Max(to - from, 0);
            return environment.CreateString(s.Substring(from, span));
        }

        [BuiltinFunction("split", "separator", "limit"), DataDescriptor(true, false, true)]
        internal static IDynamic Split(IEnvironment environment, IArgs args)
        {
            throw new NotImplementedException();
            //var o = environment.Context.ThisBinding;
            //environment.CheckObjectCoercible(o);
            //var s = o.ConvertToString().BaseValue;
            //var separatorArg = args[0];
            //var limitArg = args[1];
            //var array = ((IConstructable)environment.ArrayConstructor).Construct(environment, environment.EmptyArgs);

            //int limit = 0, lower = 0, upper = 0, count = 0;
            //IPropertyDescriptor desc;
            //IString value;

            //// Using int32 instead of uint32 due to limitations of .NET arrays.
            //limit = limitArg is IUndefined ? int.MaxValue : (int)limitArg.ConvertToUInt32().BaseValue;            
            //if (limit == 0)
            //{
            //    return array;
            //}

            //var separatorObj = separatorArg as NRegExp;
            //if (separatorObj != null)
            //{
            //    do
            //    {
            //        var result = separatorObj.RegExp.Match(s, upper);
            //        if (!result.Succeeded)
            //        {
            //            upper++;
            //        }
            //        else
            //        {
            //            upper = result.Index;
            //            value = environment.CreateString(s.Substring(lower, upper - lower - 1));
            //            desc = environment.CreateDataDescriptor(value, true, true, true);
            //            array.DefineOwnProperty(count.ToString(), desc, true);
            //            if (++count == limit) return array;
            //            for (int i = 0; i < result.Length; i++)
            //            {
            //                value = environment.CreateString(result[i]);
            //                desc = environment.CreateDataDescriptor(value, true, true, true);
            //                array.DefineOwnProperty(count.ToString(), desc, true);
            //                if (++count == limit) return array;
            //            }
            //            lower = upper;
            //        }
            //    } while (upper < s.Length);
            //    value = environment.CreateString(s.Substring(lower));
            //    desc = environment.CreateDataDescriptor(value, true, true, true);
            //    array.DefineOwnProperty(count.ToString(), desc, true);
            //}
            //else
            //{
            //    var separatorStr = ((IString)separatorArg).BaseValue;
            //    var pieces = s.Split(new[] { separatorStr }, StringSplitOptions.None);
            //    for (int i = 0; i < pieces.Length; i++)
            //    {
            //        value = environment.CreateString(pieces[i]);
            //        desc = environment.CreateDataDescriptor(value, true, true, true);
            //        array.DefineOwnProperty(i.ToString(), desc, true);
            //        if (++count == limit) return array;
            //    }
            //}

            //return array;
        }

        [BuiltinFunction("substring", "start", "end"), DataDescriptor(true, false, true)]
        internal static IDynamic Substring(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var start = args[0];
            var end = args[1];
            var startIndex = args[0].ConvertToInteger().BaseValue;
            var endIndex = end is IUndefined ? (double)s.Length : end.ConvertToInteger().BaseValue;
            var finalStart = Math.Min(Math.Max(startIndex, 0), s.Length);
            var finalEnd = Math.Min(Math.Max(endIndex, 0), s.Length);
            var from = (int)Math.Min(finalStart, finalEnd);
            var to = (int)Math.Max(finalStart, finalEnd);
            return environment.CreateString(s.Substring(from, to - from));
        }

        [BuiltinFunction("toLowerCase"), DataDescriptor(true, false, true)]
        internal static IDynamic ToLowerCase(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            return environment.CreateString(s.ToLowerInvariant());
        }

        [BuiltinFunction("toLocaleLowerCase"), DataDescriptor(true, false, true)]
        internal static IDynamic ToLocaleLowerCase(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            return environment.CreateString(s.ToLower());
        }

        [BuiltinFunction("toUpperCase"), DataDescriptor(true, false, true)]
        internal static IDynamic ToUpperCase(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            return environment.CreateString(s.ToUpperInvariant());
        }

        [BuiltinFunction("toLocaleUpperCase"), DataDescriptor(true, false, true)]
        internal static IDynamic ToLocaleUpperCase(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            return environment.CreateString(s.ToUpper());
        }

        [BuiltinFunction("trim"), DataDescriptor(true, false, true)]
        internal static IDynamic Trim(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            s = s.Trim(Machete.Compiler.CharSets.trimCharacters);
            return environment.CreateString(s);
        }
    }
}