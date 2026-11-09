# Stato delle verifiche

## Verifica del 9 novembre 2026 — correzioni per 0.5.2

Ambiente: Windows x64, SDK 9.0.320, Rhino 8.36.26296.17001, API Straus7 R31. La licenza API e i quattro solver hanno funzionato: l'errore -8 nelle sezioni successive riguarda solo le prove storiche.

- Build Release: zero errori e avvisi; **69 test gestiti** passati.
- **181 controlli geometrici e Grasshopper** passati, inclusi 36 nuovi controlli di cache dei solver su file. I 164 componenti sono istanziati e confrontati con GUID, icone e pannelli del catalogo.
- **12 prove native**, oltre ai test gestiti: export, avvisi gruppi nodali, unità/connettività/ID/carichi, import geometrico e strutturale, protezione file, attributi, statica, modale, non lineare, buckling e combinazioni. **81 passati, 0 falliti**: [rapporto](validation/2026-11-09-native.txt).
- Catalogo rigenerato senza differenze: 164 componenti, 164 GUID unici, 17 pannelli. Il nuovo `--check` controlla in CI i documenti generati.
- Helper specifico `Rhino2Straus.GrasshopperHelper.dll` verificato in Grasshopper. Non ripetuta la prova congiunta dei quattro plugin.
- CI: nome ZIP e presenza del pacchetto verificati localmente; workflow remoto non avviato.

### Benchmark riproducibili

Comando: `dotnet run --project tools/NativeTests -c Release -- --solve`. Il runner inizializza Rhino.Inside senza finestra e il resolver Straus7. `RHINO_SYSTEM_DIRECTORY` può indicare un'altra installazione Rhino. I file rimangono nella cartella temporanea indicata dal runner.

- Mensola: L=2 m, Pz=-1 kN, E=2.1e8 kPa, ν=0.3, I=1e-4 m⁴, As=0.01 m². Con P=1 kN, `uz = -[PL³/(3EI) + PL/(GAs)] = -0.000129460317460317 m`. Tolleranza relativa 1e-6; reazione verticale +1 kN entro 1e-6 kN.
- Oscillatore assiale: barra senza massa distribuita, L=2 m, A=0.01 m², massa di estremità 1 t, solo traslazione X libera. `f = sqrt(EA/(Lm))/(2π)`, tolleranza relativa 1e-5.
- Non lineare geometrica: stessa mensola, quattro incrementi; limite dei piccoli spostamenti entro 1e-4 relativo dalla soluzione elastica. Non verifica plasticità né grandi spostamenti.
- Buckling: mensola snella L=20 m, compressione 1 kN; `Pcr = π²EI/(4L²) = 129.538557764298 kN`. Fattore con 4 elementi 129.514306151800; con 16 elementi 129.509832671829. Scarto da Eulero e variazione fra mesh inferiori a 0.1%, includendo gli effetti di taglio/accorciamento trascurati dal riferimento.
- Combinazione lineare: fattore 2, spostamento doppio del caso base entro 1e-8 m; manifest verificato e rilettura nel Model.

Non costituiscono un collaudo completo dei risultati FEM di piastre, solidi, sezioni native, link o di tutte le opzioni dei solver. Le verifiche geometriche rimangono distinte dalle prove numeriche native.

### Responsività: valutazione e lavoro residuo

Le prove elementari hanno richiesto circa 2–4 secondi per analisi; il buckling con due mesh e due analisi statiche circa 9 secondi. Non sono misure di responsività del viewport o stime per modelli grandi.

I solver restano **sincroni**. `St7RunSolver(..., btTrue)` attende il processo e `StrausSession` mantiene un Monitor condiviso. Un semplice `Task.Run` lascerebbe altre chiamate native dell'interfaccia in attesa dello stesso lock; il Monitor deve inoltre essere rilasciato dallo stesso thread.

Resta da implementare un worker con isolamento delle chiamate native, snapshot degli input e stati Queued/Running/Completed/Failed/Obsolete. Servono serializzazione dei lavori sullo stesso file, acquisizione input GH sul thread dell'interfaccia, scarto risultati a input modificati e gestione della chiusura di componenti/documenti. Occorrono prove interattive di responsività e arresto: questa versione non dichiara risolto il blocco dell'interfaccia.

## Versione 0.5.1

Aggiunto il controllo di corrispondenza fra assembly e commit e corretto il suo caricamento da worker. Nessun nuovo esito numerico è attribuito retroattivamente a questa versione.

## Versione 0.5.0 — 30 settembre 2026

- Build Release senza errori/avvisi; **69 test gestiti** (inclusi import parziale, nodi coincidenti, merge, casi inutilizzati, temperature e stato Run/cache).
- **74 verifiche geometriche + 71 verifiche dati Grasshopper = 145 controlli** superati; i nuovi controlli coprono filtri con proprietà complete e sezioni/spessori legacy.
- **164 componenti** istanziati nel runtime GH con GUID unici, icone e pannelli corretti; **166 icone PNG** nell'atlante aggiornato.
- **23 definizioni**, ciascuna salvata, riaperta e ricalcolata in `.gh` e `.ghx`: zero errori di runtime. **164/164 componenti** usati, nessuno mancante. [Rapporto per file](../examples/validation.json) · [Copertura](../examples/component-coverage.json).
- I **18 esempi con Model offline** superano preflight, controlli dei conteggi previsti e Brep chiusi; il toggle preview lascia invariati dati FEM e uscite per bake. Gli altri 5 mostrano flussi dipendenti da file/API e sono verificati nello stato di attesa. Gli avvisi di dati mancanti sui rami disattivati sono elencati nel rapporto.
- Nessuna API Straus invocata durante la generazione degli esempi; i comandi Read/Run/Bake/Overwrite partono False. Risultati sintetici assenti dai file distribuiti.
- **Prova nativa tentata nuovamente:** St7Init restituisce **-8**, hardware lock STANDALONE non trovato. Il test del nuovo import (confronto byte sorgente, geometria, proprietà e carichi) è presente ma non raggiunto; né roundtrip né solver nativi sono dichiarati collaudati.
- `Read Straus Model` è descritto con limiti e semantica Complete/Allow partial nella [guida import](MODEL-IMPORT.md). Il sorgente viene letto tramite copia temporanea.
- La nuova identità include metadati termici: rigenerare export/risultati di versioni precedenti prima della rilettura con la 0.5.

## Verifiche precedenti

Data: 26 settembre 2026. Ambiente: Windows x64, .NET SDK 9, Rhino 8, Straus7 R31.

- Build Release dell'intera soluzione: completata senza errori e senza avvisi.
- **64 test gestiti**: unità, proprietà, copie indipendenti, merge di nodi e Model, ID, carichi, vincoli, spostamenti imposti, solidi, tabelle immutabili dei risultati, identità del Model, invalidazione, manifest SHA-256, dipendenze delle combinazioni, resistenze materiali, copie dei casi, interruttori della preview e conversione delle distanze dei carichi distribuiti.
- **74 controlli geometrici nel motore Rhino**: sezioni, inerzie principali, estrusione con volume positivo, offset trave, inserimento, trasformazione rigida, spessore e offset piastra, mesh chiusa, allineamento assi di travi verticali/inclinate e piastre, direzioni parallele rifiutate e assi geometrici/principali. Comprendono posizioni e direzioni dei carichi, facce native, annotazioni degli attributi, filtri, geometria senza sezioni e master OFF; inoltre volumi Brep di piastre/Tet/Wedge/Hex, coordinate grandi, sezioni cave/composte, identificativi e segnalazione delle geometrie non convertibili.
- **163 componenti istanziati nel runtime Grasshopper**: GUID unici, icone 24×24, registrazione delle uscite e corrispondenza con i 17 pannelli del catalogo. Verificata anche la collocazione del parametro Group nel pannello Modello; nomi, GUID e registrazione dei socket conservati rispetto alla 0.4.1.
- **63 controlli di calcolo dati Grasshopper** con risultati sintetici: rimappatura N/M2, estremi firmati e posizioni governanti, decomposizione Model, alberi di spostamenti, collegamento dati del verificatore e deformata. Includono default, passaggio reale delle opzioni attraverso connessioni, gestione delle preview per documento, menu, risultati ON/OFF, ripristino dopo rimozione del gestore, cast verso parametri Brep e compatibilità delle uscite. Sono prove dei componenti, non soluzioni FEM native.
- **Bake eseguito realmente in documenti Rhino temporanei senza interfaccia** tramite `IGH_BakeAwareObject`, sia dal Display Manager sia da Model Physical Geometry: oggetti Brep chiusi, nessuna mesh o linea duplicata, attributi Rhino e ID conservati; il Display Manager viene provato anche con preview OFF. Il menu interattivo e Undo nell'interfaccia rimangono da collaudare manualmente.
- **165 PNG** (163 componenti, libreria e parametro Group) e atlante controllati visivamente; generazione indipendente da Straus7.
- [Tavola dei simboli](preview-gallery.png) generata dalle stesse primitive geometriche della preview e controllata visivamente. La cattura GPU del viewport non è disponibile nell'ambiente automatico: rimane da completare il collaudo interattivo del rendering, inclusa la leggibilità dei testi nei modelli affollati.
- SDK R31 importato localmente: 5.741 dichiarazioni confrontate, nessuna mancante o differente.
- I nuovi profili nativi e le chiamate native dei nuovi link sono implementati sul manuale R31; la loro esecuzione e il confronto con Straus7 rimangono da collaudare con licenza API operativa.
- La conversione delle posizioni finali dei carichi distribuiti è verificata rispetto ai diagrammi del manuale R3, pagina stampata 1010, e con test gestiti; il roundtrip nativo resta da eseguire. La versione 0.4 invalida la provenienza degli export precedenti: rigenerare esportazioni e risultati.
- **Test nativo Straus7 non completato**: `St7Init` restituisce errore **-8**, licenza STANDALONE senza hardware lock disponibile. Non sono stati prodotti risultati di solver da usare come validazione. Il relativo test è presente e può essere rilanciato oggi con `dotnet run --project tools/NativeTests -c Release -- --solve` quando la licenza è accessibile.

Questi test verificano la libreria e parte dell'integrazione Rhino. Non attestano ancora un collaudo interattivo completo di ogni componente in Grasshopper né la correttezza ingegneristica di un modello dell'utente.

## Esempi Grasshopper — 28 settembre 2026

Tre definizioni complete, in formato `.gh` e `.ghx`, ricalcolate tramite le connessioni reali nel runtime Rhino 8.35 / Grasshopper. Ogni file viene salvato, riletto e ricalcolato: nessun componente mancante, collegamenti preservati, nessun errore di calcolo o preflight. Le verifiche degli esempi sono in [examples/validation.json](../examples/validation.json).

- Mensola: 19 componenti del plugin, 2 nodi, 1 trave, 1 caso; 1 Brep chiuso, volume 0.24 m³.
- Portale: 35 componenti del plugin e uno Stream Gate standard, 4 nodi, 3 travi, 2 casi, 2 combinazioni; 3 Brep chiusi, volume 0.0816 m³.
- Piastra: 17 componenti del plugin, 9 nodi, 4 Quad4, 1 caso; 4 Brep chiusi, volume 2.4 m³.
- Verificato l'interruttore preview tramite il collegamento al gestore: stato OFF, fingerprint del Model invariato e Brep ancora disponibili. I file sono salvati con preview ON, Run e Overwrite OFF.
- Gli 8 avvisi del portale provengono dai lettori risultati e dai relativi pannelli tenuti senza dati dallo Stream Gate quando Run=false. Negli altri due esempi non ci sono avvisi.
- Canvas renderizzati con Grasshopper e controllati per disposizione dei gruppi, componenti, note e connessioni. Il rendering 3D interattivo resta soggetto ai limiti già descritti sopra.

Il generatore non invoca il solver nativo: la presenza dei collegamenti ai lettori non costituisce validazione di un'analisi Straus7. I file distribuiti non contengono risultati FEM precalcolati.
