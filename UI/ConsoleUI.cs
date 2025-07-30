using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using symbol_extractor.Data;
using symbol_extractor.Models;
using symbol_extractor.Services;
using Spectre.Console;

namespace symbol_extractor.UI;

public static class ConsoleUi
{
    public static string GetDesktopPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    }

    public static void DisplayWelcomeMessage()
    {
        AnsiConsole.Write(
            new FigletText("Symbol Extractor")
                .Centered()
                .Color(Color.Orange1));
        
        AnsiConsole.MarkupLine("[grey]A tool for fetching, comparing, and analyzing crypto symbols.[/]");
        AnsiConsole.WriteLine();
    }

    public static AppMode GetAppMode()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<AppMode>()
                .Title("Select a [green]mode[/]:")
                .PageSize(6)
                .AddChoices(new[]
                {
                    AppMode.SymbolExtract,
                    AppMode.Compare,
                    AppMode.MultiListCompare,
                    AppMode.Match,
                    AppMode.ManageCurrencies
                }));
    }

    public static void DisplayCurrencies(List<string> currencies)
    {
        var table = new Table().Title("Known Currencies").Centered();
        table.AddColumn(new TableColumn("[yellow]Currency[/]").Centered());

        foreach (var currency in currencies.OrderBy(c => c))
        {
            table.AddRow(currency);
        }
        
        AnsiConsole.Write(table);
    }

    public static string GetCurrencyToAdd()
    {
        return AnsiConsole.Ask<string>("Enter the currency to [green]add[/]:");
    }

    public static string GetCurrencyToRemove()
    {
        return AnsiConsole.Ask<string>("Enter the currency to [red]remove[/]:");
    }

    public static char GetCurrencyManagementAction()
    {
        AnsiConsole.WriteLine();
        return AnsiConsole.Prompt(
            new TextPrompt<char>("[grey](V)iew, (A)dd, (R)emove, or (B)ack to main menu[/]")
                .PromptStyle("cyan")
                .DefaultValue(' ')
                .ShowDefaultValue(false)
                .ShowChoices(false)
                .InvalidChoiceMessage("[red]Invalid selection.[/]"));
    }

    public static string GetInputJson()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select source type:")
                .AddChoices(new[] { "JSON File", "API URL" }));

        if (choice == "JSON File")
        {
            var path = AnsiConsole.Ask<string>("Enter the path to the [yellow]JSON file[/]:");
            path = CleanPath(path);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The specified file was not found.", path);
            }
            return File.ReadAllText(path);
        }

        return AnsiConsole.Ask<string>("Enter the [cyan]API URL[/]:");
    }

    public static string GetApiUrl()
    {
        return AnsiConsole.Ask<string>("Enter the [cyan]API URL[/]:");
    }

    public static IEnumerable<SymbolInfo> GetUserSymbols(SymbolParser symbolParser)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("How would you like to provide the symbols for comparison?")
                .AddChoices(new[]
                {
                    "From an Excel file",
                    "Manual entry",
                    "Use the built-in KTR list"
                }));

        switch (choice)
        {
            case "From an Excel file":
                var filePath = AnsiConsole.Ask<string>("Enter the path to the [green]Excel file[/]:");
                filePath = CleanPath(filePath);
                if (!File.Exists(filePath))
                {
                    AnsiConsole.MarkupLine("[red]File not found![/]");
                    return new List<SymbolInfo>();
                }
                var fileService = new FileService();
                return fileService.ReadSymbolsFromExcel(filePath)
                    .Select(s =>
                    {
                        var (b, q) = symbolParser.ParseSymbol(s);
                        return new SymbolInfo(s, b, q);
                    });

            case "Manual entry":
                return ReadSymbolsFromConsole(
                        "[grey]Enter symbols (comma-separated or one per line). End with an empty line:[/]")
                    .Select(s =>
                    {
                        var (b, q) = symbolParser.ParseSymbol(s);
                        return new SymbolInfo(s, b, q);
                    });

            case "Use the built-in KTR list":
                AnsiConsole.MarkupLine("[green]Using the built-in KTR symbol list.[/]");
                return PredefinedLists.KtrSymbols.Select(kvp =>
                {
                    var (b, q) = symbolParser.ParseSymbol(kvp.Key);
                    return new SymbolInfo(kvp.Key, b, q, kvp.Value);
                });
        }

        return new List<SymbolInfo>();
    }

    public static HashSet<string> ReadSymbolsFromConsole(string prompt)
    {
        AnsiConsole.MarkupLine(prompt);
        var symbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            if (line.Contains(','))
            {
                foreach (var part in line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    symbols.Add(part.Trim());
                }
            }
            else
            {
                symbols.Add(line.Trim());
            }
        }
        return symbols;
    }

    public static OutputType GetOutputType()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select output format:")
                .AddChoices(new[] { "Excel (.xlsx)", "TXT" }));
        
        return choice == "Excel (.xlsx)" ? OutputType.Excel : OutputType.Txt;
    }

    public static string GetOutputFileName()
    {
        return AnsiConsole.Ask<string>("Enter the desired output file name (without extension, e.g., [yellow]APISymbols[/]):");
    }
    
    public static void DisplayResults(string message, IEnumerable<string> results)
    {
        var table = new Table().Title(message).Centered();
        table.AddColumn(new TableColumn("[yellow]Symbol[/]").Centered());

        if (results.Any())
        {
            foreach (var item in results)
            {
                table.AddRow(item);
            }
        }
        else
        {
            table.AddRow("[grey]NONE[/]");
        }
        AnsiConsole.Write(table);
    }
    
    public static void DisplayQuoteStatistics(Dictionary<string, int> stats)
    {
        var table = new Table()
            .Title("[yellow]Top 10 Quote Currencies[/]")
            .Centered();
            
        table.AddColumn("Quote Currency");
        table.AddColumn(new TableColumn("Symbol Count").Centered());

        foreach (var stat in stats.Take(10))
        {
            table.AddRow($"[bold]{stat.Key}[/]", $"[green]{stat.Value}[/]");
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayMessage(string message, bool isError = false)
    {
        var color = isError ? "red" : "green";
        AnsiConsole.MarkupLine($"[{color}]{message}[/]");
    }
    
    private static string CleanPath(string path)
    {
        return path?.Trim().Trim('"') ?? string.Empty;
    }

    public static string GetListName(int listNumber)
    {
        return AnsiConsole.Ask<string>($"[cyan]Enter name for List {listNumber}[/] (e.g., 'API Symbols', 'User List', 'Exchange A'):");
    }

    public static SymbolList GetSymbolList(SymbolParser symbolParser, int listNumber)
    {
        var listName = GetListName(listNumber);
        AnsiConsole.MarkupLine($"[green]Collecting symbols for '{listName}'...[/]");
        
        var symbols = GetUserSymbols(symbolParser).ToList();
        
        if (!symbols.Any())
        {
            AnsiConsole.MarkupLine("[red]No symbols were provided for this list.[/]");
            return new SymbolList(listName, new List<SymbolInfo>());
        }

        AnsiConsole.MarkupLine($"[green]Added {symbols.Count} symbols to '{listName}'.[/]");
        return new SymbolList(listName, symbols);
    }

    public static bool ShouldContinueAddingLists()
    {
        AnsiConsole.WriteLine();
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Do you want to add another symbol list?")
                .AddChoices(new[] { "Yes, add another list", "No, start comparison" }));

        return choice == "Yes, add another list";
    }

    public static void DisplayMultiListSummary(List<SymbolList> symbolLists, List<SymbolInfo> commonSymbols)
    {
        var table = new Table().Title("Multi-List Comparison Summary").Centered();
        table.AddColumn("List Name");
        table.AddColumn("Symbol Count");
        table.AddColumn("Status");

        foreach (var list in symbolLists)
        {
            table.AddRow(
                $"[bold]{list.Name}[/]", 
                $"[green]{list.Symbols.Count}[/]",
                list.Symbols.Count > 0 ? "[green]✓[/]" : "[red]✗[/]"
            );
        }

        AnsiConsole.Write(table);
        
        if (commonSymbols.Any())
        {
            AnsiConsole.MarkupLine($"\n[bold green]Common symbols found: {commonSymbols.Count}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("\n[red]No common symbols found across all lists.[/]");
        }
    }

    public static void DisplayValidationSummary(
        List<SymbolParseError> parseErrors,
        List<DuplicateSymbol> duplicateSymbols,
        List<SymbolParseInconsistency> parseInconsistencies)
    {
        var table = new Table().Title("Validation Summary").Centered();
        table.AddColumn("Issue Type");
        table.AddColumn("Count");
        table.AddColumn("Status");

        table.AddRow(
            "Parse Errors", 
            $"[red]{parseErrors.Count}[/]",
            parseErrors.Count > 0 ? "[red]⚠[/]" : "[green]✓[/]"
        );

        table.AddRow(
            "Duplicate Symbols", 
            $"[yellow]{duplicateSymbols.Count}[/]",
            duplicateSymbols.Count > 0 ? "[yellow]⚠[/]" : "[green]✓[/]"
        );

        table.AddRow(
            "Parse Inconsistencies", 
            $"[orange1]{parseInconsistencies.Count}[/]",
            parseInconsistencies.Count > 0 ? "[orange1]⚠[/]" : "[green]✓[/]"
        );

        AnsiConsole.Write(table);
    }

    public static void DisplayParseErrors(List<SymbolParseError> parseErrors)
    {
        if (!parseErrors.Any()) return;

        var table = new Table().Title("Parse Errors").Centered();
        table.AddColumn("Symbol");
        table.AddColumn("List");
        table.AddColumn("Error");
        table.AddColumn("Stored Base/Quote");
        table.AddColumn("Parsed Base/Quote");

        foreach (var error in parseErrors.Take(10)) // Show first 10 errors
        {
            table.AddRow(
                $"[red]{error.Symbol}[/]",
                error.ListName,
                error.ErrorMessage,
                $"{error.StoredBase}/{error.StoredQuote}",
                $"{error.ParsedBase}/{error.ParsedQuote}"
            );
        }

        AnsiConsole.Write(table);
        
        if (parseErrors.Count > 10)
        {
            AnsiConsole.MarkupLine($"[grey]... and {parseErrors.Count - 10} more errors (see Excel report for details)[/]");
        }
    }

    public static void DisplayDuplicateSymbols(List<DuplicateSymbol> duplicateSymbols)
    {
        if (!duplicateSymbols.Any()) return;

        var table = new Table().Title("Duplicate Symbols").Centered();
        table.AddColumn("Normalized Symbol");
        table.AddColumn("Occurrences");
        table.AddColumn("Lists");

        foreach (var duplicate in duplicateSymbols.Take(10)) // Show first 10 duplicates
        {
            var listNames = string.Join(", ", duplicate.Occurrences.Select(o => o.ListName).Distinct());
            table.AddRow(
                $"[yellow]{duplicate.NormalizedSymbol}[/]",
                $"[bold]{duplicate.Occurrences.Count}[/]",
                listNames
            );
        }

        AnsiConsole.Write(table);
        
        if (duplicateSymbols.Count > 10)
        {
            AnsiConsole.MarkupLine($"[grey]... and {duplicateSymbols.Count - 10} more duplicates (see Excel report for details)[/]");
        }
    }

    public static void DisplayParseInconsistencies(List<SymbolParseInconsistency> parseInconsistencies)
    {
        if (!parseInconsistencies.Any()) return;

        var table = new Table().Title("Parse Inconsistencies").Centered();
        table.AddColumn("Normalized Symbol");
        table.AddColumn("Lists");
        table.AddColumn("Different Parsings");

        foreach (var inconsistency in parseInconsistencies.Take(10)) // Show first 10 inconsistencies
        {
            var listNames = string.Join(", ", inconsistency.ParseInfos.Select(p => p.ListName).Distinct());
            var uniqueParsings = inconsistency.ParseInfos
                .Select(p => $"{p.ParsedBase}/{p.ParsedQuote}")
                .Distinct()
                .ToList();
            var parsingText = string.Join(", ", uniqueParsings);
            
            table.AddRow(
                $"[orange1]{inconsistency.NormalizedSymbol}[/]",
                listNames,
                parsingText
            );
        }

        AnsiConsole.Write(table);
        
        if (parseInconsistencies.Count > 10)
        {
            AnsiConsole.MarkupLine($"[grey]... and {parseInconsistencies.Count - 10} more inconsistencies (see Excel report for details)[/]");
        }
    }
}

public enum AppMode { SymbolExtract = 1, Compare = 2, MultiListCompare = 3, Match = 4, ManageCurrencies = 5 }
public enum OutputType { Txt, Excel } 