using System.Collections.Generic;
using System.Linq;
using symbol_extractor.Models;

namespace symbol_extractor.Services;

public class StatisticsService
{
    /// <summary>
    /// Calculates the count of symbols for each quote currency.
    /// </summary>
    /// <param name="symbols">A list of symbols to analyze.</param>
    /// <returns>A dictionary where the key is the quote currency and the value is the count.</returns>
    public Dictionary<string, int> GetQuoteCurrencyCounts(IEnumerable<SymbolInfo> symbols)
    {
        return symbols
            .Where(s => !string.IsNullOrEmpty(s.Quote))
            .GroupBy(s => s.Quote)
            .ToDictionary(g => g.Key, g => g.Count())
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
} 