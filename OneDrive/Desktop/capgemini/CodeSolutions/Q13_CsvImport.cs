using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Product class
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public override string ToString()
    {
        return $"Product {Id}: {Name} - ${Price} (qty: {Quantity})";
    }
}

/// <summary>
/// Represents an import error for a row
/// </summary>
public class RowError
{
    public int RowNumber { get; set; }
    public string Reason { get; set; }

    public override string ToString()
    {
        return $"Row {RowNumber}: {Reason}";
    }
}

/// <summary>
/// Result of CSV import operation
/// Contains inserted products and list of failed rows
/// </summary>
public class ImportResult
{
    public int InsertedCount { get; set; }
    public List<Product> InsertedProducts { get; set; }
    public List<RowError> FailedRows { get; set; }
    public int TotalRowsProcessed { get; set; }

    public ImportResult()
    {
        InsertedProducts = new List<Product>();
        FailedRows = new List<RowError>();
    }

    public bool IsSuccessful => FailedRows.Count == 0;

    public void Display()
    {
        Console.WriteLine("\n=== IMPORT RESULT ===");
        Console.WriteLine($"Total rows processed: {TotalRowsProcessed}");
        Console.WriteLine($"Successfully inserted: {InsertedCount}");
        Console.WriteLine($"Failed rows: {FailedRows.Count}");

        if (InsertedCount > 0)
        {
            Console.WriteLine("\n--- Inserted Products ---");
            foreach (var product in InsertedProducts)
            {
                Console.WriteLine($"  ✓ {product}");
            }
        }

        if (FailedRows.Count > 0)
        {
            Console.WriteLine("\n--- Failed Rows ---");
            foreach (var error in FailedRows)
            {
                Console.WriteLine($"  ✗ {error}");
            }
        }
    }
}

/// <summary>
/// CSV Product Importer
/// Validates each row and reports errors for invalid rows
/// </summary>
public class CsvProductImporter
{
    /// <summary>
    /// Import products from CSV file
    /// Format: Id,Name,Price,Quantity
    /// </summary>
    public ImportResult ImportProducts(string filePath)
    {
        var result = new ImportResult();

        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be empty");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        try
        {
            var lines = File.ReadAllLines(filePath);
            result.TotalRowsProcessed = lines.Length - 1; // Exclude header

            if (lines.Length == 0)
                throw new InvalidOperationException("File is empty");

            // Skip header row
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var product = ParseAndValidateProduct(line, i + 1); // +1 because file rows are 1-indexed
                    result.InsertedProducts.Add(product);
                    result.InsertedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedRows.Add(new RowError
                    {
                        RowNumber = i + 1,
                        Reason = ex.Message
                    });
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error reading file: {ex.Message}");
        }
    }

    /// <summary>
    /// Parse and validate a CSV line
    /// Throws exception if validation fails
    /// </summary>
    private Product ParseAndValidateProduct(string line, int rowNumber)
    {
        var parts = line.Split(',');

        if (parts.Length != 4)
            throw new InvalidOperationException($"Expected 4 columns, found {parts.Length}");

        // Parse ID
        if (!int.TryParse(parts[0].Trim(), out int id))
            throw new InvalidOperationException("Invalid ID (must be integer)");

        if (id <= 0)
            throw new InvalidOperationException("ID must be greater than 0");

        // Parse Name
        string name = parts[1].Trim();
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Product name cannot be empty");

        if (name.Length < 3)
            throw new InvalidOperationException("Product name must be at least 3 characters");

        // Parse Price
        if (!decimal.TryParse(parts[2].Trim(), out decimal price))
            throw new InvalidOperationException("Invalid price (must be decimal)");

        if (price <= 0)
            throw new InvalidOperationException("Price must be greater than 0");

        // Parse Quantity
        if (!int.TryParse(parts[3].Trim(), out int quantity))
            throw new InvalidOperationException("Invalid quantity (must be integer)");

        if (quantity < 0)
            throw new InvalidOperationException("Quantity cannot be negative");

        return new Product
        {
            Id = id,
            Name = name,
            Price = price,
            Quantity = quantity
        };
    }
}

// ============= USAGE EXAMPLE =============
class CsvImportDemo
{
    static void Main()
    {
        Console.WriteLine("=== CSV Import with Partial Success Demo ===\n");

        // Create test CSV file with valid and invalid data
        string testFilePath = "products.csv";
        CreateTestCsvFile(testFilePath);

        // Import products
        var importer = new CsvProductImporter();
        var result = importer.ImportProducts(testFilePath);

        // Display results
        result.Display();

        Console.WriteLine($"\nSuccess rate: {(double)result.InsertedCount / result.TotalRowsProcessed * 100:F1}%");

        // Cleanup
        File.Delete(testFilePath);
        Console.WriteLine($"\nTest file '{testFilePath}' deleted");
    }

    static void CreateTestCsvFile(string filePath)
    {
        var lines = new[]
        {
            "Id,Name,Price,Quantity",
            "1,Laptop,999.99,5",
            "2,Mouse,25.50,120",
            "3,Monitor,299.99,8",
            "4,K,15.00,50",              // ERROR: Name too short
            "5,Keyboard,79.99,invalid",   // ERROR: Invalid quantity
            "6,,49.99,30",               // ERROR: Empty name
            "7,Headphones,89.99,25",
            "-8,USB Cable,5.99,200",     // ERROR: Negative ID
            "9,Webcam,120.00,12",
            "10,Invalid,-50.00,15",      // ERROR: Negative price
            "11,USB Hub,45.99,35",
            "abc,Speaker,199.99,4"      // ERROR: Invalid ID format
        };

        File.WriteAllLines(filePath, lines);
        Console.WriteLine($"Created test CSV file '{filePath}' with mixed valid/invalid data\n");
    }
}
