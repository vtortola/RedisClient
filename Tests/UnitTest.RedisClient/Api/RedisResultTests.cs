using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using System.Globalization;
using UnitTests.Common;
using System.Linq;

namespace UnitTest.RedisClient
{
    [TestClass]
    public class RedisResultTests
    {
        RESPArray BuildArray(params String[] literals)
        {
            return new RESPArray(literals.Select(l=> new RESPBulkString(l)).ToArray());
        }

        [TestMethod]
        public void CanParseSimpleResult()
        {
            var array = BuildArray("a","b","c","d","e","f");

            var result = new RedisResults(new [] { array });

            var rarray = result[0].GetStringArray();
            Assert.IsNotNull(rarray);
            Assert.AreEqual(6, rarray.Length);
        }

        [TestMethod]
        public void CanConsolidateTransaction()
        {
            var result = new RESPObject[] 
            { 
                new RESPSimpleString("OK"), 
                new RESPSimpleString("QUEUED"), 
                new RESPSimpleString("QUEUED"),
                new RESPArray(new RESPSimpleString("OK"), new RESPSimpleString("OK"))
            };

            var headers = new [] { "MULTI", "INCR", "INCR", "EXEC" };

            Transaction.Consolidate(result, headers);

            Assert.IsNotNull(result);
            Assert.AreEqual("OK", result[0].GetString(), true);
            Assert.AreEqual("OK", result[1].GetString(), true);
            Assert.AreEqual("OK", result[2].GetString(), true);
            Assert.AreEqual("OK", result[3].GetString(), true);
        }
       
        [TestMethod]
        public void CanCancelTransaction()
        {
            var result = new RESPObject[] 
            { 
                new RESPSimpleString("OK"), 
                new RESPSimpleString("QUEUED"), 
                new RESPSimpleString("QUEUED"),
                new RESPBulkString(null)
            };
            var headers = new[] { "MULTI", "INCR", "INCR", "EXEC" };

            Transaction.Consolidate(result, headers);

            Assert.IsNotNull(result);
            Assert.AreEqual("OK", result[0].GetString());
            Assert.AreEqual("QUEUED", result[1].GetString());
            Assert.AreEqual("QUEUED", result[2].GetString());
            Assert.AreEqual("EXECWATCHFAILED", result[3].Cast<RESPError>().Prefix);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientCastException), MessagePattern="^Type 'RESPBulkString' cannot be cast to 'Int64'$")]
        public void CanParseSimpleIntegerResult()
        {
            var array = BuildArray("1", "2", "3", "4", "5", "6");

            var result = new RedisResults(new[] { array });

            var rarray = result[0].GetIntegerArray();
            Assert.IsNotNull(rarray);
            Assert.AreEqual(6, rarray.Length);
        }

        [TestMethod]
        public void CanParseComplexResult()
        {
            var array = new RESPArray(BuildArray("a", "b", "c", "d", "e", "f"),
                                      BuildArray("g", "h", "i", "j", "k", "l"));

            var result = new RedisResults(new[] { array });

            var rarray = result[0].AsResults()[0].GetStringArray();
            Assert.IsNotNull(rarray);
            Assert.AreEqual(6, rarray.Length);

            rarray = result[0].AsResults()[1].GetStringArray();
            Assert.IsNotNull(rarray);
            Assert.AreEqual(6, rarray.Length);
        }

        [TestMethod]
        public void CanParseDictionaryResult()
        {
            var array = BuildArray("a", "b", "c", "d", "e", "f");

            var result = new RedisResults(new[] { array });

            var dic = result[0].AsDictionaryCollation<String,String>();
            Assert.IsNotNull(dic);
            Assert.AreEqual("b", dic["a"]);
            Assert.AreEqual("d", dic["c"]);
            Assert.AreEqual("f", dic["e"]);
        }

        [TestMethod]
        public void CanParseIntDictionaryResult()
        {
            var array = BuildArray("a", "1", "c", "2", "e", "3");

            var result = new RedisResults(new[] { array });

            var dic = result[0].AsDictionaryCollation<String, Int32>();
            Assert.IsNotNull(dic);
            Assert.AreEqual(1, dic["a"]);
            Assert.AreEqual(2, dic["c"]);
            Assert.AreEqual(3, dic["e"]);
        }

        [TestMethod]
        public void CanParseDecimalDictionaryResult()
        {
            var array = BuildArray("a", "1.001", "c", "2", "e", "3.03");

            var result = new RedisResults(new[] { array });

            var dic = result[0].AsDictionaryCollation<String, Decimal>();
            Assert.IsNotNull(dic);
            Assert.AreEqual(1.001M, dic["a"]);
            Assert.AreEqual(2M, dic["c"]);
            Assert.AreEqual(3.03M, dic["e"]);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientBindingException), MessagePattern="^Cannot create Dictionary<String,Int32> from response")]
        public void FailsParsingBadIntDictionaryResult()
        {
            var array = BuildArray("a", "a", "c", "2", "e", "3");

            var result = new RedisResults(new[] { array });

            var dic = result[0].AsDictionaryCollation<String, Int32>();
        }

        class Collation1
        {
            public String Member1 { get; set; }
            public Int32 Member2 { get; set; }
            public Double Member3 { get; set; }
        }

        [TestMethod]
        public void CanDoObjectCollationResult()
        {
            var array = BuildArray("Member1", "Hi", "member2", "2", "MEMBER3", "3.03");

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation1>();
            Assert.IsNotNull(obj);
            Assert.AreEqual("Hi", obj.Member1);
            Assert.AreEqual(2, obj.Member2);
            Assert.AreEqual(3.03D, obj.Member3);
        }
        [TestMethod]
        public void CanDoObjectCollationResultWithCulture()
        {
            var culture = CultureInfo.CurrentCulture;
            try
            {
                var array = BuildArray("Member1", "Hi", "member2", "2", "MEMBER3", "3.03");

                var result = new RedisResults(new[] { array });

                var obj = result[0].AsObjectCollation<Collation1>();
                Assert.IsNotNull(obj);
                Assert.AreEqual("Hi", obj.Member1);
                Assert.AreEqual(2, obj.Member2);
                Assert.AreEqual(3.03D, obj.Member3);
            }
            finally
            {
                CultureInfo.DefaultThreadCurrentCulture = culture;
            }

        }

        [TestMethod]
        public void CanDoObjectCollationResultWithNulls()
        {
            var array = BuildArray("Member1", null, "member2", "2", "MEMBER3", "3.03");

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation1>();
            Assert.IsNotNull(obj);
            Assert.AreEqual(null, obj.Member1);
            Assert.AreEqual(2, obj.Member2);
            Assert.AreEqual(3.03D, obj.Member3);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientBindingException), MessagePattern="^Cannot set member 'member2'$")]
        public void FailOnObjectCollationResultWithNullInValueType()
        {
            var array = BuildArray("Member1", "Hi", "member2", null, "MEMBER3", "3.03");

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation1>();
            Assert.IsNotNull(obj);
            Assert.AreEqual("Hi", obj.Member1);
            Assert.AreEqual(2, obj.Member2);
            Assert.AreEqual(3.03D, obj.Member3);
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientBindingException), MessagePattern="^Cannot set member 'MEMBER3'$")]
        public void FailOnImpossibleParseCollationResult()
        {
            var array = BuildArray("Member1", "Hi", "member2", "2", "MEMBER3", "3.03TEXT");

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation1>();
            Assert.IsNotNull(obj);
            Assert.AreEqual("Hi", obj.Member1);
            Assert.AreEqual(2, obj.Member2);
            Assert.AreEqual(3.03D, obj.Member3);
        }

        class Collation2
        {
            public String Member1 { get; set; }
            public Int32[] Member2 { get; set; }
            public Double Member3 { get; set; }
        }

        [TestMethod]
        public void CanFailOnWrongObjectCollationMember()
        {
            var array = BuildArray("Member1", "Hi", "member2", "2", "MEMBER3", "3.03");

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation2>();
            Assert.IsNotNull(obj);
            Assert.AreEqual("Hi", obj.Member1);
            Assert.IsNull(obj.Member2);
            Assert.AreEqual(3.03D, obj.Member3);

            try
            {
                obj = result[0].AsObjectCollation<Collation2>(false, false);
            }
            catch (RedisClientBindingException)
            {
                return;
            }
            Assert.Fail();
        }

        class Collation3
        {
            public String Member1 { get; set; }
            public Int32? Member2 { get; set; }
            public Double? Member3 { get; set; }
        }

        [TestMethod]
        public void CanDoObjectCollationResultWithNullablesInValueType()
        {
            var array = BuildArray("Member1", "1", "Member2", "2", "Member3", "3.33");

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation3>(false, false);
            Assert.IsNotNull(obj);
            Assert.AreEqual("1", obj.Member1);
            Assert.AreEqual(2, obj.Member2);
            Assert.AreEqual(3.33D, obj.Member3);
        }

        [TestMethod]
        public void CanDoObjectCollationResultWithNullInNullables()
        {
            var array = BuildArray("Member1", null, "Member2", null, "Member3", null);

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation3>(false, false);
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Member1);
            Assert.IsNull(obj.Member2);
            Assert.IsNull(obj.Member3);
        }

        [TestMethod]
        public void CanDoObjectCollationResultOverridingWithNullablesInValueType()
        {
            var array = BuildArray("Member1", null, "Member2", null, "Member3", null);

            var result = new RedisResults(new[] { array });

            var obj = new Collation3()
            {
                Member1 = "test",
                Member2 = 1,
                Member3 = 2
            };

            result[0].AsObjectCollation<Collation3>(obj, false, false);
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Member1);
            Assert.IsNull(obj.Member2);
            Assert.IsNull(obj.Member3);
        }

        [TestMethod]
        public void CanAssertOKResult()
        {
            var result = new RedisResults(new RESPObject[] 
            { 
                new RESPSimpleString("OK"), 
            },
            new[] { "incr" });

            Assert.IsNotNull(result);
            result[0].AssertOK();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientCommandException), MessagePattern="ERRORR")]
        public void CanDetectErrorOnAssertOK()
        {
            var result = new RedisResults(new RESPObject[] 
            { 
                new RESPError("ERRORR") 
            },
            new[] { "incr" });

            Assert.IsNotNull(result);
            result[0].AssertOK();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientAssertException), MessagePattern="^Expected result for command was 'OK' but a 'Integer' was received instead$")]
        public void CanDetectDiffentTypeOnAssertOK()
        {
            var result = new RedisResults(new RESPObject[] 
            { 
                new RESPInteger(33) 
            },
            new[] { "incr" });

            Assert.IsNotNull(result);
            result[0].AssertOK();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientAssertException), MessagePattern = "^Expected result for command was 'OK' but result was 'NOK'$")]
        public void CanDetectDifferentValueOnAssertOK()
        {
            var result = new RedisResults(new RESPObject[] 
            { 
                new RESPSimpleString("NOK") 
            },
            new[] { "incr" });

            Assert.IsNotNull(result);
            result[0].AssertOK();
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientAssertException), MessagePattern = "^Expected result for command was 'OK' but result was '<null>'$")]
        public void CanDetectNullValueOnAssertOK()
        {
            var result = new RedisResults(new RESPObject[] 
            { 
                new RESPSimpleString(null) 
            },
            new[] { "incr" });

            Assert.IsNotNull(result);
            result[0].AssertOK();
        }

        class Collation4
        {
            public DateTime Member1 { get; set; }
            public DateTime Member2 { get; set; }
            public DateTime? Member3 { get; set; }
            public DateTime? Member4 { get; set; }
        }

        [TestMethod]
        public void CanDoDateTimeCollationResult()
        {
            var d1 = new DateTime(1985, 5, 5, 17, 55, 00);
            var d2 = new DateTime(2000, 12, 15, 13, 10, 00);

            var array = BuildArray("Member1", d1.ToBinary().ToString(), 
                                   "member2", d2.ToBinary().ToString(), 
                                   "MEMBER3", d1.ToBinary().ToString(),
                                   "MeMBer4", d2.ToBinary().ToString());

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation4>();
            Assert.IsNotNull(obj);
            Assert.AreEqual(d1, obj.Member1);
            Assert.AreEqual(d2, obj.Member2);
            Assert.AreEqual(d1, obj.Member3);
            Assert.AreEqual(d2, obj.Member4);
        }

        public enum TestEnum { TestValue=1, TestValue2= 50 }

        class Collation5
        {
            public TestEnum Member1 { get; set; }
            public TestEnum Member2 { get; set; }
        }

        [TestMethod]
        public void CanDoEnumCollationResult()
        {
            var array = BuildArray("Member1", "1",
                                   "member2", "50");

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation5>();
            Assert.IsNotNull(obj);
            Assert.AreEqual(TestEnum.TestValue, obj.Member1);
            Assert.AreEqual(TestEnum.TestValue2, obj.Member2);
        }

        [TestMethod]
        public void CanParseEnums()
        {
            var result = new RedisResults(new RESPObject[] 
            { 
                new RESPSimpleString("50") 
            });

            var val = result[0].AsEnum<TestEnum>();
            Assert.IsNotNull(val);
            Assert.AreEqual(TestEnum.TestValue2, val);
        }

        class Collation6
        {
            public TestEnum? Member1 { get; set; }
        }

        [TestMethod]
        [ExpectedExceptionPattern(typeof(RedisClientBindingException), MessagePattern="Nullable enums are not supported")]
        public void FailWithNullableEnumerations()
        {
            var array = BuildArray("Member1", "1");

            var result = new RedisResults(new[] { array });

            var obj = result[0].AsObjectCollation<Collation6>();
            Assert.IsNotNull(obj);
            Assert.AreEqual(TestEnum.TestValue, obj.Member1);
        }
    }
}
