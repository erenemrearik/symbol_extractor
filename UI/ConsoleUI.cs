using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using symbol_extractor.Data;
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
                .PageSize(5)
                .AddChoices(new[]
                {
                    AppMode.SymbolExtract,
                    AppMode.Compare,
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

    public static HashSet<string> GetUserSymbolList()
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
                    return new HashSet<string>();
                }
                var fileService = new FileService();
                return new HashSet<string>(fileService.ReadSymbolsFromExcel(filePath), StringComparer.OrdinalIgnoreCase);
            
            case "Manual entry":
                return ReadSymbolsFromConsole("[grey]Enter symbols (comma-separated or one per line). End with an empty line:[/]");
            
            case "Use the built-in KTR list":
                AnsiConsole.MarkupLine("[green]Using the built-in KTR symbol list.[/]");
                return PredefinedLists.KtrSymbols;
        }
        
        return new HashSet<string>();
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
                    symbols.Add(SymbolParser.NormalizeSymbol(part.Trim()));
                }
            }
            else
            {
                symbols.Add(SymbolParser.NormalizeSymbol(line.Trim()));
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
}

public enum AppMode { SymbolExtract = 1, Compare = 2, Match = 3, ManageCurrencies = 4 }
public enum OutputType { Txt, Excel } 