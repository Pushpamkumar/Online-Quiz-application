using System;
using System.Collections.Generic;

/// <summary>
/// Advanced Cache with:
/// - TTL (Time To Live): Each key expires after specified seconds
/// - LRU Eviction: When capacity is full, remove Least Recently Used item
/// - Thread-safe operations
/// </summary>
public class AdvancedCache<TKey, TValue>
{
    private class CacheItem
    {
        public TValue Value { get; set; }
        public DateTime ExpiryTime { get; set; }
        public DateTime LastAccessTime { get; set; }
    }

    private readonly int capacity;
    private readonly Dictionary<TKey, CacheItem> cacheData;
    private readonly LinkedList<TKey> accessOrder; // Track access order for LRU
    private readonly Dictionary<TKey, LinkedListNode<TKey>> accessMap; // Quick access to node
    private readonly object lockObject = new object();

    public AdvancedCache(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0");

        this.capacity = capacity;
        this.cacheData = new Dictionary<TKey, CacheItem>();
        this.accessOrder = new LinkedList<TKey>();
        this.accessMap = new Dictionary<TKey, LinkedListNode<TKey>>();
    }

    /// <summary>
    /// Set a key-value pair with TTL (expiry time in seconds)
    /// </summary>
    public void Set(TKey key, TValue value, int ttlSeconds)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (ttlSeconds <= 0)
            throw new ArgumentException("TTL must be greater than 0");

        lock (lockObject)
        {
            // Remove expired items first
            RemoveExpiredItems();

            // If key already exists, update it
            if (cacheData.ContainsKey(key))
            {
                cacheData[key] = new CacheItem
                {
                    Value = value,
                    ExpiryTime = DateTime.Now.AddSeconds(ttlSeconds),
                    LastAccessTime = DateTime.Now
                };

                // Update access order - move to end (most recent)
                accessOrder.Remove(accessMap[key]);
                var newNode = accessOrder.AddLast(key);
                accessMap[key] = newNode;

                return;
            }

            // If cache is at capacity, remove LRU item
            if (cacheData.Count >= capacity)
            {
                // First item in list is least recently used
                var lruKey = accessOrder.First.Value;
                accessOrder.RemoveFirst();
                cacheData.Remove(lruKey);
                accessMap.Remove(lruKey);
                Console.WriteLine($"  [Cache evicted LRU key: {lruKey}]");
            }

            // Add new item
            cacheData[key] = new CacheItem
            {
                Value = value,
                ExpiryTime = DateTime.Now.AddSeconds(ttlSeconds),
                LastAccessTime = DateTime.Now
            };

            var node = accessOrder.AddLast(key);
            accessMap[key] = node;
        }
    }

    /// <summary>
    /// Get a value from cache if it exists and hasn't expired
    /// Returns null if key not found or expired
    /// </summary>
    public TValue Get(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        lock (lockObject)
        {
            // Check if key exists
            if (!cacheData.ContainsKey(key))
                return default(TValue);

            var item = cacheData[key];

            // Check if item has expired
            if (DateTime.Now > item.ExpiryTime)
            {
                // Remove expired item
                cacheData.Remove(key);
                accessOrder.Remove(accessMap[key]);
                accessMap.Remove(key);
                return default(TValue);
            }

            // Update last access time and move to end (most recent)
            item.LastAccessTime = DateTime.Now;
            accessOrder.Remove(accessMap[key]);
            var newNode = accessOrder.AddLast(key);
            accessMap[key] = newNode;

            return item.Value;
        }
    }

    /// <summary>
    /// Remove all expired items from cache
    /// </summary>
    private void RemoveExpiredItems()
    {
        var expiredKeys = new List<TKey>();

        foreach (var entry in cacheData)
        {
            if (DateTime.Now > entry.Value.ExpiryTime)
            {
                expiredKeys.Add(entry.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            cacheData.Remove(key);
            accessOrder.Remove(accessMap[key]);
            accessMap.Remove(key);
        }
    }

    /// <summary>
    /// Get current cache size
    /// </summary>
    public int Count
    {
        get
        {
            lock (lockObject)
            {
                RemoveExpiredItems();
                return cacheData.Count;
            }
        }
    }

    /// <summary>
    /// Clear all cache
    /// </summary>
    public void Clear()
    {
        lock (lockObject)
        {
            cacheData.Clear();
            accessOrder.Clear();
            accessMap.Clear();
        }
    }
}

// ============= USAGE EXAMPLE =============
class AdvancedCacheDemo
{
    static void Main()
    {
        Console.WriteLine("=== Advanced Cache Demo (TTL + LRU) ===\n");

        // Create cache with capacity of 3 items
        var cache = new AdvancedCache<string, string>(capacity: 3);

        // Example 1: Add items with 5-second TTL
        Console.WriteLine("--- Adding 3 items (capacity = 3, TTL = 5 seconds) ---");
        cache.Set("user1", "Alice", ttlSeconds: 5);
        Console.WriteLine("Set user1 = Alice");

        cache.Set("user2", "Bob", ttlSeconds: 5);
        Console.WriteLine("Set user2 = Bob");

        cache.Set("user3", "Charlie", ttlSeconds: 5);
        Console.WriteLine("Set user3 = Charlie");
        Console.WriteLine($"Cache size: {cache.Count}\n");

        // Example 2: Try to add 4th item (triggers LRU eviction)
        Console.WriteLine("--- Adding 4th item (triggers LRU eviction) ---");
        cache.Set("user4", "Diana", ttlSeconds: 5);
        Console.WriteLine("Set user4 = Diana (user1 was evicted as LRU)");
        Console.WriteLine($"Cache size: {cache.Count}\n");

        // Example 3: Retrieve items (updates access time)
        Console.WriteLine("--- Retrieving items (updates LRU order) ---");
        Console.WriteLine($"Get user2: {cache.Get("user2")}");
        Console.WriteLine($"Get user3: {cache.Get("user3")}");
        Console.WriteLine($"Cache size: {cache.Count}\n");

        // Example 4: Add another item (user4 is now LRU since it hasn't been accessed)
        Console.WriteLine("--- Adding 5th item (user4 is evicted as it's least recently used) ---");
        cache.Set("user5", "Eve", ttlSeconds: 5);
        Console.WriteLine("Set user5 = Eve");
        Console.WriteLine($"Cache size: {cache.Count}\n");

        // Example 5: Expired item is removed
        Console.WriteLine("--- Waiting for TTL expiry (simulated) ---");
        Console.WriteLine("(In real scenario, wait 5 seconds for items to expire)");
        Console.WriteLine("When you Get an expired item, it's automatically removed:");
        // In real code, you would: System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
        Console.WriteLine("Get expired user2: " + (cache.Get("user2") ?? "[Item expired, removed]"));
        Console.WriteLine($"Cache size after expiry: {cache.Count}\n");

        // Example 6: Numbers cache
        Console.WriteLine("--- Numeric Cache Example ---");
        var numberCache = new AdvancedCache<int, int>(capacity: 2);
        numberCache.Set(1, 100, ttlSeconds: 10);
        numberCache.Set(2, 200, ttlSeconds: 10);
        Console.WriteLine($"Key 1: {numberCache.Get(1)}");
        Console.WriteLine($"Key 2: {numberCache.Get(2)}");

        numberCache.Set(3, 300, ttlSeconds: 10);
        Console.WriteLine("Added key 3 (key 1 was evicted as LRU)");
        Console.WriteLine($"Key 1 (was evicted): {numberCache.Get(1) ?? "0"}");
        Console.WriteLine($"Cache size: {numberCache.Count}");
    }
}
