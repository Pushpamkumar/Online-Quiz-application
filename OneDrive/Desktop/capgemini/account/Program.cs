using System;
using System.Collections.Generic;
using System.Linq;

// Base product interface
public interface IProduct
{
    int Id { get; }
    string Name { get; }
    decimal Price { get; }
    Category Category { get; }
}

public enum Category { Electronics, Clothing, Books, Groceries }

// 1. Create a generic repository for products
public class ProductRepository<T> where T : class, IProduct
{
    private List<T> _products = new List<T>();

    // Implement method to add product with validation
    public void AddProduct(T product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name cannot be empty.");

        if (product.Price <= 0)
            throw new ArgumentException("Price must be positive.");

        if (_products.Any(p => p.Id == product.Id))
            throw new ArgumentException("Product ID must be unique.");

        _products.Add(product);
    }

    // Create method to find products by predicate
    public IEnumerable<T> FindProducts(Func<T, bool> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return _products.Where(predicate);
    }

    // Calculate total inventory value
    public decimal CalculateTotalValue()
    {
        return _products.Sum(p => p.Price);
    }

    public List<T> GetAll()
    {
        return _products;
    }
}

// 2. Specialized electronic product
public class ElectronicProduct : IProduct
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public Category Category => Category.Electronics;
    public int WarrantyMonths { get; set; }
    public string Brand { get; set; }
}

// Clothing Product
public class ClothingProduct : IProduct
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public Category Category => Category.Clothing;
    public string Size { get; set; }
}

// Book Product
public class BookProduct : IProduct
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public Category Category => Category.Books;
    public string Author { get; set; }
}

// 3. Create a discounted product wrapper
public class DiscountedProduct<T> where T : IProduct
{
    private T _product;
    private decimal _discountPercentage;

    public DiscountedProduct(T product, decimal discountPercentage)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount must be between 0 and 100.");

        _product = product;
        _discountPercentage = discountPercentage;
    }

    // Implement calculated price with discount
    public decimal DiscountedPrice =>
        _product.Price * (1 - _discountPercentage / 100);

    // Override ToString to show discount details
    public override string ToString()
    {
        return $"{_product.Name} | Original: {_product.Price:C} | " +
               $"Discount: {_discountPercentage}% | Final: {DiscountedPrice:C}";
    }
}

// 4. Inventory manager with constraints
public class InventoryManager
{
    // Create method that accepts any IProduct collection
    public void ProcessProducts<T>(IEnumerable<T> products) where T : IProduct
    {
        if (products == null)
            throw new ArgumentNullException(nameof(products));

        Console.WriteLine("\n--- Product List ---");

        foreach (var p in products)
        {
            Console.WriteLine($"{p.Name} - {p.Price:C}");
        }

        // b) Find most expensive product
        var maxProduct = products.OrderByDescending(p => p.Price).FirstOrDefault();

        if (maxProduct != null)
        {
            Console.WriteLine($"\nMost Expensive: {maxProduct.Name} - {maxProduct.Price:C}");
        }

        // c) Group by category
        Console.WriteLine("\n--- Grouped By Category ---");

        var groups = products.GroupBy(p => p.Category);

        foreach (var group in groups)
        {
            Console.WriteLine($"\n{group.Key}:");

            foreach (var p in group)
            {
                Console.WriteLine($"  {p.Name}");
            }
        }

        // d) Apply 10% discount to Electronics over $500
        Console.WriteLine("\n--- Discounted Electronics (> $500) ---");

        var discounted = products
            .Where(p => p.Category == Category.Electronics && p.Price > 500)
            .Select(p => new DiscountedProduct<T>(p, 10));

        foreach (var d in discounted)
        {
            Console.WriteLine(d);
        }
    }

    // Implement bulk price update with delegate
    public void UpdatePrices<T>(List<T> products, Func<T, decimal> priceAdjuster)
        where T : IProduct
    {
        if (products == null || priceAdjuster == null)
            throw new ArgumentNullException();

        for (int i = 0; i < products.Count; i++)
        {
            try
            {
                var newPrice = priceAdjuster(products[i]);

                if (newPrice <= 0)
                    throw new Exception("Invalid price.");

                // Using reflection to set price (since interface is read-only)
                var prop = products[i].GetType().GetProperty("Price");
                prop?.SetValue(products[i], newPrice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating {products[i].Name}: {ex.Message}");
            }
        }
    }
}

// 5. TEST SCENARIO
class Program
{
    static void Main()
    {
        var repo = new ProductRepository<IProduct>();

        // Sample products
        var laptop = new ElectronicProduct
        {
            Id = 1,
            Name = "Laptop",
            Price = 800,
            Brand = "Dell",
            WarrantyMonths = 24
        };

        var phone = new ElectronicProduct
        {
            Id = 2,
            Name = "Smartphone",
            Price = 600,
            Brand = "Samsung",
            WarrantyMonths = 12
        };

        var tshirt = new ClothingProduct
        {
            Id = 3,
            Name = "T-Shirt",
            Price = 25,
            Size = "M"
        };

        var book = new BookProduct
        {
            Id = 4,
            Name = "C# Mastery",
            Price = 40,
            Author = "John Doe"
        };

        var headphones = new ElectronicProduct
        {
            Id = 5,
            Name = "Headphones",
            Price = 120,
            Brand = "Sony",
            WarrantyMonths = 18
        };

        // Add products
        repo.AddProduct(laptop);
        repo.AddProduct(phone);
        repo.AddProduct(tshirt);
        repo.AddProduct(book);
        repo.AddProduct(headphones);

        Console.WriteLine("Products added successfully.");

        // Find products by brand
        var dellProducts = repo.FindProducts(p =>
            p is ElectronicProduct e && e.Brand == "Dell");

        Console.WriteLine("\nDell Products:");

        foreach (var p in dellProducts)
        {
            Console.WriteLine(p.Name);
        }

        // Total value
        Console.WriteLine($"\nTotal Inventory Value: {repo.CalculateTotalValue():C}");

        // Discounts
        var discount = new DiscountedProduct<ElectronicProduct>(laptop, 15);

        Console.WriteLine("\nDiscount Applied:");
        Console.WriteLine(discount);

        // Inventory manager
        var manager = new InventoryManager();

        manager.ProcessProducts(repo.GetAll());

        // Bulk price update (increase 5%)
        manager.UpdatePrices(repo.GetAll(), p => p.Price * 1.05m);

        Console.WriteLine("\nAfter 5% Price Increase:");

        foreach (var p in repo.GetAll())
        {
            Console.WriteLine($"{p.Name} - {p.Price:C}");
        }

        Console.WriteLine($"\nNew Total Value: {repo.CalculateTotalValue():C}");
    }
}
