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
                worksheet.Cell(1, 1).Value = "Symbol";
                worksheet.Cell(1, 2).Value = "Base";
                worksheet.Cell(1, 3).Value = "Quote";

                worksheet.Row(1).Style.Font.Bold = true;

                int rowIdx = 2;
                foreach (var s in symbols)
                {
                    worksheet.Cell(rowIdx, 1).Value = s.Symbol;
                    worksheet.Cell(rowIdx, 2).Value = s.Base;
                    worksheet.Cell(rowIdx, 3).Value = s.Quote;
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
        IEnumerable<string> onlyInUser,
        string filePath)
    {
        try
        {
            using (var workbook = new XLWorkbook())
            {
                // Sheet for Common Symbols
                if (commonSymbols.Any())
                {
                    var wsCommon = workbook.Worksheets.Add("Common Symbols");
                    CreateSymbolInfoHeader(wsCommon);
                    wsCommon.Cell(2, 1).InsertData(commonSymbols);
                    wsCommon.Columns().AdjustToContents();
                }

                // Sheet for Symbols Only In API
                if (onlyInApi.Any())
                {
                    var wsApi = workbook.Worksheets.Add("Only In API");
                    CreateSymbolInfoHeader(wsApi);
                    wsApi.Cell(2, 1).InsertData(onlyInApi);
                    wsApi.Columns().AdjustToContents();
                }

                // Sheet for Symbols Only In User's List
                if (onlyInUser.Any())
                {
                    var wsUser = workbook.Worksheets.Add("Only In User List");
                    wsUser.Cell(1, 1).Value = "Symbol";
                    wsUser.Row(1).Style.Font.Bold = true;
                    wsUser.Cell(2, 1).InsertData(onlyInUser);
                    wsUser.Columns().AdjustToContents();
                }
                
                workbook.SaveAs(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving comparison report to Excel: {ex.Message}");
        }
    }

    private void CreateSymbolInfoHeader(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "Symbol";
        worksheet.Cell(1, 2).Value = "Base";
        worksheet.Cell(1, 3).Value = "Quote";
        worksheet.Row(1).Style.Font.Bold = true;
    }
} 