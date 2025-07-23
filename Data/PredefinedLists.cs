using System.Collections.Generic;

namespace symbol_extractor.Data;

public static class PredefinedLists
{
    public static readonly Dictionary<string, int> KtrSymbols = new()
    {
        { "BTCUSDT", 1 }, { "ETHUSDT", 2 }, { "LTCUSDT", 3 }, { "BCHUSDT", 4 },
        { "ETHBTC", 5 }, { "LTCBTC", 6 }, { "BCHBTC", 7 }, { "LTCETH", 8 },
        { "ONTETH", 9 }, { "BNBBTC", 10 }, { "ADABTC", 11 }, { "DOGEBTC", 12 },
        { "XRPBTC", 13 }, { "TFUELBTC", 14 }, { "LINKBTC", 15 }, { "MATICBTC", 16 },
        { "SOLBTC", 17 }, { "DOTBTC", 18 }, { "ATABTC", 20 }, { "THETABTC", 21 },
        { "VETBTC", 22 }, { "ETCBTC", 23 }, { "DATABTC", 24 }, { "WBTCBTC", 25 },
        { "KSMBTC", 26 }, { "RUNEBTC", 27 }, { "UNIBTC", 28 }, { "AAVEBTC", 29 },
        { "GTOBTC", 30 }, { "ICPBTC", 31 }, { "1INCHBTC", 32 }, { "NEBLBTC", 33 },
        { "XMRBTC", 34 }, { "BCDBTC", 35 }, { "CHZBTC", 36 }, { "SUSHIBTC", 37 },
        { "TRXBTC", 38 }, { "SXPBTC", 39 }, { "XVGBTC", 40 }, { "EOSBTC", 41 },
        { "FILBTC", 42 }, { "FTMBTC", 43 }, { "ENJBTC", 44 }, { "LUNABTC", 45 },
        { "GRTBTC", 46 }, { "FRONTBTC", 47 }, { "ZILBTC", 48 }, { "WAVESBTC", 49 },
        { "ATOMBTC", 50 }, { "SNXBTC", 51 }, { "YFIBTC", 52 }, { "KNCBTC", 53 },
        { "AVAXBTC", 54 }, { "NEOBTC", 55 }, { "CAKEBTC", 56 }, { "COTIBTC", 57 },
        { "ZECBTC", 58 }, { "STMXBTC", 59 }, { "OGNBTC", 60 }, { "FIOBTC", 61 },
        { "DASHBTC", 62 }, { "CRVBTC", 63 }, { "SCBTC", 64 }, { "PAXGBTC", 65 },
        { "RSRBTC", 66 }, { "ALGOBTC", 67 }, { "HBARBTC", 68 }, { "LTOBTC", 69 },
        { "OMGBTC", 70 }, { "COMPBTC", 71 }, { "MDABTC", 72 }, { "WRXBTC", 73 },
        { "ONEBTC", 74 }, { "IOSTBTC", 75 }, { "SUNBTC", 76 }, { "BQXBTC", 77 },
        { "BZRXBTC", 78 }, { "AUDIOBTC", 79 }, { "NEARBTC", 80 }, { "RLCBTC", 81 },
        { "ICXBTC", 82 }, { "XTZBTC", 83 }, { "BTSBTC", 84 }, { "RENBTC", 85 },
        { "CELRBTC", 86 }, { "DGBBTC", 87 }, { "DIABTC", 88 }, { "EGLDBTC", 89 },
        { "DOCKBTC", 90 }, { "FETBTC", 91 }, { "BARBTC", 92 }, { "RVNBTC", 93 },
        { "INJBTC", 94 }, { "ONTBTC", 95 }, { "IOTABTC", 96 }, { "KMDBTC", 97 },
        { "XEMBTC", 98 }, { "DCRBTC", 99 }, { "FTTBTC", 100 }, { "ADXBTC", 101 },
        { "QTUMBTC", 102 }, { "BTGBTC", 103 }, { "LRCBTC", 104 }, { "CTSIBTC", 105 },
        { "ONGBTC", 106 }, { "LSKBTC", 107 }, { "BTCTRY", 108 }, { "BNBTRY", 109 },
        { "NEOTRY", 110 }, { "SOLTRY", 111 }, { "LINKTRY", 112 }, { "DOTTRY", 113 },
        { "AVAXTRY", 114 }, { "EOSTRY", 115 }, { "SXPTRY", 116 }, { "ADATRY", 117 },
        { "MATICTRY", 118 }, { "BUSDTRY", 119 }, { "USDTTRY", 120 }, { "XRPTRY", 121 },
        { "ONTTRY", 122 }, { "DOGETRY", 123 }, { "XLMTRY", 124 }, { "CHZTRY", 125 },
        { "VETTRY", 126 }, { "TRXTRY", 127 }, { "ADAUSDT", 129 }, { "XRPUSDT", 130 },
        { "BNBUSDT", 131 }, { "DOGEUSDT", 132 }, { "MATICUSDT", 133 }, { "DOTUSDT", 134 },
        { "LINKUSDT", 135 }, { "QTUMUSDT", 136 }, { "VETUSDT", 137 }, { "FTTUSDT", 138 },
        { "ETCUSDT", 139 }, { "THETAUSDT", 140 }, { "SOLUSDT", 142 }, { "BNBETH", 144 },
        { "LINKETH", 145 }, { "DASHETH", 146 }, { "ETCETH", 147 }, { "QTUMETH", 149 },
        { "THETAETH", 150 }, { "EOSETH", 151 }, { "ADAETH", 152 }, { "XRPETH", 153 },
        { "VETETH", 154 }, { "ETHTRY", 155 }, { "SHIBTRY", 156 }, { "FDUSDTRY", 158 }
    };
} 