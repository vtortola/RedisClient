using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnitTests.Common;
using vtortola.Redis;

namespace UnitTest.RedisClient.Parsing
{
    [TestClass]
    public class ParameterReaderTests
    {
        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientBindingException), MessagePattern="^The parameter key 'Whatever' does not exists in the given parameter data.$")]
        public void FailOnRequiringMemberOfEmptyParameters()
        {
            var reader = new ParameterReader<Object>();
            reader.GetValues("Whatever").ToArray();
        }

        private void AssertSameValues(String[] expected, String[] returned)
        {
            if (expected == null || returned == null)
            {
                Assert.Fail("There is a null");
            }
            if (expected.Length == 0 || returned.Length == 0)
            {
                Assert.Fail("No values");
            }
            if (expected.Length != returned.Length)
            {
                Assert.Fail("Different lengths");
            }

            Assert.AreEqual(0, expected.Except(returned).Count());
        }

        private void Test<T>(T parameter, String[] keys, String[] expected)
        {
            var reader = new ParameterReader<T>(parameter);
            var parsed = new List<String>();
            foreach (var key in keys)
            {
                parsed.AddRange(reader.GetValues(key));
            }
            AssertSameValues(expected, parsed.ToArray());
        }

        private String[] Array(params String[] array)
        {
            return array;
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientBindingException), MessagePattern="is not supported as parameter member")]
        public void CannotParseNonCollatedObject()
        {
            Test(
                parameter: new { Prop1 = "1", Prop2 = new { Prop3 = 3L, Prop4 = 4M, Prop5 = 5F } },
                keys: Array("Prop1", "Prop2", "Prop3", "Prop4", "Prop5"),
                expected: null
                );
        }

        [TestMethod]
        public void CanParseObject()
        {
            Test( 
                parameter:  new { Prop1 = "1", Prop2 = 2, Prop3 = 3L, Prop4 = 4M, Prop5 = 5F },
                keys:       Array ("Prop1", "Prop2", "Prop3", "Prop4", "Prop5"),
                expected:   Array ("1", "2", "3", "4", "5")
                );
        }

        [TestMethod]
        public void CanParseComplexObject()
        {
            Test(
                parameter: new { Prop1 = "1", Prop2 = Parameter.SequenceProperties(new { K2 = 3, K4 = 5 }), Prop3 = Parameter.SequenceProperties(new { K6 = 7, K8 = 9 }) },
                keys: Array("Prop1", "Prop2", "Prop3"),
                expected: Array("1", "K2", "3", "K4", "5", "K6", "7", "K8", "9")
                );
        }

        [TestMethod]
        public void CanParseObjectWithDateTime()
        {
            var d1 = new DateTime(1985, 5, 5, 17, 55, 00);
            var d2 = new DateTime(2000, 12, 15, 13, 10, 00);
            Test(
                parameter: new { Prop1 = d1 , Prop2 = d2},
                keys: Array("Prop1", "Prop2"),
                expected: Array(d1.ToBinary().ToString(), d2.ToBinary().ToString())
                );
        }

        public enum TestEnum { Test1 = 40, Test2 = 100}

        [TestMethod]
        public void CanParseObjectWithEnum()
        {
            Test(
                parameter: new { Prop1 = TestEnum.Test1, Prop2 = TestEnum.Test2 },
                keys: Array("Prop1", "Prop2"),
                expected: Array("40", "100")
                );
        }

        [TestMethod]
        public void CanParseInt32Arrays()
        {
            Test(
                parameter:  new { Prop1 = "1", Prop2 = new []{2,3,4,5}, Prop3 = new[]{6,7,8} },
                keys:       Array ("Prop1", "Prop2", "Prop3" ),
                expected:   Array ( "1", "2", "3", "4", "5", "6", "7", "8" )
                );
        }

        [TestMethod]
        public void CanParseDecimalArrays()
        {
            var culture = CultureInfo.CurrentCulture;
            try
            {
                // Spanish uses a colon to sepparate decimals.
                // Redis would not understan such notation
                // so this test changes the default to '
                CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("es-ES");
                var example = 100.34M;
                Assert.AreEqual("100,34", example.ToString());
                Test(
                    parameter: new { Prop1 = "1", Prop2 = new[] { 2.1M, 3.5M, 4M, 5.6M }, Prop3 = new[] { 6.1M, 7.0001M, 8.00001M } },
                    keys: Array("Prop1", "Prop2", "Prop3"),
                    expected: Array("1", "2.1", "3.5", "4", "5.6", "6.1", "7.0001", "8.00001")
                    );
            }
            finally
            {
                CultureInfo.DefaultThreadCurrentCulture = culture;
            }
        }

        [TestMethod]
        public void CanParsADictionaries()
        {
            Test(
                parameter: new { Prop1 = "1", Prop2 = Parameter.Sequence(new Dictionary<Int32, String> { { 2, "3" }, { 4, "5" } }), Prop3 = Parameter.Sequence(new Dictionary<Int32, String> { { 6, "7" }, { 8, "9" } }) },
                keys: Array("Prop1", "Prop2", "Prop3"),
                expected: Array("1", "2", "3", "4", "5", "6", "7", "8", "9")
                );
        }

        [TestMethod]
        public void CanParseKeyValuePairs()
        {
            Test(
                parameter: new { Prop1 = "1", Prop2 = Parameter.Sequence( new[] { new KeyValuePair<Int32, String>(2, "3"), new KeyValuePair<Int32, String>(4, "5") }), Prop3 = Parameter.Sequence( new[] { new KeyValuePair<Int32, String>(6, "7"), new KeyValuePair<Int32, String>(8, "9") }) },
                keys: Array("Prop1", "Prop2", "Prop3"),
                expected: Array("1", "2", "3", "4", "5", "6", "7", "8", "9")
                );
        }

        [TestMethod]
        public void CanParseTuple()
        {
            Test(
                parameter: new { Prop1 = "1", Prop2 = Parameter.Sequence(new[] { new Tuple<Int32, String>(2, "3"), new Tuple<Int32, String>(4, "5") }), Prop3 = Parameter.Sequence(new[] { new Tuple<Int32, String>(6, "7"), new Tuple<Int32, String>(8, "9") }) },
                keys: Array("Prop1", "Prop2", "Prop3"),
                expected: Array("1", "2", "3", "4", "5", "6", "7", "8", "9")
                );
        }
    }
}
