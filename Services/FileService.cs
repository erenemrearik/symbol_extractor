using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using symbol_extractor.Models;

namespace symbol_extractor.Services;

public class FileService
{
    public List<string> ReadSymbolsFromTxt(string filePath)
    {
        return File.ReadAllLines(filePath).ToList();
    }

    public List<string> ReadSymbolsFromExcel(string filePath)
    {
        var symbols = new List<string>();
        try
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheets.First();
                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    var symbol = row.Cell(1).GetString();
                    if (!string.IsNullOrWhiteSpace(symbol))
                    {
                        symbols.Add(symbol);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading Excel file: {ex.Message}");
        }
        return symbols;
    }

    public void SaveSymbolsToTxt(IEnumerable<string> symbols, string filePath)
    {
        File.WriteAllLines(filePath, symbols);
    }

    public void SaveSymbolsToExcel(IEnumerable<SymbolInfo> symbols, string filePath)
    {
        try
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Symbols");
                CreateSymbolInfoHeader(worksheet);

                int rowIdx = 2;
                foreach (var s in symbols)
                {
                    worksheet.Cell(rowIdx, 1).Value = s.Id;
                    worksheet.Cell(rowIdx, 2).Value = s.Symbol;
                    worksheet.Cell(rowIdx, 3).Value = s.Base;
                    worksheet.Cell(rowIdx, 4).Value = s.Quote;
                    rowIdx++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to Excel file: {ex.Message}");
        }
    }

    public void SaveComparisonReportToExcel(
        IEnumerable<SymbolInfo> commonSymbols,
        IEnumerable<SymbolInfo> onlyInApi,
        IEnumerable<SymbolInfo> onlyInUser,
        string filePath)
    {
        try
        {
            using (var workbook = new XLWorkbook())
            {
                if (commonSymbols.Any())
                {
                    var ws = workbook.Worksheets.Add("Common Symbols");
                    PopulateSymbolInfoSheet(ws, commonSymbols);
                }

                if (onlyInApi.Any())
                {
                    var ws = workbook.Worksheets.Add("Only In API");
                    PopulateSymbolInfoSheet(ws, onlyInApi);
                }

                if (onlyInUser.Any())
                {
                    var ws = workbook.Worksheets.Add("Only In User List");
                    PopulateSymbolInfoSheet(ws, onlyInUser);
                }

                workbook.SaveAs(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving comparison report to Excel: {ex.Message}");
        }
    }

    public void SaveMultiListComparisonToExcel(
        List<SymbolList> symbolLists,
        List<SymbolInfo> commonSymbols,
        Dictionary<string, List<SymbolInfo>> uniqueSymbolsPerList,
        string filePath)
    {
        try
        {
            using (var workbook = new XLWorkbook())
            {
                // Add Common Symbols sheet
                if (commonSymbols.Any())
                {
                    var commonSheet = workbook.Worksheets.Add("Common Symbols");
                    PopulateSymbolInfoSheet(commonSheet, commonSymbols);
                }

                // Add individual list sheets
                foreach (var list in symbolLists)
                {
                    var worksheet = workbook.Worksheets.Add(list.Name);
                    PopulateSymbolInfoSheet(worksheet, list.Symbols);
                }

                // Add unique symbols sheets
                foreach (var kvp in uniqueSymbolsPerList)
                {
                    if (kvp.Value.Any())
                    {
                        var uniqueSheet = workbook.Worksheets.Add($"{kvp.Key} - Unique");
                        PopulateSymbolInfoSheet(uniqueSheet, kvp.Value);
                    }
                }

                workbook.SaveAs(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving multi-list comparison to Excel: {ex.Message}");
        }
    }

    public void SaveValidationReportToExcel(
        List<SymbolParseError> parseErrors,
        List<DuplicateSymbol> duplicateSymbols,
        List<SymbolParseInconsistency> parseInconsistencies,
        string filePath)
    {
        try
        {
            using (var workbook = new XLWorkbook())
            {
                // Parse Errors sheet
                if (parseErrors.Any())
                {
                    var errorSheet = workbook.Worksheets.Add("Parse Errors");
                    CreateParseErrorHeader(errorSheet);
                    
                    int row = 2;
                    foreach (var error in parseErrors)
                    {
                        errorSheet.Cell(row, 1).Value = error.Symbol;
                        errorSheet.Cell(row, 2).Value = error.ListName;
                        errorSheet.Cell(row, 3).Value = error.ErrorMessage;
                        errorSheet.Cell(row, 4).Value = error.StoredBase;
                        errorSheet.Cell(row, 5).Value = error.StoredQuote;
                        errorSheet.Cell(row, 6).Value = error.ParsedBase;
                        errorSheet.Cell(row, 7).Value = error.ParsedQuote;
                        row++;
                    }
                    errorSheet.Columns().AdjustToContents();
                }

                // Duplicate Symbols sheet
                if (duplicateSymbols.Any())
                {
                    var duplicateSheet = workbook.Worksheets.Add("Duplicate Symbols");
                    CreateDuplicateSymbolHeader(duplicateSheet);
                    
                    int row = 2;
                    foreach (var duplicate in duplicateSymbols)
                    {
                        foreach (var occurrence in duplicate.Occurrences)
                        {
                            duplicateSheet.Cell(row, 1).Value = duplicate.NormalizedSymbol;
                            duplicateSheet.Cell(row, 2).Value = occurrence.Symbol;
                            duplicateSheet.Cell(row, 3).Value = occurrence.Base;
                            duplicateSheet.Cell(row, 4).Value = occurrence.Quote;
                            duplicateSheet.Cell(row, 5).Value = occurrence.ListName;
                            row++;
                        }
                    }
                    duplicateSheet.Columns().AdjustToContents();
                }

                // Parse Inconsistencies sheet
                if (parseInconsistencies.Any())
                {
                    var inconsistencySheet = workbook.Worksheets.Add("Parse Inconsistencies");
                    CreateParseInconsistencyHeader(inconsistencySheet);
                    
                    int row = 2;
                    foreach (var inconsistency in parseInconsistencies)
                    {
                        foreach (var parseInfo in inconsistency.ParseInfos)
                        {
                            inconsistencySheet.Cell(row, 1).Value = inconsistency.NormalizedSymbol;
                            inconsistencySheet.Cell(row, 2).Value = parseInfo.Symbol;
                            inconsistencySheet.Cell(row, 3).Value = parseInfo.StoredBase;
                            inconsistencySheet.Cell(row, 4).Value = parseInfo.StoredQuote;
                            inconsistencySheet.Cell(row, 5).Value = parseInfo.ParsedBase;
                            inconsistencySheet.Cell(row, 6).Value = parseInfo.ParsedQuote;
                            inconsistencySheet.Cell(row, 7).Value = parseInfo.ListName;
                            row++;
                        }
                    }
                    inconsistencySheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving validation report to Excel: {ex.Message}");
        }
    }

    private void PopulateSymbolInfoSheet(IXLWorksheet worksheet, IEnumerable<SymbolInfo> symbols)
    {
        CreateSymbolInfoHeader(worksheet);
        int currentRow = 2;
        foreach (var symbol in symbols)
        {
            worksheet.Cell(currentRow, 1).Value = symbol.Id;
            worksheet.Cell(currentRow, 2).Value = symbol.Symbol;
            worksheet.Cell(currentRow, 3).Value = symbol.Base;
            worksheet.Cell(currentRow, 4).Value = symbol.Quote;
            currentRow++;
        }
        worksheet.Columns().AdjustToContents();
    }

    private void CreateSymbolInfoHeader(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Symbol";
        worksheet.Cell(1, 3).Value = "Base";
        worksheet.Cell(1, 4).Value = "Quote";
        worksheet.Row(1).Style.Font.Bold = true;
    }

    private void CreateParseErrorHeader(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "Symbol";
        worksheet.Cell(1, 2).Value = "List Name";
        worksheet.Cell(1, 3).Value = "Error Message";
        worksheet.Cell(1, 4).Value = "Stored Base";
        worksheet.Cell(1, 5).Value = "Stored Quote";
        worksheet.Cell(1, 6).Value = "Parsed Base";
        worksheet.Cell(1, 7).Value = "Parsed Quote";
        worksheet.Row(1).Style.Font.Bold = true;
    }

    private void CreateDuplicateSymbolHeader(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "Normalized Symbol";
        worksheet.Cell(1, 2).Value = "Original Symbol";
        worksheet.Cell(1, 3).Value = "Base";
        worksheet.Cell(1, 4).Value = "Quote";
        worksheet.Cell(1, 5).Value = "List Name";
        worksheet.Row(1).Style.Font.Bold = true;
    }

    private void CreateParseInconsistencyHeader(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "Normalized Symbol";
        worksheet.Cell(1, 2).Value = "Original Symbol";
        worksheet.Cell(1, 3).Value = "Stored Base";
        worksheet.Cell(1, 4).Value = "Stored Quote";
        worksheet.Cell(1, 5).Value = "Parsed Base";
        worksheet.Cell(1, 6).Value = "Parsed Quote";
        worksheet.Cell(1, 7).Value = "List Name";
        worksheet.Row(1).Style.Font.Bold = true;
    }
} 