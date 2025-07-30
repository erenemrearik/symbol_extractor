using System;
using System.Collections.Generic;
using System.Linq;
using symbol_extractor.Models;

namespace symbol_extractor.Services;

public class MultiListComparisonService
{
    private readonly SymbolParser _symbolParser;

    public MultiListComparisonService(SymbolParser symbolParser)
    {
        _symbolParser = symbolParser;
    }

    public List<SymbolInfo> FindCommonSymbols(List<SymbolList> symbolLists)
    {
        if (symbolLists == null || symbolLists.Count < 2)
            return new List<SymbolInfo>();

        // Normalize all symbols across all lists
        var normalizedLists = symbolLists.Select(list => new
        {
            ListName = list.Name,
            NormalizedSymbols = list.Symbols.ToDictionary(
                s => SymbolParser.NormalizeSymbol(s.Symbol), 
                s => s, 
                StringComparer.OrdinalIgnoreCase)
        }).ToList();

        // Find symbols that exist in all lists
        var firstListKeys = new HashSet<string>(normalizedLists[0].NormalizedSymbols.Keys, StringComparer.OrdinalIgnoreCase);
        
        foreach (var list in normalizedLists.Skip(1))
        {
            firstListKeys.IntersectWith(list.NormalizedSymbols.Keys);
        }

        // Return the common symbols (using the first list's symbol info as reference)
        return firstListKeys
            .Select(key => normalizedLists[0].NormalizedSymbols[key])
            .ToList();
    }

    public Dictionary<string, List<SymbolInfo>> GetUniqueSymbolsPerList(List<SymbolList> symbolLists)
    {
        var result = new Dictionary<string, List<SymbolInfo>>();
        
        if (symbolLists == null || symbolLists.Count < 2)
            return result;

        // Normalize all symbols across all lists
        var normalizedLists = symbolLists.Select(list => new
        {
            ListName = list.Name,
            NormalizedSymbols = list.Symbols.ToDictionary(
                s => SymbolParser.NormalizeSymbol(s.Symbol), 
                s => s, 
                StringComparer.OrdinalIgnoreCase)
        }).ToList();

        // For each list, find symbols that are unique to that list
        for (int i = 0; i < normalizedLists.Count; i++)
        {
            var currentList = normalizedLists[i];
            var uniqueSymbols = new List<SymbolInfo>();
            
            foreach (var symbol in currentList.NormalizedSymbols)
            {
                bool isUnique = true;
                
                // Check if this symbol exists in any other list
                for (int j = 0; j < normalizedLists.Count; j++)
                {
                    if (i != j && normalizedLists[j].NormalizedSymbols.ContainsKey(symbol.Key))
                    {
                        isUnique = false;
                        break;
                    }
                }
                
                if (isUnique)
                {
                    uniqueSymbols.Add(symbol.Value);
                }
            }
            
            result[currentList.ListName] = uniqueSymbols;
        }

        return result;
    }

    public Dictionary<string, int> GetSymbolCounts(List<SymbolList> symbolLists)
    {
        return symbolLists?.ToDictionary(
            list => list.Name, 
            list => list.Symbols.Count) ?? new Dictionary<string, int>();
    }
}