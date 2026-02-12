using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Order class representing a customer order
/// </summary>
public class Order
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }

    public override string ToString()
    {
        return $"Order {OrderId} by {CustomerName} (${Amount})";
    }
}

/// <summary>
/// Producer-Consumer Order Processing System
/// - Producer adds orders to queue
/// - 3 Consumer tasks process orders concurrently
/// - Graceful shutdown when done
/// </summary>
public class OrderProcessingSystem
{
    private readonly Queue<Order> orderQueue;
    private readonly object lockObject = new object();
    private int totalProcessed = 0;
    private bool isProducingComplete = false;

    public OrderProcessingSystem()
    {
        orderQueue = new Queue<Order>();
    }

    /// <summary>
    /// Producer: Add an order to the queue
    /// </summary>
    public void ProduceOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        lock (lockObject)
        {
            orderQueue.Enqueue(order);
            Console.WriteLine($"[PRODUCER] Added: {order}");
        }
    }

    /// <summary>
    /// Consumer: Get next order from queue (returns null if queue empty)
    /// </summary>
    private Order ConsumeOrder()
    {
        lock (lockObject)
        {
            if (orderQueue.Count > 0)
            {
                return orderQueue.Dequeue();
            }
            return null;
        }
    }

    /// <summary>
    /// Process an order (simulate with delay)
    /// </summary>
    private void ProcessOrder(Order order)
    {
        // Simulate processing time (1-3 seconds)
        int processingTime = new Random().Next(1000, 3000);
        Thread.Sleep(processingTime);

        lock (lockObject)
        {
            totalProcessed++;
        }
    }

    /// <summary>
    /// Worker task for a consumer
    /// Processes orders until production is complete and queue is empty
    /// </summary>
    private async Task ConsumerWorker(int workerId)
    {
        Console.WriteLine($"  [Consumer {workerId}] Started");

        while (true)
        {
            Order order = ConsumeOrder();

            if (order == null)
            {
                // Queue is empty, check if producer is done
                lock (lockObject)
                {
                    if (isProducingComplete && orderQueue.Count == 0)
                    {
                        Console.WriteLine($"  [Consumer {workerId}] All done, shutting down");
                        break;
                    }
                }

                // Wait a bit before checking again (avoid busy-loop)
                await Task.Delay(100);
                continue;
            }

            // Process order
            Console.WriteLine($"  [Consumer {workerId}] Processing: {order}");
            ProcessOrder(order);
            Console.WriteLine($"  [Consumer {workerId}] Completed: {order}");
        }
    }

    /// <summary>
    /// Run the entire producer-consumer system
    /// Returns: Total number of orders processed
    /// </summary>
    public async Task<int> RunAsync(List<Order> ordersToProcess)
    {
        if (ordersToProcess == null || ordersToProcess.Count == 0)
            throw new ArgumentException("Orders list cannot be empty");

        Console.WriteLine("\n=== Starting Order Processing System ===");
        Console.WriteLine($"Orders to process: {ordersToProcess.Count}");
        Console.WriteLine($"Number of workers: 3\n");

        // Start 3 consumer workers
        var consumerTasks = new List<Task>();
        for (int i = 1; i <= 3; i++)
        {
            consumerTasks.Add(ConsumerWorker(i));
        }

        // Producer: Add all orders to queue
        Console.WriteLine("[PRODUCER] Starting to add orders...");
        foreach (var order in ordersToProcess)
        {
            ProduceOrder(order);
            // Small delay between producing orders for realistic scenario
            await Task.Delay(200);
        }

        // Signal that producer is done
        lock (lockObject)
        {
            isProducingComplete = true;
        }
        Console.WriteLine("[PRODUCER] Done adding orders, waiting for consumers...\n");

        // Wait for all consumers to finish
        await Task.WhenAll(consumerTasks);

        Console.WriteLine("\n=== Order Processing Complete ===");
        Console.WriteLine($"Total orders processed: {totalProcessed}");

        return totalProcessed;
    }
}

// ============= USAGE EXAMPLE =============
class ProducerConsumerDemo
{
    static async Task Main()
    {
        Console.WriteLine("=== Producer-Consumer Order Processing Demo ===\n");

        // Create sample orders
        var orders = new List<Order>
        {
            new Order { OrderId = 101, CustomerName = "Alice", Amount = 150.00m },
            new Order { OrderId = 102, CustomerName = "Bob", Amount = 250.50m },
            new Order { OrderId = 103, CustomerName = "Charlie", Amount = 75.25m },
            new Order { OrderId = 104, CustomerName = "Diana", Amount = 320.00m },
            new Order { OrderId = 105, CustomerName = "Eve", Amount = 95.99m },
            new Order { OrderId = 106, CustomerName = "Frank", Amount = 480.00m },
            new Order { OrderId = 107, CustomerName = "Grace", Amount = 210.50m },
            new Order { OrderId = 108, CustomerName = "Henry", Amount = 165.75m }
        };

        // Create and run the system
        var system = new OrderProcessingSystem();
        int processedCount = await system.RunAsync(orders);

        Console.WriteLine($"\nFinal Result: {processedCount}/{orders.Count} orders processed successfully");
    }
}
