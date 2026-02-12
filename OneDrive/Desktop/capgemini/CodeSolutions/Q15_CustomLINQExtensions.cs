using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Custom LINQ-style extension methods using yield return
/// Implements: WhereEx, SelectEx, DistinctEx, GroupByEx
/// </summary>
public static class CustomLinqExtensions
{
    /// <summary>
    /// WhereEx: Filter items that match a condition
    /// Example: numbers.WhereEx(x => x > 5)
    /// </summary>
    public static IEnumerable<T> WhereEx<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        // Yield: lazy evaluation - only processes items as needed
        foreach (var item in source)
        {
            if (predicate(item))
                yield return item;
        }
    }

    /// <summary>
    /// SelectEx: Transform each item to a new form
    /// Example: words.SelectEx(w => w.Length)
    /// </summary>
    public static IEnumerable<TResult> SelectEx<T, TResult>(
        this IEnumerable<T> source,
        Func<T, TResult> selector)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (selector == null)
            throw new ArgumentNullException(nameof(selector));

        foreach (var item in source)
        {
            yield return selector(item);
        }
    }

    /// <summary>
    /// DistinctEx: Return unique items (duplicates removed)
    /// Example: numbers.DistinctEx()
    /// </summary>
    public static IEnumerable<T> DistinctEx<T>(this IEnumerable<T> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var seen = new HashSet<T>();

        foreach (var item in source)
        {
            // Only yield if we haven't seen this value before
            if (seen.Add(item)) // Add returns true if item was not already in set
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// GroupByEx: Group items by a key selector
    /// Returns IEnumerable of groups (each group has a Key and items)
    /// Example: people.GroupByEx(p => p.City)
    /// </summary>
    public static IEnumerable<IGrouping<TKey, T>> GroupByEx<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));

        var groups = new Dictionary<TKey, List<T>>();

        // Groups items into buckets by key
        foreach (var item in source)
        {
            var key = keySelector(item);

            if (!groups.ContainsKey(key))
            {
                groups[key] = new List<T>();
            }

            groups[key].Add(item);
        }

        // Yield each group
        foreach (var group in groups)
        {
            yield return new GroupImpl<TKey, T>(group.Key, group.Value);
        }
    }

    /// <summary>
    /// Helper class to implement IGrouping interface
    /// </summary>
    private class GroupImpl<TKey, T> : List<T>, IGrouping<TKey, T>
    {
        public TKey Key { get; }

        public GroupImpl(TKey key, IEnumerable<T> items)
            : base(items)
        {
            Key = key;
        }
    }
}

// ============= USAGE EXAMPLE =============
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string City { get; set; }

    public override string ToString() => $"{Name} ({Age} years, {City})";
}

class CustomLinqDemo
{
    static void Main()
    {
        Console.WriteLine("=== Custom LINQ Extension Methods Demo ===\n");

        // Sample data
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 25, City = "NYC" },
            new Person { Name = "Bob", Age = 30, City = "LA" },
            new Person { Name = "Charlie", Age = 25, City = "NYC" },
            new Person { Name = "Diana", Age = 35, City = "LA" },
            new Person { Name = "Eve", Age = 25, City = "Chicago" }
        };

        // 1. WhereEx: Filter people older than 25
        Console.WriteLine("--- WhereEx: People older than 25 ---");
        var olderThan25 = people.WhereEx(p => p.Age > 25);
        foreach (var person in olderThan25)
        {
            Console.WriteLine($"  {person}");
        }

        // 2. SelectEx: Get just the names
        Console.WriteLine("\n--- SelectEx: Get just the names ---");
        var names = people.SelectEx(p => p.Name);
        foreach (var name in names)
        {
            Console.WriteLine($"  {name}");
        }

        // 3. SelectEx: Transform to a custom format
        Console.WriteLine("\n--- SelectEx: Name and City only ---");
        var nameCities = people.SelectEx(p => $"{p.Name} lives in {p.City}");
        foreach (var info in nameCities)
        {
            Console.WriteLine($"  {info}");
        }

        // 4. DistinctEx: Get unique ages
        Console.WriteLine("\n--- DistinctEx: Unique ages ---");
        var ages = new[] { 25, 25, 30, 35, 25, 30 };
        var uniqueAges = ages.DistinctEx();
        Console.WriteLine($"  {string.Join(", ", uniqueAges)}");

        // 5. GroupByEx: Group people by city
        Console.WriteLine("\n--- GroupByEx: People grouped by City ---");
        var byCity = people.GroupByEx(p => p.City);
        foreach (var cityGroup in byCity)
        {
            Console.WriteLine($"  {cityGroup.Key}:");
            foreach (var person in cityGroup)
            {
                Console.WriteLine($"    - {person.Name}");
            }
        }

        // 6. GroupByEx: Group people by age
        Console.WriteLine("\n--- GroupByEx: People grouped by Age ---");
        var byAge = people.GroupByEx(p => p.Age);
        foreach (var ageGroup in byAge)
        {
            Console.WriteLine($"  Age {ageGroup.Key}: {string.Join(", ", ageGroup.SelectEx(p => p.Name))}");
        }

        // 7. Chaining multiple operations
        Console.WriteLine("\n--- Chaining: Age > 25, then map to names ---");
        var result = people
            .WhereEx(p => p.Age > 25)
            .SelectEx(p => p.Name.ToUpper());
        Console.WriteLine($"  {string.Join(", ", result)}");
    }
}
