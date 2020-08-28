# Spuštění

Dvojklik na BacterioCrawler.exe (možná bude chtít povolit firewall). Standardně se vstupní data čtou z input.csv (oddělovač je ';').

Případné chyby se vypíší na konzoli (kdyžtak mi je pošli a já ti řeknu co s tím).

Výsledky budou v souboru results.csv.


# Konfigurace

Soubor BacterioCrawler.exe.config obsahuje konfiguraci programu (upravíš to normálně v poznámkovém bloku). Funguje to tak, že vždycky nastavíš value="..." u toho co chceš změnit, takže když budeš třeba chtít nastavit jméno vstupního souboru, změníš řádku:

	<add key="inputFile" value="input.csv"/>

na:
	<add key="inputFile" value="neco-jineho.csv"/>
	
Konfiguraci hodnoty 'cx' neni potřeba měnit, ale budeš si muset nastavit svůj google klíč.

Klíčová slova jsou v souboru keywords.csv, formát je:

klíčové slovo;negativ 1;negativ 2; negativ 3;...
další klíčové slovo;negativ 1;negativ 2;negativ 3;...

Když si to otevřeš v excelu tak to půjde vidět.


# Poznámky

Souborů s koncovkou '.dll' jsou knihovny pro program a musí být vždy ve stejné složce jako BacterioCrawler.exe.

Když jsem to testoval, tak vyhledávání na googlu občas trvalo docela dlouho, řádově desítky vteřin, takže když se program občas na chvilku zastaví, nejspíš čeká až mu po síti přijdou data.

Program si vytváří pomocnou složku html, kdyby zabírala moc místa (jsou tam stažené stránky s nálezy) tak ji po doběhnutí programu smaž.