using System;
using System.Collections.Specialized;
using System.Threading;

namespace MemCache
{
    /// <summary>
    /// Mem cache that implements a set number of records that can be held in memory. Once that
    /// limit is exceeded the oldest data cached will be removed to make space for the new one.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="MemCache.ICache{TKey, TValue}" />
    public class MemCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly int _maxNumberOfRecordsToStore;

        private readonly OrderedDictionary _orderedDictionaryCache = new OrderedDictionary();
        private readonly ReaderWriterLockSlim _readerWriterLocker = new ReaderWriterLockSlim();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemCache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="maxNumberOfRecordsToStore">The number of records to store.</param>
        /// <exception cref="ArgumentException"></exception>
        public MemCache(int maxNumberOfRecordsToStore)
        {
            if (maxNumberOfRecordsToStore <= 0) throw new ArgumentException($"Maximum number of records to store must greater than zero");
            _maxNumberOfRecordsToStore = maxNumberOfRecordsToStore;
        }

        /// <summary>
        /// Adds the value to the cache against the specified key.
        /// If the key already exists, its value is updated.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdate(TKey key, TValue value)
        {
            _readerWriterLocker.EnterWriteLock();
            try
            {
                if (!_orderedDictionaryCache.Contains(key))
                {
                    if (_orderedDictionaryCache.Count >= _maxNumberOfRecordsToStore)
                    {
                        //remove oldest record if the capacity has been reached
                        _orderedDictionaryCache.RemoveAt(0); 
                    }
                    _orderedDictionaryCache.Add(key, value);
                }
                else
                {
                    //removes the entry and adds it to the bottom of the list to ensure that 
                    //any data that gets updated relocated in the dictionary.
                    _orderedDictionaryCache.Remove(key);
                    _orderedDictionaryCache.Add(key, value);
                }
            }
            finally
            {
                _readerWriterLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Attempts to gets the value from the cache against the specified key
        /// and returns true if the key existed in the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var valueFoundInCache = false;
            value = default(TValue);

            _readerWriterLocker.EnterReadLock();
            try
            {
                if (_orderedDictionaryCache.Contains(key))
                {
                    value = (TValue) _orderedDictionaryCache[key];
                    valueFoundInCache = true;
                }
            }
            catch (Exception)
            {
                value = default(TValue);
            }
            finally
            {
                _readerWriterLocker.ExitReadLock();
            }

            return valueFoundInCache;
        }       
    }
}