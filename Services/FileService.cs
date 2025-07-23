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
} 