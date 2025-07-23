# Symbol Extractor
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/) [![C#](https://img.shields.io/badge/C%23-12-239120?logo=c-sharp)](https://learn.microsoft.com/en-us/dotnet/csharp/) [![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

A command-line tool to extract, compare, and manage trading symbols from API endpoints or local files. This tool is designed to help traders and developers work with symbol data from financial exchanges.

## Features

- **Extract Symbols**: Fetch symbol data from a given API URL or a local JSON file.
- **Save Formats**: Save the extracted symbols to either an Excel (.xlsx) or a Text (.txt) file.
- **Symbol Statistics**: Display statistics for quote currencies (e.g., USDT, BTC, ETH).
- **Compare with API**: Compare a user-provided symbol list against an API to find common and different symbols.
- **Compare Two Lists**: Compare two manually entered lists to find discrepancies.
- **Manage Currencies**: Maintain a local list of known quote currencies (add, remove, view).
- **Flexible Configuration**: Configure default save paths and API URLs via `appsettings.json`.

## How to Use

1.  **Clone the repository.**
2.  **Build the project** using Visual Studio or the `dotnet build` command.
3.  **Run the executable** from your terminal.

Upon running, the application will present a menu with the following modes:

### 1. Extract Symbols
This mode allows you to fetch symbols and save them.
- You'll be prompted to enter an API URL or the raw JSON content.
- The tool will extract symbols (e.g., `BTCUSDT`, `ETHUSDT`).
- It will then display statistics based on the quote currency (USDT, BTC, etc.).
- Finally, you can choose to save the symbol list as an Excel or TXT file.

### 2. Compare Symbols
This mode is for comparing a list of your symbols against an official API list.
- Provide the API URL for the source symbols.
- Enter your list of symbols directly into the console.
- The application generates an Excel report (`ComparisonReport.xlsx`) showing:
  - Common symbols.
  - Symbols that are in the API but not in your list.
  - Symbols that are in your list but not in the API.

### 3. Match Two Lists
This mode helps you find differences between two lists of symbols.
- Enter the first list of symbols.
- Enter the second list of symbols.
- The tool will display which symbols are missing from each list and save the results in `MissingInList1.txt` and `MissingInList2.txt`.

### 4. Manage Currencies
This mode is for managing the list of quote currencies that the application recognizes for statistical purposes.
- **View**: See all currently recognized currencies.
- **Add**: Add a new currency to the list.
- **Remove**: Remove an existing currency from the list.

## Configuration

You can configure the application using the `appsettings.json` file:

```json
{
  "AppSettings": {
    "DefaultSavePath": "C:\\Users\\YourUser\\Desktop\\SymbolExports"
  }
}
```
- `DefaultSavePath`: The default directory where generated files will be saved. If empty, your Desktop path will be used.

---
