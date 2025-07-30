using System.Collections.Generic;

namespace symbol_extractor.Models;

public record SymbolList(string Name, List<SymbolInfo> Symbols);