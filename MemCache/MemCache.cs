using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MemCache
{
    /// <summary>
    /// Mem cache that implements a set number of records that can be held in memory. Once that
    /// limit is exceeded the oldest data cached will be removed to make space for the new one.
    /// The cache assumes less write in comparison to read whereby the write operation (AddOrUpdate) will block threads but
    /// reads will allow all threads to access the cache. However if the writelock is entered then reads will be queued until write 
    /// is completed. 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="MemCache.ICache{TKey, TValue}" />
    public class MemCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly int _maxNumberOfRecordsToStore;

        private readonly Dictionary<TKey,TValue> _dictionaryCache = new Dictionary<TKey,TValue>();

        //another internal list to track the history of the data being inserted into cache. Since list keeps order of insertion
        //its better to use this for tracking the history
        private readonly List<TKey> _historyOfInserts = new List<TKey>();

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
            //Get exclusive lock
            _readerWriterLocker.EnterWriteLock();
            try
            {
                if (!_dictionaryCache.ContainsKey(key))
                {
                    if (_dictionaryCache.Count >= _maxNumberOfRecordsToStore)
                    {
                        //remove oldest record if the capacity has been reached
                        var oldest = _historyOfInserts.First();
                        _dictionaryCache.Remove(oldest);
                        _historyOfInserts.Remove(oldest);
                    }
                    _dictionaryCache.Add(key, value);
                    _historyOfInserts.Add(key);
                }
                else
                {
                    //removes the entry and adds it to the bottom of the list to ensure that 
                    //any data that gets updated relocated in the history list.
                    _dictionaryCache[key] = value;

                    _historyOfInserts.Remove(key);
                    _historyOfInserts.Add(key);
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

            //create shared lock for reading threads
            _readerWriterLocker.EnterReadLock();
            try
            {
                if (_dictionaryCache.ContainsKey(key))
                {
                    value = _dictionaryCache[key];
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