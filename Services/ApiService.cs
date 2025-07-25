using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using symbol_extractor.Models;

namespace symbol_extractor.Services;

public class ApiService
{
    private static readonly HttpClient HttpClient = new();
    private readonly SymbolParser _symbolParser;

    public ApiService(SymbolParser symbolParser)
    {
        _symbolParser = symbolParser;
    }

    public async Task<List<SymbolInfo>> GetSymbolsAsync(string url)
    {
        var json = await HttpClient.GetStringAsync(url);
        return ExtractSymbolsFromJson(json);
    }

    public List<SymbolInfo> ExtractSymbolsFromJson(string json)
    {
        var result = new List<SymbolInfo>();
        try
        {
            var jToken = JToken.Parse(json);

            JArray itemsArray = null;

            if (jToken is JObject jObj)
            {
                if (jObj["data"] is JObject dataObj && dataObj["list"] is JArray listArray)
                {
                    itemsArray = listArray;
                }
                else if (jObj["data"] is JArray dataArray)
                {
                    itemsArray = dataArray;
                }
            }
            else if (jToken is JArray jArray)
            {
                itemsArray = jArray;
            }

            if (itemsArray != null)
            {
                foreach (var item in itemsArray)
                {
                    string symbol = null;
                    string baseCurrency = null;
                    string quoteCurrency = null;
                    
                    if (item is JObject itemObj)
                    {
                        symbol = itemObj["symbol"]?.ToString();
                        baseCurrency = itemObj["baseAsset"]?.ToString();
                        quoteCurrency = itemObj["quoteAsset"]?.ToString();
                    }
                    else if (item is JValue itemVal && itemVal.Type == JTokenType.String)
                    {
                        symbol = itemVal.ToString();
                    }

                    if (!string.IsNullOrEmpty(symbol))
                    {
                        if (!string.IsNullOrEmpty(baseCurrency) && !string.IsNullOrEmpty(quoteCurrency))
                        {
                            result.Add(new SymbolInfo(symbol, baseCurrency, quoteCurrency));
                        }
                        else
                        {
                            var (parsedBase, parsedQuote) = _symbolParser.ParseSymbol(symbol);
                            result.Add(new SymbolInfo(symbol, parsedBase, parsedQuote));
                        }
                    }
                }
            }
            else if (jToken is JObject singleObj && singleObj["symbol"] != null)
            {
                var symbol = singleObj["symbol"].ToString();
                if (!string.IsNullOrEmpty(symbol))
                {
                    var (baseCurrency, quoteCurrency) = _symbolParser.ParseSymbol(symbol);
                    result.Add(new SymbolInfo(symbol, baseCurrency, quoteCurrency));
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
        }
        return result;
    }
} 