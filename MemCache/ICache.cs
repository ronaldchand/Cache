namespace MemCache
{
    /// <summary>
    /// Interface for implmenting different implementations of a cache. For now only
    /// Mem change uses this interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface ICache<in TKey, TValue>
    {
        /// <summary>
        /// Adds the value to the cache against the specified key.
        /// If the key already exists, its value is updated.
        /// </summary>
        void AddOrUpdate(TKey key, TValue value);
        /// <summary>
        /// Attempts to gets the value from the cache against the specified key
        /// and returns true if the key existed in the cache.
        /// </summary>
        bool TryGetValue(TKey key, out TValue value);
    }
}
