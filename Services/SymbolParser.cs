using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace symbol_extractor.Services;

public class SymbolParser
{
    private const string CurrenciesFilePath = "currencies.json";
    private readonly List<string> _knownCurrencies;

    public SymbolParser()
    {
        _knownCurrencies = LoadCurrenciesFromFile();
    }

    private List<string> LoadCurrenciesFromFile()
    {
        if (!File.Exists(CurrenciesFilePath))
        {
            // Return a default list if the file doesn't exist
            return new List<string> { "USDT", "BTC", "ETH", "USD" };
        }
        var json = File.ReadAllText(CurrenciesFilePath);
        return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
    }

    private void SaveCurrenciesToFile()
    {
        var json = JsonConvert.SerializeObject(_knownCurrencies, Formatting.Indented);
        File.WriteAllText(CurrenciesFilePath, json);
    }

    public List<string> GetKnownCurrencies()
    {
        return new List<string>(_knownCurrencies);
    }

    public bool AddCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) return false;

        currency = currency.Trim().ToUpperInvariant();
        if (!_knownCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase))
        {
            _knownCurrencies.Add(currency);
            _knownCurrencies.Sort((a, b) => b.Length.CompareTo(a.Length));
            SaveCurrenciesToFile();
            return true;
        }
        return false;
    }

    public bool RemoveCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) return false;

        currency = currency.Trim().ToUpperInvariant();
        var itemToRemove = _knownCurrencies.FirstOrDefault(c => c.Equals(currency, StringComparison.OrdinalIgnoreCase));
        
        if (itemToRemove != null)
        {
            _knownCurrencies.Remove(itemToRemove);
            SaveCurrenciesToFile();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Adds a new currency to the list of known currencies if it's not already present.
    /// </summary>
    private void AddNewCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) return;

        currency = currency.Trim().ToUpperInvariant();
        if (!_knownCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase))
        {
            _knownCurrencies.Add(currency);
            _knownCurrencies.Sort((a, b) => b.Length.CompareTo(a.Length)); // Keep sorted by length for better parsing
            SaveCurrenciesToFile();
            Console.WriteLine($"Info: New currency '{currency}' has been detected and saved.");
        }
    }

    /// <summary>
    /// Parses a trading symbol (e.g., "BTCUSDT", "ETH-BTC") into its base and quote components.
    /// It also handles and removes suffixes like "/P".
    /// </summary>
    /// <param name="symbol">The symbol string to parse.</param>
    /// <returns>A tuple containing the base and quote currency.</returns>
    public (string Base, string Quote) ParseSymbol(string symbol)
    {
        // First, remove known suffixes
        if (symbol.EndsWith("/P", StringComparison.OrdinalIgnoreCase))
        {
            symbol = symbol.Substring(0, symbol.Length - 2);
        }

        // Handle symbols with explicit separators like "-", "/", "_", ":"
        var separators = new[] { "-", "/", "_", ":" };
        foreach (var sep in separators)
        {
            if (symbol.Contains(sep))
            {
                var parts = symbol.Split(new[] { sep }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    AddNewCurrency(parts[0]);
                    AddNewCurrency(parts[1]);
                    return (parts[0], parts[1]);
                }
            }
        }

        // If no separator, try to match against known quote currencies
        foreach (var quote in _knownCurrencies) // No need to sort here if sorted on add
        {
            if (symbol.EndsWith(quote, StringComparison.OrdinalIgnoreCase) && symbol.Length > quote.Length)
            {
                var baseCurrency = symbol.Substring(0, symbol.Length - quote.Length);
                if (!string.IsNullOrEmpty(baseCurrency))
                {
                    AddNewCurrency(baseCurrency);
                    return (baseCurrency, quote);
                }
            }
        }

        // Fallback for common but potentially unlisted patterns
        string[] commonEndings = { "USDT", "BTC", "ETH", "USD", "BUSD" };
        foreach (var ending in commonEndings)
        {
            if (symbol.Length > ending.Length && symbol.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
            {
                var baseCurrency = symbol.Substring(0, symbol.Length - ending.Length);
                if (baseCurrency.Length >= 2) // Ensure base currency is at least 2 chars
                {
                    AddNewCurrency(baseCurrency);
                    return (baseCurrency, ending);
                }
            }
        }

        // If no quote currency is found, return the symbol as base and an empty quote
        return (symbol, string.Empty);
    }

    /// <summary>
    /// Normalizes a symbol string for consistent comparison by removing separators and converting to uppercase.
    /// Also removes suffixes like "/P".
    /// </summary>
    public static string NormalizeSymbol(string symbol)
    {
        if (string.IsNullOrEmpty(symbol)) return string.Empty;

        // Remove suffix
        if (symbol.EndsWith("/P", StringComparison.OrdinalIgnoreCase))
        {
            symbol = symbol.Substring(0, symbol.Length - 2);
        }

        // Remove separators and convert to uppercase
        return symbol.Replace("-", "").Replace("/", "").Replace("_", "").Replace(" ", "").ToUpperInvariant();
    }
} 