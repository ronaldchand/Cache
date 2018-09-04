using System;
using MemCache;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class MenCacheTests
    {
        private MemCache<string, string> _memCache;

        [SetUp]
        public void Init()
        {
            _memCache = new MemCache<string,string>(2); 
        }
        
        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        public void MemCache_InvalidMaxNumberOfRecordsToStoreValue_ShouldThrowArgumentException(int num)
        {
            Assert.Throws<ArgumentException>(() => new MemCache<string, object>(num));
        }

        [Test]
        public void AddOrUpdate_NullKeyValuePassed_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _memCache.AddOrUpdate(null, "somedata"));
        }

        [Test]
        [TestCase("NullValue", null)]
        [TestCase("ActualValue", "testing")]
        public void AddOrUpdate_ValuePassedIn_ShouldAddToCache(string key, string value)
        {
            _memCache.AddOrUpdate(key, value);
            _memCache.TryGetValue(key, out var dataAdded);
            Assert.AreEqual(value, dataAdded);
        }

        [Test]
        public void AddOrUpdate_EntryExceedsLimit_ShouldRemoveTheOldestFromTheList()
        {
            _memCache.AddOrUpdate("firstKey", "firstValue");
            _memCache.AddOrUpdate("secondKey", "secondValue");
            _memCache.AddOrUpdate("thirdKey", "thirdValue");
            
            Assert.IsFalse(_memCache.TryGetValue("firstKey", out var dataNotFound));
            Assert.IsTrue(_memCache.TryGetValue("secondKey", out var secondValue));
            Assert.IsTrue(_memCache.TryGetValue("thirdKey", out var thirdValue));
        }

        [Test]
        public void AddOrUpdate_UpdateTheOldestAndExceedValue_ShouldNotRemoveTheUpdatedValue()
        {
            _memCache.AddOrUpdate("firstKey", "firstValue");
            _memCache.AddOrUpdate("secondKey", "secondValue");
            _memCache.AddOrUpdate("firstKey", "newFirstKeyValue");
            _memCache.AddOrUpdate("thirdKey", "thirdKeyValue");

            _memCache.TryGetValue("firstKey", out var firstValue);
            Assert.IsFalse(_memCache.TryGetValue("secondKey", out var secondKey));
            Assert.AreEqual("newFirstKeyValue", firstValue);
        }

        //[Test]
        //public void TryGetValue_InvalidOutValueDefined_ShouldReturnFalseAndDefaultValue()
        //{
        //    _memCache.AddOrUpdate("firstKey", 1);

        //    var result = _memCache.TryGetValue("firstKey", out string value);
        //}
    }
}
