using System;
using System.Text;
using Machete.Core;
using Machete.Runtime.HostObjects.Iterables;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using System.Collections.Generic;
using Machete.Compiler;
using System.Text.RegularExpressions;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace Machete.Runtime.NativeObjects.BuiltinObjects.PrototypeObjects
{
    public sealed class PString : LObject
    {
        public BFunction ToStringFunction { get; private set; }
        public BFunction ValueOfFunction { get; private set; }
        public BFunction CharAtFunction { get; private set; }
        public BFunction CharCodeAtFunction { get; private set; }
        public BFunction ConcatFunction { get; private set; }
        public BFunction IndexOfFunction { get; private set; }
        public BFunction LastIndexOfFunction { get; private set; }
        public BFunction LocaleCompareFunction { get; private set; }
        public BFunction MatchFunction { get; private set; }
        public BFunction ReplaceFunction { get; private set; }
        public BFunction SearchFunction { get; private set; }
        public BFunction SliceFunction { get; private set; }
        public BFunction SplitFunction { get; private set; }
        public BFunction SubstringFunction { get; private set; }
        public BFunction ToLowerCaseFunction { get; private set; }
        public BFunction ToLocaleLowerCaseFunction { get; private set; }
        public BFunction ToUpperCaseFunction { get; private set; }
        public BFunction ToLocaleUpperCaseFunction { get; private set; }
        public BFunction TrimFunction { get; private set; }
        public BFunction CreateIteratorFunction { get; private set; }


        public PString(IEnvironment environment)
            : base(environment)
        {

        }


        public override void Initialize()
        {
            Class = "String";
            Extensible = true;
            Prototype = Environment.ObjectPrototype;

            ToStringFunction = new BFunction(Environment, ToString, ReadOnlyList<string>.Empty);
            ValueOfFunction = new BFunction(Environment, ValueOf, ReadOnlyList<string>.Empty);
            CharAtFunction = new BFunction(Environment, CharAt, new ReadOnlyList<string>("pos"));
            CharCodeAtFunction = new BFunction(Environment, CharCodeAt, new ReadOnlyList<string>("pos"));
            ConcatFunction = new BFunction(Environment, Concat, new ReadOnlyList<string>("string1"));
            IndexOfFunction = new BFunction(Environment, IndexOf, new ReadOnlyList<string>("searchString"));
            LastIndexOfFunction = new BFunction(Environment, LastIndexOf, new ReadOnlyList<string>("searchString"));
            LocaleCompareFunction = new BFunction(Environment, LocaleCompare, new ReadOnlyList<string>("that"));
            MatchFunction = new BFunction(Environment, Match, new ReadOnlyList<string>("regexp"));
            ReplaceFunction = new BFunction(Environment, Replace, new ReadOnlyList<string>("searchValue", "replaceValue"));
            SearchFunction = new BFunction(Environment, Search, new ReadOnlyList<string>("regexp"));
            SliceFunction = new BFunction(Environment, Slice, new ReadOnlyList<string>("start", "end"));
            SplitFunction = new BFunction(Environment, Split, new ReadOnlyList<string>("separator", "limit"));
            SubstringFunction = new BFunction(Environment, Substring, new ReadOnlyList<string>("start", "end"));
            ToLowerCaseFunction = new BFunction(Environment, ToLowerCase, ReadOnlyList<string>.Empty);
            ToLocaleLowerCaseFunction = new BFunction(Environment, ToLocaleLowerCase, ReadOnlyList<string>.Empty);
            ToUpperCaseFunction = new BFunction(Environment, ToUpperCase, ReadOnlyList<string>.Empty);
            ToLocaleUpperCaseFunction = new BFunction(Environment, ToLocaleUpperCase, ReadOnlyList<string>.Empty);
            TrimFunction = new BFunction(Environment, Trim, ReadOnlyList<string>.Empty);
            CreateIteratorFunction = new BFunction(Environment, CreateIterator, ReadOnlyList<string>.Empty);

            new LObject.Builder(this)
            .SetAttributes(true, false, true)
            .AppendDataProperty("constructor", Environment.StringConstructor)
            .AppendDataProperty("toString", ToStringFunction)
            .AppendDataProperty("valueOf", ValueOfFunction)
            .AppendDataProperty("charAt", CharAtFunction)
            .AppendDataProperty("charCodeAt", CharCodeAtFunction)
            .AppendDataProperty("concat", ConcatFunction)
            .AppendDataProperty("indexOf", IndexOfFunction)
            .AppendDataProperty("lastIndexOf", LastIndexOfFunction)
            .AppendDataProperty("localeCompare", LocaleCompareFunction)
            .AppendDataProperty("match", MatchFunction)
            .AppendDataProperty("replace", ReplaceFunction)
            .AppendDataProperty("search", SearchFunction)
            .AppendDataProperty("slice", SliceFunction)
            .AppendDataProperty("split", SplitFunction)
            .AppendDataProperty("substring", SubstringFunction)
            .AppendDataProperty("toLowerCase", ToLowerCaseFunction)
            .AppendDataProperty("toLocaleLowerCase", ToLocaleLowerCaseFunction)
            .AppendDataProperty("toUpperCase", ToUpperCaseFunction)
            .AppendDataProperty("toLocaleUpperCase", ToLocaleUpperCaseFunction)
            .AppendDataProperty("trim", TrimFunction)
            .SetAttributes(false, false, false)
            .AppendDataProperty("createIterator", CreateIteratorFunction);
        }

        IDynamic ToString(IEnvironment environment, IArgs args)
        {
            return ValueOf(environment, args);
        }

        IDynamic ValueOf(IEnvironment environment, IArgs args)
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

        IDynamic CharAt(IEnvironment environment, IArgs args)
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

        IDynamic CharCodeAt(IEnvironment environment, IArgs args)
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

        IDynamic Concat(IEnvironment environment, IArgs args)
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

        IDynamic IndexOf(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var searchString = args[0].ConvertToString().BaseValue;
            var position = args.Count > 1 ? (int)args[1].ConvertToInteger().BaseValue : 0;
            var start = Math.Min(Math.Max(position, 0), s.Length);
            return environment.CreateNumber((double)s.IndexOf(searchString, start));
        }

        IDynamic LastIndexOf(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var searchString = args[0].ConvertToString().BaseValue;
            var position = args.Count > 1 ? (int)args[1].ConvertToInteger().BaseValue : int.MaxValue;
            var start = Math.Min(Math.Max(position, 0), s.Length);
            return environment.CreateNumber((double)s.LastIndexOf(searchString, start));
        }

        IDynamic LocaleCompare(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var that = args[0].ConvertToString().BaseValue;
            return environment.CreateNumber((double)s.CompareTo(that));
        }

        IDynamic Match(IEnvironment environment, IArgs args)
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
                regexpObj = (NRegExp)constructor.Construct(environment, environment.CreateArgs(new [] { pattern }));
            }
            var exec = ((ICallable)regexpObj.Get("exec"));
            var execArgs = environment.CreateArgs(new [] { dynS });
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


        delegate string Replacer(string matchedSubstring, RegExpParser.MatchState state);

        static readonly ConcurrentDictionary<string, Replacer> _replacerMap = new ConcurrentDictionary<string, Replacer>();

        static Replacer MakeReplacer(string format)
        {
            Replacer replacer;

            if (_replacerMap.TryGetValue(format, out replacer))
                return replacer;

            var items =
                Regex.Matches(format, @"([^\$]*)(\$[\$&`']|\$\d{1,2})")
                .Cast<Match>()
                .Select(m => 
                    new {
                        TextBefore = m.Groups[1].Value,
                        Token = m.Groups[2].Value
                    }
                )
                .ToArray();

            if (items.Length == 0)
            {
                replacer = (value, state) => format;
                _replacerMap.TryAdd(format, replacer);
                return replacer;
            }

            var appendMethod = typeof(StringBuilder).GetMethod("Append", new[] { typeof(string) });
            var toStringMethod = typeof(StringBuilder).GetMethod("ToString", new Type[0]);
            var substringMethod = typeof(string).GetMethod("Substring", new[] { typeof(int) });
            var substringWithLengthMethod = typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) });
            var zero = Expression.Constant(0);
            var one = Expression.Constant(1);
            var dollarSign = Expression.Constant("$");
            var valueVariable = Expression.Parameter(typeof(string), "value");
            var stateVariable = Expression.Parameter(typeof(RegExpParser.MatchState), "state");
            var sbVariable = Expression.Variable(typeof(StringBuilder), "sb");
            var capturesProp = Expression.Property(stateVariable, "captures");
            var capturesLengthProp = Expression.Property(capturesProp, "Length");
            var endIndexProp = Expression.Property(stateVariable, "endIndex");
            var inputProp = Expression.Property(stateVariable, "input");
            var getMatchedSubstring = Expression.ArrayIndex(capturesProp, zero);
            var getBeforeSubstring = Expression.Call(inputProp, substringWithLengthMethod, zero, Expression.Subtract(endIndexProp, one));
            var getAfterSubstring = Expression.Call(inputProp, substringMethod, endIndexProp);
            
            var e = (Expression)Expression.New(typeof(StringBuilder));
            foreach (var item in items)
            {
                e = Expression.Call(e, appendMethod, Expression.Constant(item.TextBefore));
                if (item.Token == "$$")
                {
                    e = Expression.Call(e, appendMethod, dollarSign);
                }
                else if (item.Token == "$&")
                {
                    e = Expression.Call(e, appendMethod, valueVariable);
                }
                else if (item.Token == "$`")
                {
                    e = Expression.Call(e, appendMethod, getBeforeSubstring);
                }
                else if (item.Token == "$'")
                {
                    e = Expression.Call(e, appendMethod, getAfterSubstring);
                }
                else
                {
                    var n = int.Parse(item.Token.Substring(1)) - 1;
                    var t = Expression.Constant(item.Token);
                    if (n < 0)
                    {
                        e = Expression.Call(e, appendMethod, t);  
                    }
                    else
                    {
                        var nc = Expression.Constant(n);
                        var gt = Expression.GreaterThan(capturesLengthProp, nc);
                        var ai = Expression.ArrayIndex(capturesProp, nc);
                        var cnd = Expression.Condition(gt, ai, t);
                        e = Expression.Call(e, appendMethod, cnd);  
                    }
                }
            }

            var lambda = Expression.Lambda<Replacer>(Expression.Call(e, toStringMethod), valueVariable, stateVariable);
            replacer = lambda.Compile();
            _replacerMap.TryAdd(format, replacer);
            return replacer;
        }
        
        IDynamic Replace(IEnvironment environment, IArgs args)
        {
            var so = environment.Context.ThisBinding.ConvertToString();
            var s = so.BaseValue;
            var sb = new StringBuilder();
            var searchValueArg = args[0];
            var replaceValueArg = args[1];
            var replaceValueFunc = replaceValueArg as ICallable;
            var replaceValueString = replaceValueArg.ConvertToString();
            var matchSubstring = "";
            var i = 0;

            if (searchValueArg is NRegExp)
            {
                var regExpObj = (NRegExp)searchValueArg;
                var matcher = regExpObj.RegExpMatcher;

                Func<int, RegExpParser.MatchState, Func<string, string>> makeReplacer = null;

                if (replaceValueFunc != null)
                {
                    makeReplacer =
                        (startIndex, state) =>
                            v =>
                            {
                                var cArgs = new List<IDynamic>();
                                cArgs.Add(environment.CreateString(v));
                                foreach (var c in state.captures) 
                                    cArgs.Add(c == null ? (IDynamic)environment.Null : environment.CreateString(c));
                                cArgs.Add(environment.CreateNumber(startIndex));
                                cArgs.Add(so);
                                var result = 
                                    replaceValueFunc.Call(
                                        environment, 
                                        environment.Undefined, 
                                        environment.CreateArgs(cArgs)
                                    );
                                return result.ConvertToString().BaseValue;
                            };
                }
                else
                {
                    var replacer = MakeReplacer(replaceValueString.BaseValue);
                    makeReplacer = (_, state) => v => replacer(v, state);
                }

                var global = regExpObj.Flags.Contains("g");
                i = global ? (int)regExpObj.Get("lastIndex").ConvertToNumber().BaseValue : 0;

                do
                {
                    var r = matcher(s, i);
                    if (!r.success)
                    {
                        sb.Append(s[i++]);
                        continue;
                    }
                    matchSubstring = s.Substring(i, r.matchState.endIndex - i);
                    sb.Append(makeReplacer(i, r.matchState)(matchSubstring));
                    if (!global)
                        break;
                    i = r.matchState.endIndex;
                    regExpObj.Put("lastIndex", environment.CreateNumber(i), false);
                } while (i < s.Length);
            }
            else
            {
                Func<string, string> replace = null;

                if (replaceValueFunc == null)
                {
                    replace =  v => replaceValueString.BaseValue;
                }
                else
                {
                    replace = 
                        v => 
                            replaceValueFunc.Call(
                                environment, 
                                environment.Undefined, 
                                environment.CreateArgs(
                                    new[] { environment.CreateString(v) }
                                )
                            )
                            .ConvertToString()
                            .BaseValue;
                }

                var searchValue = args[0].ConvertToString().BaseValue;
                var index = 0;
                
                do
                {
                    var resultIndex = s.IndexOf(searchValue, index);
                    if (resultIndex < 0)
                    {
                        sb.Append(s.Substring(index));
                        break;
                    }
                    matchSubstring = s.Substring(resultIndex, searchValue.Length);
                    sb.Append(replace(matchSubstring));
                    index = resultIndex + searchValue.Length;
                } while (index < s.Length);
            }

            return environment.CreateString(sb.ToString());                
        }


        IDynamic Search(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var regexpArg = args[0];
            var regexpObj = regexpArg as NRegExp;
            if (regexpObj == null)
            {
                var constructor = (IConstructable)environment.RegExpConstructor;
                var pattern = regexpArg.ConvertToString();
                regexpObj = (NRegExp)constructor.Construct(environment, environment.CreateArgs(new [] { pattern }));
            }
            var regExp = regexpObj.RegExpMatcher;
            var index = 0;
            do
            {
                var result = regExp(s, index);
                if (result.success)
                {
                    return environment.CreateNumber(index);
                }
            } while (++index < s.Length);
            return environment.CreateNumber(-1);
        }

        IDynamic Slice(IEnvironment environment, IArgs args)
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

        IDynamic Split(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            var separatorArg = args[0];
            var limitArg = args[1];
            var array = ((IConstructable)environment.ArrayConstructor).Construct(environment, environment.EmptyArgs);

            int limit = 0, lower = 0, upper = 0, count = 0;
            IPropertyDescriptor desc;
            IString value;

            // Using int32 instead of uint32 due to limitations of .NET arrays.
            limit = limitArg is IUndefined ? int.MaxValue : (int)limitArg.ConvertToUInt32().BaseValue;
            if (limit == 0)
            {
                return array;
            }

            var separatorObj = separatorArg as NRegExp;
            if (separatorObj != null)
            {
                do
                {
                    var result = separatorObj.RegExpMatcher(s, upper);
                    if (!result.success)
                    {
                        upper++;
                    }
                    else
                    {
                        upper = result.matchState.endIndex;
                        value = environment.CreateString(s.Substring(lower, upper - lower - 1));
                        desc = environment.CreateDataDescriptor(value, true, true, true);
                        array.DefineOwnProperty(count.ToString(), desc, true);
                        if (++count == limit) return array;
                        var captures = result.matchState.captures;
                        for (int i = 0; i < captures.Length; i++)
                        {
                            value = environment.CreateString(captures[i]);
                            desc = environment.CreateDataDescriptor(value, true, true, true);
                            array.DefineOwnProperty(count.ToString(), desc, true);
                            if (++count == limit) return array;
                        }
                        lower = upper;
                    }
                } while (upper < s.Length);
                value = environment.CreateString(s.Substring(lower));
                desc = environment.CreateDataDescriptor(value, true, true, true);
                array.DefineOwnProperty(count.ToString(), desc, true);
            }
            else
            {
                var separatorStr = ((IString)separatorArg).BaseValue;
                var pieces = s.Split(new[] { separatorStr }, StringSplitOptions.None);
                for (int i = 0; i < pieces.Length; i++)
                {
                    value = environment.CreateString(pieces[i]);
                    desc = environment.CreateDataDescriptor(value, true, true, true);
                    array.DefineOwnProperty(i.ToString(), desc, true);
                    if (++count == limit) return array;
                }
            }

            return array;
        }

        IDynamic Substring(IEnvironment environment, IArgs args)
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

        IDynamic ToLowerCase(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            return environment.CreateString(s.ToLowerInvariant());
        }

        IDynamic ToLocaleLowerCase(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            return environment.CreateString(s.ToLower());
        }

        IDynamic ToUpperCase(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            return environment.CreateString(s.ToUpperInvariant());
        }

        IDynamic ToLocaleUpperCase(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            return environment.CreateString(s.ToUpper());
        }

        IDynamic Trim(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            environment.CheckObjectCoercible(o);
            var s = o.ConvertToString().BaseValue;
            s = s.Trim(Machete.Compiler.CharSets.trimCharacters);
            return environment.CreateString(s);
        }

        IDynamic CreateIterator(IEnvironment environment, IArgs args)
        {
            var s = environment.Context.ThisBinding.ConvertToString();
            return new HStringIterator(environment, s);
        }
    }
}