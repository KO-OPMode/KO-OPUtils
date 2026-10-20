using System.Collections.Generic;
using NUnit.Framework;

namespace KOOP.Tests
{
    public class StableHashCodeTests
    {
        /// <summary>
        /// Unit tests for our StableHashCode class
        /// Since hashing creates counterintuitive output by its nature, most of these tests were generated
        /// by running them and then recording the output. This doesn't really validate that the hash is 'correct'
        /// so much as it validates that it remains stable across runs.
        /// </summary>
        /// <owner>jonagill</owner>
        private struct TestHashCodeProvider : IStableHashCodeProvider
        {
            public int Value;

            public int GetStableHashCode()
            {
                return Value.GetStableHashCode();
            }
        }
        
        [Test]
        [TestCase(1234, 1234, -710215995)]
        [TestCase(true, 1, 1464945392)]
        [TestCase("Hello", 2062843556, -128462832)]
        public void HashPrimitives(object value, int valueHash, int combinedHash)
        {
            var hashCode = new StableHashCode();
            hashCode.Add(value);
            Assert.AreEqual(valueHash, value.GetStableHashCode());
            Assert.AreEqual(combinedHash, hashCode.ToHashCode());
        }
        
        [Test]
        public void HashIStableHashCodeProvider()
        {
            var value = new TestHashCodeProvider()
            {
                Value = 1234
            };
            
            var hashCode = new StableHashCode();
            hashCode.Add(value);
            Assert.AreEqual(-710215995, hashCode.ToHashCode());
        }

        [Test]
        public void HashArray()
        {
            var array = new object[] { 1, false, float.MaxValue, "Testing" };
            var hashCode = new StableHashCode();
            hashCode.Add(array);
            Assert.AreEqual(-471264082, hashCode.ToHashCode());
        }
        
        [Test]
        public void HashDictionary()
        {
            var dictionary = new Dictionary<object, object>()
            {
                { 1, false },
                { "Key", "Value" },
                {
                    'a', new TestHashCodeProvider()
                    {
                        Value = 0
                    }
                },
                { "Nested", new int[] { 0, 1, 2, 99 }}
            };
            
            var hashCode = new StableHashCode();
            hashCode.Add(dictionary);
            Assert.AreEqual(-972993533, hashCode.ToHashCode());
        }
        
        [Test]
        public void Combine()
        {
            int combined = StableHashCode.Combine(0, "Test", false, 420.69f);
            Assert.AreEqual(747798083, combined);
        }
    }
}
