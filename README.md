# Symbol Extractor

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

# Sembol Çıkarıcı

API endpoint'lerinden veya yerel dosyalardan alım satım sembollerini çıkarmak, karşılaştırmak ve yönetmek için bir komut satırı aracıdır. Bu araç, trader'ların ve geliştiricilerin finansal borsalardan gelen sembol verileriyle çalışmasına yardımcı olmak için tasarlanmıştır.

## Özellikler

- **Sembol Çıkarma**: Belirtilen bir API URL'sinden veya yerel bir JSON dosyasından sembol verilerini alın.
- **Kayıt Formatları**: Çıkarılan sembolleri Excel (.xlsx) veya Metin (.txt) dosyası olarak kaydedin.
- **Sembol İstatistikleri**: Karşıt para birimleri (örn. USDT, BTC, ETH) için istatistikleri görüntüleyin.
- **API ile Karşılaştırma**: Ortak ve farklı sembolleri bulmak için kullanıcı tarafından sağlanan bir sembol listesini bir API'ye karşı karşılaştırın.
- **İki Listeyi Karşılaştırma**: Uyuşmazlıkları bulmak için manuel olarak girilen iki listeyi karşılaştırın.
- **Para Birimlerini Yönetme**: Bilinen karşıt para birimlerinin yerel bir listesini tutun (ekleme, çıkarma, görüntüleme).
- **Esnek Yapılandırma**: `appsettings.json` aracılığıyla varsayılan kaydetme yollarını ve API URL'lerini yapılandırın.

## Nasıl Kullanılır

1.  **Depoyu klonlayın.**
2.  **Projeyi derleyin** (Visual Studio veya `dotnet build` komutu ile).
3.  **Uygulamayı çalıştırın** (terminal üzerinden).

Çalıştırıldığında, uygulama aşağıdaki modları içeren bir menü sunacaktır:

### 1. Sembolleri Çıkar
Bu mod, sembolleri çekmenizi ve kaydetmenizi sağlar.
- Bir API URL'si veya ham JSON içeriği girmeniz istenecektir.
- Araç sembolleri çıkaracaktır (örn. `BTCUSDT`, `ETHUSDT`).
- Daha sonra karşıt para birimine (USDT, BTC, vb.) göre istatistikleri gösterecektir.
- Son olarak, sembol listesini bir Excel veya TXT dosyası olarak kaydetmeyi seçebilirsiniz.

### 2. Sembolleri Karşılaştır
Bu mod, sembol listenizi resmi bir API listesiyle karşılaştırmak içindir.
- Kaynak semboller için API URL'sini sağlayın.
- Sembol listenizi doğrudan konsola girin.
- Uygulama, aşağıdakileri gösteren bir Excel raporu (`ComparisonReport.xlsx`) oluşturur:
  - Ortak semboller.
  - API'de olan ancak listenizde olmayan semboller.
  - Listenizde olan ancak API'de olmayan semboller.

### 3. İki Listeyi Eşleştir
Bu mod, iki sembol listesi arasındaki farkları bulmanıza yardımcı olur.
- İlk sembol listesini girin.
- İkinci sembol listesini girin.
- Araç, her listede hangi sembollerin eksik olduğunu gösterecek ve sonuçları `MissingInList1.txt` ve `MissingInList2.txt` dosyalarına kaydedecektir.

### 4. Para Birimlerini Yönet
Bu mod, uygulamanın istatistiksel amaçlarla tanıdığı karşıt para birimleri listesini yönetmek içindir.
- **Görüntüle**: Tanınan tüm para birimlerini görün.
- **Ekle**: Listeye yeni bir para birimi ekleyin.
- **Kaldır**: Listeden mevcut bir para birimini kaldırın.

## Yapılandırma

Uygulamayı `appsettings.json` dosyasını kullanarak yapılandırabilirsiniz:

```json
{
  "AppSettings": {
    "DefaultSavePath": "C:\\Users\\KullaniciAdiniz\\Desktop\\SymbolExports"
  }
}
```
- `DefaultSavePath`: Oluşturulan dosyaların kaydedileceği varsayılan dizin. Boş bırakılırsa, Masaüstü yolunuz kullanılır. 