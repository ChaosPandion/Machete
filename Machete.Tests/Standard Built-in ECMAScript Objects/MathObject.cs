using System;
using Xunit;

namespace Machete.Tests
{
    public class MathObject : TestBase
    {
        [Fact(DisplayName = "15.8.2.15  round (x)")]
        public void Test158215()
        {
            ExpectDouble("Math.round(1.0)", 1.0);
            ExpectDouble("Math.round(1.1)", 1.0);
            ExpectDouble("Math.round(1.25)", 1.0);
            ExpectDouble("Math.round(1.5)", 2.0);
            ExpectDouble("Math.round(1.75)", 2.0);
            ExpectDouble("Math.round(-1.0)", -1.0);
            ExpectDouble("Math.round(-1.1)", -1.0);
            ExpectDouble("Math.round(-1.25)", -1.0);
            ExpectDouble("Math.round(-1.5)", -1.0);
            ExpectDouble("Math.round(-1.75)", -2.0);
        }
    }
}