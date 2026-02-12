using System;
using System.Collections.Generic;
using System.Linq;

public interface IFinancialInstrument
{
    string Symbol { get; }
    decimal CurrentPrice { get; }
    InstrumentType Type { get; }
}

public enum InstrumentType { Stock, Bond, Option, Future }
public enum Trend { Upward, Downward, Sideways }

// 1. Generic portfolio
public class Portfolio<T> where T : IFinancialInstrument
{
    private Dictionary<T, int> _holdings = new(); // Instrument -> Quantity

    // Buy instrument
    public void Buy(T instrument, int quantity, decimal price)
    {
        if (instrument == null) throw new ArgumentNullException(nameof(instrument));
        if (quantity <= 0) throw new ArgumentException("Quantity must be > 0.");
        if (price <= 0) throw new ArgumentException("Price must be > 0.");

        if (_holdings.ContainsKey(instrument))
            _holdings[instrument] += quantity;
        else
            _holdings[instrument] = quantity;

        Console.WriteLine($" BUY: {instrument.Symbol} x{quantity} @ {price}");
    }

    // Sell instrument
    public decimal? Sell(T instrument, int quantity, decimal currentPrice)
    {
        if (instrument == null) throw new ArgumentNullException(nameof(instrument));
        if (quantity <= 0) throw new ArgumentException("Quantity must be > 0.");
        if (currentPrice <= 0) throw new ArgumentException("Price must be > 0.");

        if (!_holdings.TryGetValue(instrument, out var heldQty))
        {
            Console.WriteLine($" SELL failed: {instrument.Symbol} not in portfolio.");
            return null;
        }

        if (heldQty < quantity)
        {
            Console.WriteLine($" SELL failed: Not enough quantity for {instrument.Symbol}.");
            return null;
        }

        _holdings[instrument] = heldQty - quantity;

        if (_holdings[instrument] == 0)
            _holdings.Remove(instrument);

        var proceeds = quantity * currentPrice;
        Console.WriteLine($"✅ SELL: {instrument.Symbol} x{quantity} @ {currentPrice} => {proceeds}");
        return proceeds;
    }

    // Calculate total value
    public decimal CalculateTotalValue()
    {
        return _holdings.Sum(h => h.Value * h.Key.CurrentPrice);
    }

    // Get top performing instrument
    public (T instrument, decimal returnPercentage)? GetTopPerformer(
        Dictionary<T, decimal> purchasePrices)
    {
        if (purchasePrices == null) throw new ArgumentNullException(nameof(purchasePrices));

        var performers = new List<(T instrument, decimal ret)>();

        foreach (var holding in _holdings)
        {
            var inst = holding.Key;

            if (!purchasePrices.TryGetValue(inst, out var buyPrice) || buyPrice <= 0)
                continue;

            var ret = ((inst.CurrentPrice - buyPrice) / buyPrice) * 100m;
            performers.Add((inst, ret));
        }

        if (!performers.Any()) return null;

        var top = performers.OrderByDescending(p => p.ret).First();
        return (top.instrument, top.ret);
    }

    public IReadOnlyDictionary<T, int> Holdings => _holdings;

    // Helper: return instruments currently held
    public IEnumerable<T> GetInstruments() => _holdings.Keys.ToList();
}

// 2. Specialized instruments
public class Stock : IFinancialInstrument
{
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public InstrumentType Type => InstrumentType.Stock;
    public string CompanyName { get; set; } = string.Empty;
    public decimal DividendYield { get; set; }
}

public class Bond : IFinancialInstrument
{
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public InstrumentType Type => InstrumentType.Bond;
    public DateTime MaturityDate { get; set; }
    public decimal CouponRate { get; set; }
}

// 3. Generic trading strategy
public class TradingStrategy<T> where T : IFinancialInstrument
{
    private readonly List<T> _marketData = new();

    // Add/Update market data list
    public void SetMarketData(IEnumerable<T> instruments)
    {
        _marketData.Clear();
        _marketData.AddRange(instruments ?? throw new ArgumentNullException(nameof(instruments)));
    }

    // Execute strategy on portfolio
    public void Execute(
        Portfolio<T> portfolio,
        Func<T, bool> buyCondition,
        Func<T, bool> sellCondition)
    {
        if (portfolio == null) throw new ArgumentNullException(nameof(portfolio));
        if (buyCondition == null) throw new ArgumentNullException(nameof(buyCondition));
        if (sellCondition == null) throw new ArgumentNullException(nameof(sellCondition));

        foreach (var inst in _marketData)
        {
            try
            {
                // If sell condition true and instrument held => sell 1 unit (demo)
                if (sellCondition(inst) && portfolio.Holdings.ContainsKey(inst))
                {
                    portfolio.Sell(inst, 1, inst.CurrentPrice);
                    continue;
                }

                // If buy condition true => buy 1 unit (demo)
                if (buyCondition(inst))
                {
                    portfolio.Buy(inst, 1, inst.CurrentPrice);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Strategy error for {inst.Symbol}: {ex.Message}");
            }
        }
    }

    // Calculate risk metrics
    public Dictionary<string, decimal> CalculateRiskMetrics(IEnumerable<T> instruments)
    {
        if (instruments == null) throw new ArgumentNullException(nameof(instruments));

        var prices = instruments.Select(i => i.CurrentPrice).Where(p => p > 0).ToList();
        if (prices.Count < 2)
        {
            return new Dictionary<string, decimal>
            {
                { "Volatility", 0m },
                { "Beta", 1m },
                { "SharpeRatio", 0m }
            };
        }

        // simple returns
        var returns = new List<decimal>();
        for (int i = 1; i < prices.Count; i++)
        {
            var r = (prices[i] - prices[i - 1]) / prices[i - 1];
            returns.Add(r);
        }

        var avgReturn = returns.Average();
        var variance = returns.Select(r => (r - avgReturn) * (r - avgReturn)).Average();
        var volatility = (decimal)Math.Sqrt((double)variance);

        // Placeholder beta: use relative volatility mapping (demo)
        var beta = 1m + volatility; // not real beta, but shows metric generation

        // Sharpe: (avgReturn - riskFree)/volatility, riskFree = 0 for demo
        var sharpe = volatility == 0 ? 0m : (avgReturn / volatility);

        return new Dictionary<string, decimal>
        {
            { "Volatility", volatility },
            { "Beta", beta },
            { "SharpeRatio", sharpe }
        };
    }
}

// 4. Price history with generics
public class PriceHistory<T> where T : IFinancialInstrument
{
    private Dictionary<T, List<(DateTime, decimal)>> _history = new();

    // Add price point
    public void AddPrice(T instrument, DateTime timestamp, decimal price)
    {
        if (instrument == null) throw new ArgumentNullException(nameof(instrument));
        if (price <= 0) throw new ArgumentException("Price must be > 0.");

        if (!_history.ContainsKey(instrument))
            _history[instrument] = new List<(DateTime, decimal)>();

        _history[instrument].Add((timestamp, price));
    }

    // Get moving average
    public decimal? GetMovingAverage(T instrument, int days)
    {
        if (instrument == null) throw new ArgumentNullException(nameof(instrument));
        if (days <= 0) throw new ArgumentException("Days must be > 0.");

        if (!_history.TryGetValue(instrument, out var list) || list.Count == 0)
            return null;

        var cutoff = DateTime.Now.AddDays(-days);

        var recent = list
            .Where(x => x.Item1 >= cutoff)
            .Select(x => x.Item2)
            .ToList();

        if (!recent.Any()) return null;

        return recent.Average();
    }

    // Detect trends
    public Trend DetectTrend(T instrument, int period)
    {
        if (instrument == null) throw new ArgumentNullException(nameof(instrument));
        if (period <= 1) throw new ArgumentException("Period must be > 1.");

        if (!_history.TryGetValue(instrument, out var list) || list.Count < period)
            return Trend.Sideways;

        var last = list
            .OrderBy(x => x.Item1)
            .TakeLast(period)
            .Select(x => x.Item2)
            .ToList();

        var first = last.First();
        var lastPrice = last.Last();

        // Threshold (1%) to classify sideways
        var change = (lastPrice - first) / first;

        if (change > 0.01m) return Trend.Upward;
        if (change < -0.01m) return Trend.Downward;
        return Trend.Sideways;
    }
}

// 5. TEST SCENARIO: Trading simulation
class Program
{
    static void Main()
    {
        // Create instruments
        var s1 = new Stock { Symbol = "AAPL", CompanyName = "Apple", CurrentPrice = 200m, DividendYield = 0.5m };
        var s2 = new Stock { Symbol = "TSLA", CompanyName = "Tesla", CurrentPrice = 180m, DividendYield = 0m };
        var b1 = new Bond { Symbol = "GOVT10Y", CurrentPrice = 95m, CouponRate = 6.5m, MaturityDate = DateTime.Now.AddYears(10) };

        // Portfolio (using IFinancialInstrument so it can hold mixed types)
        var portfolio = new Portfolio<IFinancialInstrument>();

        // Purchase price record (for performance)
        var purchasePrices = new Dictionary<IFinancialInstrument, decimal>();

        // Buy initial positions
        portfolio.Buy(s1, 2, s1.CurrentPrice);
        purchasePrices[s1] = s1.CurrentPrice;

        portfolio.Buy(s2, 1, s2.CurrentPrice);
        purchasePrices[s2] = s2.CurrentPrice;

        portfolio.Buy(b1, 5, b1.CurrentPrice);
        purchasePrices[b1] = b1.CurrentPrice;

        Console.WriteLine($"\n Portfolio Value: {portfolio.CalculateTotalValue()}");

        // Price history tracking
        var history = new PriceHistory<IFinancialInstrument>();

        // Add some historical prices (fake simulation)
        for (int i = 5; i >= 1; i--)
        {
            history.AddPrice(s1, DateTime.Now.AddDays(-i), 195m + i); // upward-ish
            history.AddPrice(s2, DateTime.Now.AddDays(-i), 185m - i); // downward-ish
            history.AddPrice(b1, DateTime.Now.AddDays(-i), 95m);      // flat
        }

        Console.WriteLine($"\nAAPL 3-day MA: {history.GetMovingAverage(s1, 3)}");
        Console.WriteLine($"TSLA 3-day MA: {history.GetMovingAverage(s2, 3)}");
        Console.WriteLine($"GOVT10Y 3-day MA: {history.GetMovingAverage(b1, 3)}");

        Console.WriteLine($"\nTrend AAPL: {history.DetectTrend(s1, 5)}");
        Console.WriteLine($"Trend TSLA: {history.DetectTrend(s2, 5)}");
        Console.WriteLine($"Trend GOVT10Y: {history.DetectTrend(b1, 5)}");

        // Update current prices (market moved)
        s1.CurrentPrice = 210m; // up
        s2.CurrentPrice = 160m; // down
        b1.CurrentPrice = 96m;  // slightly up

        // Strategy
        var strategy = new TradingStrategy<IFinancialInstrument>();
        strategy.SetMarketData(new List<IFinancialInstrument> { s1, s2, b1 });

        // Buy condition: price < 170 (cheap)
        // Sell condition: price > 205 (profit)
        strategy.Execute(
            portfolio,
            buyCondition: inst => inst.CurrentPrice < 170m,
            sellCondition: inst => inst.CurrentPrice > 205m
        );

        Console.WriteLine($"\ Portfolio Value After Strategy: {portfolio.CalculateTotalValue()}");

        // Risk metrics (demo)
        var risk = strategy.CalculateRiskMetrics(new[] { s1, s2, b1 });
        Console.WriteLine("\n--- Risk Metrics (Demo) ---");
        foreach (var kv in risk)
            Console.WriteLine($"{kv.Key}: {kv.Value}");

        // Top performer
        var top = portfolio.GetTopPerformer(purchasePrices);
        Console.WriteLine("\n--- Top Performer ---");
        if (top.HasValue)
            Console.WriteLine($"{top.Value.instrument.Symbol} => Return: {top.Value.returnPercentage:F2}%");
        else
            Console.WriteLine("No performance data.");

        // Show holdings
        Console.WriteLine("\n--- Holdings ---");
        foreach (var h in portfolio.Holdings)
            Console.WriteLine($"{h.Key.Symbol} ({h.Key.Type}) x{h.Value} @ {h.Key.CurrentPrice}");
    }
}
