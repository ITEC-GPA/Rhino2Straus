# Note di versione

## 0.6.0 — 9 novembre 2026

- Tutti gli otto componenti Analysis avviano il solver EXE tramite `St7RunSolverProcess` senza attesa bloccante; le chiamate API e il rilascio delle sessioni rimangono sul thread UI. Export e lettura dei risultati restano sincroni.
- Coda comune per le analisi; le altre chiamate native falliscono subito con `API busy` durante l'analisi. Stato indipendente per ciascuna iterazione Grasshopper.
- Copie profonde di coordinate dei carichi e gruppi di carico/vincolo, senza riferimenti mutabili condivisi con gli input.
- Copia degli input al fronte Run, aggiornamento automatico dell'output al completamento, nessun riavvio con Run mantenuto True.
- Invalidazione per cambio degli input, sequenza A → B → A, input mancanti, rimozione del componente e chiusura documento. I risultati ricevuti in ingresso non attraversano l'output in attesa.
- Controllo dei file modificati durante l'attesa e dei file di output sovrascritti; rimane la verifica SHA-256 dei manifest Model.
- Nuove prove su responsività UI, coda, annullamento, risultati obsoleti e tutti gli otto solver reali. GUID, pannelli e socket invariati.

Verifiche: 84 test gestiti, 96 test nel runner nativo (inclusi 12 benchmark/regressioni native), 221 controlli geometrici/Grasshopper con tutti gli otto solver. Tutti i processi di verifica terminati con codice 0.

Aggiornamento: sostituire l'intera cartella del plugin. Il rilascio del pulsante Run non annulla l'analisi. Un lavoro obsoleto già avviato termina normalmente e può lasciare file sul disco; il suo risultato viene scartato dall'output Grasshopper. Vedere [MODEL-RESULTS](MODEL-RESULTS.md) e [VALIDATION](VALIDATION.md).

## 0.5.2 — 9 novembre 2026

- CI: versione del pacchetto ricavata dai metadati del progetto; ZIP mancante causa errore. Controllo dei cataloghi con `tools/catalogue.py --check`.
- API: ricerca esplicita accanto all'assembly e diagnostica dei tentativi di caricamento.
- Import/export: eliminate chiamate ai gruppi nativi dei nodi, non supportate da R31; omissione esplicita con avviso in export.
- Export: non linearità materiale disattivata per i modelli elastici, senza ereditare i default nativi.
- NLA: incrementi creati prima di abilitare i casi; freedom case 1 abilitato esplicitamente.
- Solver su file: cache invalidata al cambio di percorso o parametri; nuovo fronte Run necessario.
- Packaging: helper rinominato `Rhino2Straus.GrasshopperHelper.dll`, senza cambiare GUID, socket o namespace dei componenti.
- Test: runner nativo Rhino.Inside separato, resolver inizializzato prima delle API, riferimento della mensola comprensivo della deformabilità a taglio; benchmark modale, non lineare e buckling aggiunti.
- Documentazione: requisiti Python, dipendenza dell'audit congiunto e risultati effettivi aggiornati.

Verifiche: 69 test gestiti; 12 prove native (81 totali nel runner nativo); 181 controlli geometrici/GH; 164 componenti in 17 pannelli. Vedere [VALIDATION](VALIDATION.md) per riferimenti, tolleranze e limiti.

Aggiornamento: sostituire l'intera precedente cartella del plugin; non lasciare `GrasshopperHelper.dll` nella cartella di Rhino2Straus. Rigenerare i file esportati per applicare le nuove impostazioni elastiche. L'esecuzione dei solver rimane sincrona; il worker asincrono resta da implementare e collaudare.

## 0.5.1

Controllo di corrispondenza fra assembly e commit locale e correzioni al caricamento da worker.
