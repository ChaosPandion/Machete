using Machete.Interfaces;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using RegExpMatcher = Machete.Compiler.RegExpParser.RegExpMatcher;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NRegExp : LObject
    {
        public RegExpMatcher RegExpMatcher { get; set; }
        public string Body { get; set; }
        public string Flags { get; set; }


        public NRegExp(IEnvironment environment)
            : base(environment)
        {

        }
    }
}