using Machete.Core;
using Xunit;

namespace Machete.Tests.Lexical_Conventions
{
    public sealed class LineTerminators : TestBase
    {
        [Fact(DisplayName = "07.03 Line Terminators: LF")]
        public void Test0703A()
        {
            var r = Engine.ExecuteScriptToDynamic("\u000A2");
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(2.0, ((INumber)r).BaseValue);
        }

        [Fact(DisplayName = "07.03 Line Terminators: CR")]
        public void Test0703B()
        {
            var r = Engine.ExecuteScriptToDynamic("\u000D2");
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(2.0, ((INumber)r).BaseValue);
        }

        [Fact(DisplayName = "07.03 Line Terminators: LS")]
        public void Test0703C()
        {
            var r = Engine.ExecuteScriptToDynamic("\u20282");
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(2.0, ((INumber)r).BaseValue);
        }

        [Fact(DisplayName = "07.03 Line Terminators: PS")]
        public void Test0703D()
        {
            var r = Engine.ExecuteScriptToDynamic("\u20292");
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(2.0, ((INumber)r).BaseValue);
        }
    }
}