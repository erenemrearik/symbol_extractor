using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using symbol_extractor.Models;
using symbol_extractor.Services;
using symbol_extractor.UI;
using Spectre.Console;

namespace symbol_extractor;

class Program
{
    // Services can be instantiated here. For larger apps, a dependency injection container would be used.
    private static readonly ConfigurationService ConfigService = new();
    private static readonly SymbolParser SymbolParser = new();
    private static readonly ApiService ApiService = new(SymbolParser);
    private static readonly FileService FileService = new();
    private static readonly SymbolComparisonService ComparisonService = new();
    private static readonly StatisticsService StatisticsService = new();

    static async Task Main(string[] args)
    {
        ConsoleUi.DisplayWelcomeMessage();
        var mode = ConsoleUi.GetAppMode();

        try
        {
            await RunSelectedMode(mode);
        }
        catch (Exception ex)
        {
            // A global error handler for any unexpected issues.
            ConsoleUi.DisplayMessage($"\nAn unexpected error occurred: {ex.Message}", true);
        }

        ConsoleUi.DisplayMessage("\nPress any key to exit.");
        Console.ReadKey();
    }

    private static async Task RunSelectedMode(AppMode mode)
    {
        var settings = ConfigService.GetAppSettings();
        var savePath = string.IsNullOrEmpty(settings.DefaultSavePath)
            ? ConsoleUi.GetDesktopPath()
            : settings.DefaultSavePath;

        // Ensure the directory exists
        Directory.CreateDirectory(savePath);

        switch (mode)
        {
            case AppMode.SymbolExtract:
                await RunSymbolExtractMode(savePath);
                break;
            case AppMode.Compare:
                await RunCompareMode(savePath);
                break;
            case AppMode.Match:
                RunMatchingMode(savePath);
                break;
            case AppMode.ManageCurrencies:
                RunManageCurrenciesMode();
                break;
        }
    }

    private static void RunManageCurrenciesMode()
    {
        while (true)
        {
            var action = ConsoleUi.GetCurrencyManagementAction();
            switch (action)
            {
                case 'v':
                    var currencies = SymbolParser.GetKnownCurrencies();
                    ConsoleUi.DisplayCurrencies(currencies);
                    break;
                case 'a':
                    var currencyToAdd = ConsoleUi.GetCurrencyToAdd();
                    if (SymbolParser.AddCurrency(currencyToAdd))
                    {
                        ConsoleUi.DisplayMessage($"'{currencyToAdd.ToUpper()}' was successfully added.");
                    }
                    else
                    {
                        ConsoleUi.DisplayMessage($"'{currencyToAdd.ToUpper()}' could not be added (it might already exist).");
                    }
                    break;
                case 'r':
                    var currencyToRemove = ConsoleUi.GetCurrencyToRemove();
                    if (SymbolParser.RemoveCurrency(currencyToRemove))
                    {
                        ConsoleUi.DisplayMessage($"'{currencyToRemove.ToUpper()}' was successfully removed.");
                    }
                    else
                    {
                        ConsoleUi.DisplayMessage($"'{currencyToRemove.ToUpper()}' was not found.");
                    }
                    break;
                case 'b':
                    return; // Back to main menu
                default:
                    ConsoleUi.DisplayMessage("Invalid action selected.");
                    break;
            }
        }
    }

    /// <summary>
    /// Fetches symbols from an API or JSON file and saves them.
    /// </summary>
    private static async Task RunSymbolExtractMode(string savePath)
    {
        List<SymbolInfo> symbols;
        var sourceChoice = ConsoleUi.GetInputJson(); // This now handles the logic

        if (sourceChoice.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            symbols = await ApiService.GetSymbolsAsync(sourceChoice);
        }
        else
        {
            symbols = ApiService.ExtractSymbolsFromJson(sourceChoice); // It's already file content
        }
        
        if (symbols == null || !symbols.Any())
        {
            ConsoleUi.DisplayMessage("No symbols were found.", true);
            return;
        }

        AnsiConsole.MarkupLine($"Total symbols found: [bold green]{symbols.Count}[/]");
        
        // Display statistics
        var stats = StatisticsService.GetQuoteCurrencyCounts(symbols);
        ConsoleUi.DisplayQuoteStatistics(stats);

        var outputType = ConsoleUi.GetOutputType();
        var fileName = ConsoleUi.GetOutputFileName();
        var fullPath = Path.Combine(savePath, fileName);

        if (outputType == OutputType.Excel)
        {
            FileService.SaveSymbolsToExcel(symbols, fullPath + ".xlsx");
            ConsoleUi.DisplayMessage($"Successfully saved to '[yellow]{fullPath}.xlsx[/]'.");
        }
        else
        {
            FileService.SaveSymbolsToTxt(symbols.Select(s => s.Symbol), fullPath + ".txt");
            ConsoleUi.DisplayMessage($"Successfully saved to '[yellow]{fullPath}.txt[/]'.");
        }
    }

    /// <summary>
    /// Compares symbols from an API with a user-provided list and finds common ones.
    /// </summary>
    private static async Task RunCompareMode(string savePath)
    {
        var apiUrl = ConsoleUi.GetApiUrl();
        var apiSymbols = await ApiService.GetSymbolsAsync(apiUrl);

        if (apiSymbols == null || !apiSymbols.Any())
        {
            ConsoleUi.DisplayMessage("Could not fetch symbols from the API.", true);
            return;
        }

        var userSymbols = ConsoleUi.GetUserSymbolList();
        if (!userSymbols.Any())
        {
            ConsoleUi.DisplayMessage("No symbols were provided by the user.", true);
            return;
        }

        var (commonSymbols, onlyInApi, onlyInUser) = ComparisonService.FindSymbolDifferences(apiSymbols, userSymbols);

        if (!commonSymbols.Any() && !onlyInApi.Any() && !onlyInUser.Any())
        {
            ConsoleUi.DisplayMessage("No symbols found to compare.", true);
            return;
        }

        var outputFile = Path.Combine(savePath, "ComparisonReport.xlsx");
        FileService.SaveComparisonReportToExcel(commonSymbols, onlyInApi, onlyInUser, outputFile);
        
        AnsiConsole.MarkupLine($"\nComparison report saved to '[yellow]{outputFile}[/]'.");
        var table = new Table().Border(TableBorder.Simple);
        table.AddColumn("Category");
        table.AddColumn("Count");
        table.AddRow("[cyan]Common[/]", $"[bold green]{commonSymbols.Count}[/]");
        table.AddRow("[red]Only in API[/]", $"[bold red]{onlyInApi.Count}[/]");
        table.AddRow("[yellow]Only in your list[/]", $"[bold yellow]{onlyInUser.Count}[/]");
        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Compares two manually entered lists of symbols and shows the differences.
    /// </summary>
    private static void RunMatchingMode(string savePath)
    {
        var list1 = ConsoleUi.ReadSymbolsFromConsole("[grey]Enter the first list of symbols (end with an empty line):[/]");
        var list2 = ConsoleUi.ReadSymbolsFromConsole("[grey]Enter the second list of symbols (end with an empty line):[/]");

        var (missingInSecond, missingInFirst) = ComparisonService.FindMissingSymbols(list1, list2);

        ConsoleUi.DisplayResults("Symbols in [yellow]List 1[/] but not in List 2", missingInSecond);
        ConsoleUi.DisplayResults("Symbols in [yellow]List 2[/] but not in List 1", missingInFirst);
        
        var path1 = Path.Combine(savePath, "MissingInList2.txt");
        File.WriteAllLines(path1, missingInSecond);
        
        var path2 = Path.Combine(savePath, "MissingInList1.txt");
        File.WriteAllLines(path2, missingInFirst);

        ConsoleUi.DisplayMessage($"\nDifference reports saved to '[yellow]{savePath}[/]'.");
    }
}
