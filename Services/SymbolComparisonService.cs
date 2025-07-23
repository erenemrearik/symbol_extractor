using System;
using System.Collections.Generic;
using System.Linq;
using symbol_extractor.Models;
using symbol_extractor.Services;

namespace symbol_extractor.Services;

public class SymbolComparisonService
{
    public (List<SymbolInfo> common, List<SymbolInfo> onlyInApi, List<string> onlyInUser) FindSymbolDifferences(
        IEnumerable<SymbolInfo> apiSymbols,
        IEnumerable<string> userSymbols)
    {
        var normalizedUserSymbols = new HashSet<string>(
            userSymbols.Select(SymbolParser.NormalizeSymbol),
            StringComparer.OrdinalIgnoreCase
        );

        var normalizedApiSymbols = apiSymbols
            .ToDictionary(s => SymbolParser.NormalizeSymbol(s.Symbol), s => s, StringComparer.OrdinalIgnoreCase);

        var commonSymbols = normalizedApiSymbols
            .Where(kvp => normalizedUserSymbols.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        var onlyInApi = normalizedApiSymbols
            .Where(kvp => !normalizedUserSymbols.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        var normalizedApiSymbolKeys = new HashSet<string>(normalizedApiSymbols.Keys, StringComparer.OrdinalIgnoreCase);
        var onlyInUser = normalizedUserSymbols
            .Where(s => !normalizedApiSymbolKeys.Contains(s))
            .ToList();

        return (commonSymbols, onlyInApi, onlyInUser);
    }

    public (List<string> missingInSecond, List<string> missingInFirst) FindMissingSymbols(
        IEnumerable<string> list1,
        IEnumerable<string> list2)
    {
        var set1 = new HashSet<string>(list1.Select(SymbolParser.NormalizeSymbol), StringComparer.OrdinalIgnoreCase);
        var set2 = new HashSet<string>(list2.Select(SymbolParser.NormalizeSymbol), StringComparer.OrdinalIgnoreCase);

        var missingInSecond = set1.Except(set2).ToList();
        var missingInFirst = set2.Except(set1).ToList();

        return (missingInSecond, missingInFirst);
    }
} 