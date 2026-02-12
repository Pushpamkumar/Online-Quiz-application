using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Rate Limiter with Sliding Window
/// Rule: Max 5 requests per 10 seconds per client
/// </summary>
public class RateLimiter
{
    private readonly Dictionary<string, Queue<DateTime>> clientRequestHistory;
    private readonly int maxRequests = 5;
    private readonly TimeSpan windowSize = TimeSpan.FromSeconds(10);
    private readonly object lockObject = new object();

    public RateLimiter()
    {
        clientRequestHistory = new Dictionary<string, Queue<DateTime>>();
    }

    /// <summary>
    /// Check if a request is allowed for the given client
    /// Uses sliding window: only counts requests within last 10 seconds
    /// </summary>
    public bool AllowRequest(string clientId, DateTime now)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("Client ID cannot be null or empty");

        lock (lockObject)
        {
            // Get or create queue for this client
            if (!clientRequestHistory.ContainsKey(clientId))
            {
                clientRequestHistory[clientId] = new Queue<DateTime>();
            }

            var requestQueue = clientRequestHistory[clientId];

            // Remove old requests outside the 10-second window
            // If a request is older than 10 seconds, remove it
            while (requestQueue.Count > 0 && (now - requestQueue.Peek()) > windowSize)
            {
                requestQueue.Dequeue();
            }

            // Check if client has hit the limit
            if (requestQueue.Count >= maxRequests)
            {
                return false; // Request denied
            }

            // Allow the request and record it
            requestQueue.Enqueue(now);
            return true; // Request allowed
        }
    }

    /// <summary>
    /// Get remaining requests for a client (for monitoring)
    /// </summary>
    public int GetRemainingRequests(string clientId, DateTime now)
    {
        lock (lockObject)
        {
            if (!clientRequestHistory.ContainsKey(clientId))
                return maxRequests;

            var requestQueue = clientRequestHistory[clientId];

            // Remove expired requests
            while (requestQueue.Count > 0 && (now - requestQueue.Peek()) > windowSize)
            {
                requestQueue.Dequeue();
            }

            return maxRequests - requestQueue.Count;
        }
    }

    /// <summary>
    /// Reset all data (useful for testing)
    /// </summary>
    public void Reset()
    {
        lock (lockObject)
        {
            clientRequestHistory.Clear();
        }
    }
}

// ============= USAGE EXAMPLE =============
class RateLimiterDemo
{
    static void Main()
    {
        var limiter = new RateLimiter();
        var now = DateTime.Now;

        Console.WriteLine("=== Rate Limiter Sliding Window Demo ===\n");

        // Simulate Client A making requests
        string clientA = "ClientA";
        Console.WriteLine($"Client: {clientA}");
        
        for (int i = 1; i <= 7; i++)
        {
            bool allowed = limiter.AllowRequest(clientA, now);
            Console.WriteLine($"  Request {i}: {(allowed ? "✓ ALLOWED" : "✗ DENIED")}");
        }

        Console.WriteLine($"\nRemaining requests for {clientA}: {limiter.GetRemainingRequests(clientA, now)}");

        // Simulate time passing (6 seconds)
        Console.WriteLine("\n--- 6 seconds pass ---");
        var later = now.AddSeconds(6);
        bool allowed2 = limiter.AllowRequest(clientA, later);
        Console.WriteLine($"Request after 6s: {(allowed2 ? "✓ ALLOWED" : "✗ DENIED")}");

        // Simulate time passing (5 more seconds, total 11 seconds)
        Console.WriteLine("\n--- 5 more seconds pass (total 11s) ---");
        var muchLater = now.AddSeconds(11);
        bool allowed3 = limiter.AllowRequest(clientA, muchLater);
        Console.WriteLine($"Request after 11s: {(allowed3 ? "✓ ALLOWED" : "✗ DENIED")}");
        Console.WriteLine($"Remaining requests: {limiter.GetRemainingRequests(clientA, muchLater)}");

        // Test different clients
        Console.WriteLine("\n\nDifferent clients are independent:");
        string clientB = "ClientB";
        for (int i = 1; i <= 6; i++)
        {
            limiter.AllowRequest(clientB, now);
        }
        Console.WriteLine($"{clientB} can make 6 requests independently");
    }
}
