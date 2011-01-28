using System;
using Xunit;

namespace Machete.Tests
{
    public class JsonObject : TestBase
    {
        [Fact(DisplayName = "15.12.2  parse ( text [ , reviver ] ): JSONBooleanLiteral")]
        public void TestJSONBooleanLiteral()
        {
            ExpectTrue("JSON.parse('true')");
            ExpectFalse("JSON.parse('false')");
        }

        [Fact(DisplayName = "15.12.2  parse ( text [ , reviver ] ): JSONNullLiteral")]
        public void TestJSONNullLiteral()
        {
            ExpectTrue("JSON.parse('null') === null");
        }

        [Fact(DisplayName = "15.12.2  parse ( text [ , reviver ] ): JSONString")]
        public void TestJSONString()
        {
            ExpectTrue("JSON.parse('\"string\"') === 'string'");
        }

        [Fact(DisplayName = "15.12.2  parse ( text [ , reviver ] ): JSONNumber")]
        public void TestJSONNumber()
        {
            ExpectTrue("JSON.parse('-2.999e2') === -2.999e2");
        }

        [Fact(DisplayName = "15.12.2  parse ( text [ , reviver ] ): JSONArray")]
        public void TestJSONArray()
        {
            ExpectString("JSON.parse('[true, false, null, -2.01, \"string\"]')", "true,false,,-2.01,string");
        }

        [Fact(DisplayName = "15.12.2  parse ( text [ , reviver ] ): JSONObject")]
        public void TestJSONObject()
        {
            ExpectString(@"
                (function() {
                    var o = JSON.parse('{ ""BoolTrue"": true, ""BoolFalse"": false, ""Null"": null, ""Number"": 2, ""String"": ""string"" }');
                    if (!o.hasOwnProperty('BoolTrue')) return 'Missing BoolTrue';
                    if (!o.BoolTrue) return 'BoolTrue is false';
                    if (!o.hasOwnProperty('BoolFalse')) return 'Missing BoolFalse';
                    if (o.BoolFalse) return 'BoolFalse is true';
                    if (!o.hasOwnProperty('Null')) return 'Missing Null';
                    if (o.Null !== null) return 'Null is not null';
                    if (!o.hasOwnProperty('Number')) return 'Missing Number';
                    if (o.Number !== 2) return 'Number is not 2';
                    if (!o.hasOwnProperty('String')) return 'Missing String';
                    if (o.String !== 'string') return 'String is not ""string""';
                    return 'Success';
                })();
            ", "Success");
        }
    }
}