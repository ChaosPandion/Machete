using Machete.Core;
using Xunit;

namespace Machete.Tests.Lexical_Conventions
{
    public sealed class Comments : TestBase
    {
        [Fact(DisplayName = "07.04 Comments: SingleLineComment")]
        public void Test0704A()
        {
            var r = Engine.ExecuteScriptToDynamic("//A single line comment\n2");
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(2.0, ((INumber)r).BaseValue);
        }

        [Fact(DisplayName = "07.04 Comments: MultiLineComment")]
        public void Test0704B()
        {
            var r = Engine.ExecuteScriptToDynamic("/*A multi \n line comment*/\n2");
            Assert.IsAssignableFrom<INumber>(r);
            Assert.Equal(2.0, ((INumber)r).BaseValue);
        }
    }
}