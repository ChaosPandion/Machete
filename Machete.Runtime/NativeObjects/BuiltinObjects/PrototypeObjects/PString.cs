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

        
        static Func<string, RegExpParser.MatchState, string> MakeReplacer(string format)
        {
            var tokens =
                new Regex(@"[^\$]*(\$[\$&`']|\$\d{1,2})")
                .Matches(format)
                .Cast<Match>()
                .Select(m => (Capture)m.Groups[1])
                .ToList();

            if (tokens.Count == 0)
            {
                return (value, state) => format;
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
            var appendDollarSign = Expression.Call(sbVariable, appendMethod, Expression.Constant("$"));
            var getMatchedSubstring = Expression.ArrayIndex(capturesProp, zero);
            var appendMatchedSubstring = Expression.Call(sbVariable, appendMethod, getMatchedSubstring);
            var getBeforeSubstring = Expression.Call(inputProp, substringWithLengthMethod, zero, Expression.Subtract(endIndexProp, one));
            var appendBeforeSubstring = Expression.Call(sbVariable, appendMethod, getBeforeSubstring);
            var getAfterSubstring = Expression.Call(inputProp, substringMethod, endIndexProp);
            var appendAfterSubstring = Expression.Call(sbVariable, appendMethod, getAfterSubstring);
            var e = (Expression)Expression.New(typeof(StringBuilder));
            var index = 0;

            foreach (var token in tokens)
            {
                if (token.Index > 0)
                {
                    e = Expression.Call(e, appendMethod, Expression.Constant(format.Substring(index, token.Index - index)));                   
                }
                index = token.Index + token.Length;
                if (token.Value == "$$")
                {
                    e = Expression.Call(e, appendMethod, dollarSign);
                }
                else if (token.Value == "$&")
                {
                    e = Expression.Call(e, appendMethod, valueVariable);
                }
                else if (token.Value == "$`")
                {
                    e = Expression.Call(e, appendMethod, getBeforeSubstring);
                }
                else if (token.Value == "$'")
                {
                    e = Expression.Call(e, appendMethod, getAfterSubstring);
                }
                else
                {
                    var n = token.Value.Substring(1);
                    if (n.Length == 2 && n[0] == '0')
                    {
                        n = n.Substring(1);
                    }
                    var i = int.Parse(n) - 1;
                    var nConst = Expression.Constant(i);
                    var t = Expression.Constant(token.Value);
                    if (i < 0)
                    {
                        e = Expression.Call(e, appendMethod, t);  
                    }
                    else
                    {
                        var cond =
                            Expression.Condition(
                                Expression.GreaterThan(
                                    capturesLengthProp,
                                    nConst
                                ),
                                Expression.ArrayIndex(capturesProp, nConst),
                                t                                
                            );
                        e = Expression.Call(e, appendMethod, cond);  
                    }
                }
            }

            e = Expression.Call(e, toStringMethod);
            var lambda = Expression.Lambda<Func<string, RegExpParser.MatchState, string>>(e, valueVariable, stateVariable);
            return lambda.Compile();
        }


        IDynamic Replace(IEnvironment environment, IArgs args)
        {
            var o = environment.Context.ThisBinding;
            var so = o.ConvertToString();
            var s = so.BaseValue;
            var matches = new List<Tuple<int, int, Func<string, string>>>();
            var searchValueArg = args[0];
            var replaceValueArg = args[1];
            var replaceValueFunc = replaceValueArg as ICallable;
            var replaceValueString = replaceValueArg.ConvertToString();

            if (searchValueArg is NRegExp)
            {
                var regExpObj = (NRegExp)searchValueArg;
                var matcher = regExpObj.RegExpMatcher;

                Func<int, RegExpParser.MatchState, Func<string, string>> makeReplacer = null;

                if (replaceValueFunc != null)
                {
                    makeReplacer =
                        (startIndex, state) =>
                            _ =>
                            {
                                var nullArg = (IDynamic)environment.Null;
                                var cArgs = new List<IDynamic>();
                                foreach (var c in state.captures)
                                {
                                    if (c == null)
                                    {
                                        cArgs.Add(environment.Null);
                                        continue;
                                    }
                                    cArgs.Add(c == null ? nullArg : environment.CreateString(c));
                                }
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
                    makeReplacer =
                        (_, state) =>
                            v => replacer(v, state);
                }

                if (!regExpObj.Flags.Contains("g"))
                {
                    var index = 0;
                    do
                    {
                        var r = matcher(s, index);
                        if (!r.success)
                        {
                            index++;
                            continue;
                        }
                        matches.Add(
                            Tuple.Create(
                                index,
                                r.matchState.endIndex - index,
                                makeReplacer(index, r.matchState)
                            )
                        );
                        break;
                    } while (index < s.Length);
                }
                else
                {
                    var index = (int)regExpObj.Get("lastIndex").ConvertToNumber().BaseValue;
                    do
                    {
                        var r = matcher(s, index);
                        if (!r.success)
                        {
                            index++;
                            continue;
                        }
                        matches.Add(
                            Tuple.Create(
                                index,
                                r.matchState.endIndex - index,
                                makeReplacer(index, r.matchState)                                
                            )
                        );
                        index = r.matchState.endIndex;
                        regExpObj.Put("lastIndex", environment.CreateNumber(index), false);
                    } while (index < s.Length);
                }
            }
            else
            {
                Func<string, string> replace = null;

                if (replaceValueFunc == null)
                {
                    replace =  _ => replaceValueString.BaseValue;
                }
                else
                {
                    replace = 
                        v =>
                        {
                            var arg = environment.CreateString(v);
                            var result =
                                replaceValueFunc.Call(
                                    environment,
                                    environment.Undefined,
                                    environment.CreateArgs(new[] { arg })
                                );
                            return result.ConvertToString().BaseValue;
                        };
                }

                var searchValue = args[0].ConvertToString().BaseValue;
                var index = 0;
                
                do
                {
                    var resultIndex = s.IndexOf(searchValue, index);
                    if (resultIndex < 0)
                    {
                        index++;
                        continue;
                    }
                    matches.Add(
                        Tuple.Create<int, int, Func<string, string>>(
                            resultIndex, 
                            searchValue.Length,
                            replace
                        )
                    );
                    index = resultIndex + 1;
                } while (index < s.Length);
            }

            if (matches.Count == 0)
                return so;
            {
            var sb = new StringBuilder();
            var startIndex = 0;
            foreach (var match in matches)
            {
                if (match.Item1 > 0) 
                    sb.Append(s.Substring(startIndex, match.Item1 - startIndex));
                sb.Append(match.Item3(s.Substring(match.Item1, match.Item2)));
                startIndex = match.Item1 + match.Item2;
            }
            if (startIndex < s.Length)
                sb.Append(s.Substring(startIndex));
            return environment.CreateString(sb.ToString());
                }
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