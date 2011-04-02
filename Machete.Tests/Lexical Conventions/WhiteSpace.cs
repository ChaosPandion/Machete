using System.Globalization;
using Machete.Core;
using Xunit;

namespace Machete.Tests.Lexical_Conventions
{
    public sealed class WhiteSpace : TestBase
    {
        //[Fact(DisplayName = "07.02 White Space: TAB")]
        //public void Test0702A()
        //{
        //    var r = Engine.ExecuteScriptToDynamic("\u00092");
        //    Assert.IsAssignableFrom<INumber>(r);
        //    Assert.Equal(2.0, ((INumber)r).BaseValue);
        //}

        //[Fact(DisplayName = "07.02 White Space: VT")]
        //public void Test0702B()
        //{
        //    var r = Engine.ExecuteScriptToDynamic("\u000B2");
        //    Assert.IsAssignableFrom<INumber>(r);
        //    Assert.Equal(2.0, ((INumber)r).BaseValue);
        //}

        //[Fact(DisplayName = "07.02 White Space: FF")]
        //public void Test0702C()
        //{
        //    var r = Engine.ExecuteScriptToDynamic("\u000C2");
        //    Assert.IsAssignableFrom<INumber>(r);
        //    Assert.Equal(2.0, ((INumber)r).BaseValue);
        //}

        //[Fact(DisplayName = "07.02 White Space: SP")]
        //public void Test0702D()
        //{
        //    var r = Engine.ExecuteScriptToDynamic("\u00202");
        //    Assert.IsAssignableFrom<INumber>(r);
        //    Assert.Equal(2.0, ((INumber)r).BaseValue);
        //}

        //[Fact(DisplayName = "07.02 White Space: NBSP")]
        //public void Test0702E()
        //{
        //    var r = Engine.ExecuteScriptToDynamic("\u00A02");
        //    Assert.IsAssignableFrom<INumber>(r);
        //    Assert.Equal(2.0, ((INumber)r).BaseValue);
        //}

        //[Fact(DisplayName = "07.02 White Space: BOM")]
        //public void Test0702F()
        //{
        //    var r = Engine.ExecuteScriptToDynamic("\uFEFF2");
        //    Assert.IsAssignableFrom<INumber>(r);
        //    Assert.Equal(2.0, ((INumber)r).BaseValue);
        //}

        //[Fact(DisplayName = "07.02 White Space: USP")]
        //public void Test0702G()
        //{
        //    for (char c = char.MinValue; c < char.MaxValue; c++)
        //    {
        //        switch (char.GetUnicodeCategory(c))
        //        {
        //            case UnicodeCategory.SpaceSeparator:
        //                var r = Engine.ExecuteScriptToDynamic(c + "2");
        //                Assert.IsAssignableFrom<INumber>(r);
        //                Assert.Equal(2.0, ((INumber)r).BaseValue);
        //                break;
        //        }                
        //    }
        //}
    }
}