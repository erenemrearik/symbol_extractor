using System;
using System.Collections.Generic;
using System.Linq;
using symbol_extractor.Models;

namespace symbol_extractor.Services;

public class SymbolValidationService
{
    private readonly SymbolParser _symbolParser;

    public SymbolValidationService(SymbolParser symbolParser)
    {
        _symbolParser = symbolParser;
    }

    /// <summary>
    /// Validates symbol parsing and returns parsing errors
    /// </summary>
    public List<SymbolParseError> ValidateSymbolParsing(List<SymbolList> symbolLists)
    {
        var errors = new List<SymbolParseError>();

        foreach (var list in symbolLists)
        {
            foreach (var symbol in list.Symbols)
            {
                var (parsedBase, parsedQuote) = _symbolParser.ParseSymbol(symbol.Symbol);
                
                // Check if parsing failed (empty quote)
                if (string.IsNullOrEmpty(parsedQuote))
                {
                    errors.Add(new SymbolParseError(
                        symbol.Symbol,
                        list.Name,
                        "Could not parse quote currency",
                        symbol.Base,
                        symbol.Quote,
                        parsedBase,
                        parsedQuote
                    ));
                }
                // Check if parsed values don't match stored values
                else if (!string.Equals(symbol.Base, parsedBase, StringComparison.OrdinalIgnoreCase) ||
                         !string.Equals(symbol.Quote, parsedQuote, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new SymbolParseError(
                        symbol.Symbol,
                        list.Name,
                        "Parsed values don't match stored values",
                        symbol.Base,
                        symbol.Quote,
                        parsedBase,
                        parsedQuote
                    ));
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Finds duplicate symbols across all lists
    /// </summary>
    public List<DuplicateSymbol> FindDuplicateSymbols(List<SymbolList> symbolLists)
    {
        var duplicates = new List<DuplicateSymbol>();
        var symbolOccurrences = new Dictionary<string, List<SymbolOccurrence>>(StringComparer.OrdinalIgnoreCase);

        // Collect all symbol occurrences
        foreach (var list in symbolLists)
        {
            foreach (var symbol in list.Symbols)
            {
                var normalizedSymbol = SymbolParser.NormalizeSymbol(symbol.Symbol);
                
                if (!symbolOccurrences.ContainsKey(normalizedSymbol))
                {
                    symbolOccurrences[normalizedSymbol] = new List<SymbolOccurrence>();
                }

                symbolOccurrences[normalizedSymbol].Add(new SymbolOccurrence(
                    symbol.Symbol,
                    symbol.Base,
                    symbol.Quote,
                    list.Name
                ));
            }
        }

        // Find symbols that appear in multiple lists
        foreach (var kvp in symbolOccurrences)
        {
            if (kvp.Value.Count > 1)
            {
                duplicates.Add(new DuplicateSymbol(
                    kvp.Key,
                    kvp.Value
                ));
            }
        }

        return duplicates.OrderByDescending(d => d.Occurrences.Count).ToList();
    }

    /// <summary>
    /// Finds symbols with parsing inconsistencies (same symbol parsed differently)
    /// </summary>
    public List<SymbolParseInconsistency> FindParseInconsistencies(List<SymbolList> symbolLists)
    {
        var inconsistencies = new List<SymbolParseInconsistency>();
        var symbolParsings = new Dictionary<string, List<SymbolParseInfo>>(StringComparer.OrdinalIgnoreCase);

        // Collect all symbol parsing results
        foreach (var list in symbolLists)
        {
            foreach (var symbol in list.Symbols)
            {
                var normalizedSymbol = SymbolParser.NormalizeSymbol(symbol.Symbol);
                var (parsedBase, parsedQuote) = _symbolParser.ParseSymbol(symbol.Symbol);
                
                if (!symbolParsings.ContainsKey(normalizedSymbol))
                {
                    symbolParsings[normalizedSymbol] = new List<SymbolParseInfo>();
                }

                symbolParsings[normalizedSymbol].Add(new SymbolParseInfo(
                    symbol.Symbol,
                    symbol.Base,
                    symbol.Quote,
                    parsedBase,
                    parsedQuote,
                    list.Name
                ));
            }
        }

        // Find symbols with different parsing results
        foreach (var kvp in symbolParsings)
        {
            if (kvp.Value.Count > 1)
            {
                var firstParse = kvp.Value[0];
                var hasInconsistency = kvp.Value.Any(p => 
                    !string.Equals(p.ParsedBase, firstParse.ParsedBase, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(p.ParsedQuote, firstParse.ParsedQuote, StringComparison.OrdinalIgnoreCase));

                if (hasInconsistency)
                {
                    inconsistencies.Add(new SymbolParseInconsistency(
                        kvp.Key,
                        kvp.Value
                    ));
                }
            }
        }

        return inconsistencies.OrderByDescending(i => i.ParseInfos.Count).ToList();
    }
}

public record SymbolParseError(
    string Symbol,
    string ListName,
    string ErrorMessage,
    string StoredBase,
    string StoredQuote,
    string ParsedBase,
    string ParsedQuote
);

public record SymbolOccurrence(
    string Symbol,
    string Base,
    string Quote,
    string ListName
);

public record DuplicateSymbol(
    string NormalizedSymbol,
    List<SymbolOccurrence> Occurrences
);

public record SymbolParseInfo(
    string Symbol,
    string StoredBase,
    string StoredQuote,
    string ParsedBase,
    string ParsedQuote,
    string ListName
);

public record SymbolParseInconsistency(
    string NormalizedSymbol,
    List<SymbolParseInfo> ParseInfos
);