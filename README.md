# Rhino2Straus

Componenti **Rhino 8 / Grasshopper per Straus7 R3, Windows x64**. Il progetto è dedicato esclusivamente a Straus7: modellazione, attributi, modello FEM unificato, preview, esportazione `.st7`, analisi e lettura dei risultati.

**163 componenti in 17 pannelli per argomento** nella tab **Rhino2Straus**, con icone a 24 px, più le icone della libreria e del parametro Group. [Mappa dei pannelli](docs/TOPICS.md) · [Catalogo per argomento](docs/COMPONENTS.md) · [Preview e impostazioni](docs/PREVIEW.md) · [Sezioni R31, link e default](docs/SECTIONS-LINKS.md) · [Copertura API e limiti](docs/API-COVERAGE.md) · [Verifiche](docs/VALIDATION.md) · [Model, solver e risultati](docs/MODEL-RESULTS.md).

La **0.4.2** separa Materiali, Sezioni, Proprietà, Elementi, Vincoli e link, Assi e offset, Casi e combo, Carichi, Modello, File Straus, Analisi e Preview/bake. I risultati hanno cinque pannelli dedicati: Model, nodi/link, travi, piastre e solidi. Cartelle sorgenti, catalogo e atlante delle icone seguono la stessa suddivisione. I GUID e i collegamenti dei componenti restano invariati. Dopo l'installazione riavviare Rhino/Grasshopper per aggiornare la tab.

## Esempi pronti

La **0.4.3** include tre definizioni Grasshopper con componenti già collegati: **mensola con offset e bake**, **portale con merge, casi/combinazioni e lettori dei risultati**, **piastra da mesh con pressione e assi locali**. Geometrie interne, slider, note in italiano, preview disattivabile e uscite Brep. Il calcolo nativo è inizialmente disattivato. [Apri la guida e scarica i file .gh / .ghx](examples/README.md).

## Impostazione del modello

```mermaid
flowchart LR
  G[Geometrie Rhino] --> E[Nodi / travi / piastre / solidi / link]
  P[Materiali e proprietà] --> E
  E --> A[Vincoli / carichi / offset / gruppi]
  A --> B[Build Model]
  M[Altri Model] --> U[Merge Straus Models]
  B --> U
  U --> S[Un solo Model]
  S --> V[Validate / Preview / Decompose]
  S --> X[Straus7 R3 Export]
  X --> F[File .st7]
  F --> R[Solver / risultati / import geometrico]
  S --> C[Run Model: casi e combinazioni / altri solver]
  C --> RM[Model con risultati incorporati]
  RM --> Q[Sollecitazioni / deformata / diagrammi]
  RM --> K[Dati per futuro verificatore]
```

Gli attributi modificano copie degli elementi. `Build Model` salda i nodi entro la tolleranza, unifica i riferimenti e raccoglie proprietà, casi e combinazioni. Accetta anche altri Model. `Merge Straus Models` restituisce sempre un solo Model; `Renumber` permette di unire parti create indipendentemente. Con `Renumber=false`, gli stessi ID di elemento devono avere geometria e proprietà compatibili: i carichi si sommano come contributi e i gruppi si uniscono. Gli input restano invariati. Non collegare due volte lo stesso contributo di carico.

`Rhino2Straus Display Manager` mostra sezioni estruse, spessori, offset, carichi, vincoli, svincoli, molle, masse, spostamenti imposti, proprietà e assi. Collegare il Model assemblato e usare **Enabled** per spegnere tutte le preview gestite; **Display Settings** controlla le singole categorie, i risultati, le scale, l'ombreggiatura e la trasparenza. Il gestore sopprime automaticamente le anteprime del plugin a monte, senza cambiarne i flag Hidden. Il tasto destro offre **About**, **Guide / helper** e controlli di visibilità. `Model Physical Geometry` restituisce anche Brep/mesh Rhino. [Guida completa e limiti](docs/PREVIEW.md).

Nella **0.4** è corretta anche la conversione delle posizioni dei carichi distribuiti trave nei parametri nativi Straus7. Rigenerare esportazioni e risultati prodotti con versioni precedenti; il fingerprint distingue il nuovo contratto di export.

La **0.4.1** aggiunge l'uscita **Breps** al Display Manager e a Model Physical Geometry: volumi chiusi di travi, piastre con spessore e solidi FEM, con offset e vuoti delle sezioni. Collegarla a un parametro Brep oppure usare **Bake** dal menu del componente per creare solidi Rhino. Il bake funziona anche con preview spenta e conserva gli ID degli elementi. [Cast, bake e geometrie non convertibili](docs/PREVIEW.md#brep-e-bake-solido).

`Run Model Cases and Combinations` esporta e risolve i casi/combinazioni selezionati e restituisce **lo stesso tipo Model, con risultati immutabili incorporati**. Sono disponibili anche i solver Model per frequenze naturali, buckling e statica non lineare. Gli strumenti di allineamento lavorano sugli assi locali di travi/piastre; i lettori Model coprono nodi, travi, piastre, solidi e link. `Model Verification Data` espone geometrie, proprietà, materiali, unità, sezioni native e risultati per il verificatore futuro. [Flusso e contratto dati](docs/MODEL-RESULTS.md).

## Installazione

1. Usare Rhino **8.27 o successivo**, in modalità **.NET 8**, su Windows x64.
2. Eseguire `./build.ps1` da PowerShell con .NET SDK 8 o 9 installato.
3. Copiare il contenuto di `artifacts/Rhino2Straus` nella cartella `Rhino2Straus` dentro **Grasshopper → File → Special Folders → Components Folder**. Sbloccare i file scaricati dalle proprietà di Windows e riavviare Rhino.
4. Installare Straus7 R3 x64 con licenza API e le licenze solver necessarie. Se non viene rilevato, impostare `STRAUS7_API_PATH` alla cartella `Bin64` oppure al file `St7api.dll`, poi riavviare Rhino.

Il pacchetto distribuisce soltanto gli assembly del progetto: RhinoCommon, Grasshopper, DLL native Straus7 e manuali proprietari rimangono nelle rispettive installazioni. I GUID dei componenti Straus originali sono conservati: disabilitare il vecchio `Rhino2Fem.Grasshopper.Straus.gha` quando si installa questo plugin, per evitare registrazioni duplicate.

## Convenzioni

- Geometria, spessori, offset e dimensioni delle sezioni usano l'unità di lunghezza del Model; forze e carichi usano la sua unità di forza. Le coordinate Rhino non vengono convertite implicitamente in Build Model.
- Il modulo elastico e le tensioni lette dal solver sono in **kPa**; le resistenze nominali fy/fu/fck della libreria sono in **MPa**; densità in **tonnellate/m³**, masse nodali in tonnellate. La densità viene convertita durante l'export. Coefficiente di dilatazione in **1/°C**, convertito anche quando il Model usa °F.
- Gli angoli degli elementi Grasshopper sono in **gradi**; l'angolo degli assi della sezione generica e le rotazioni imposte sono in **radianti**. Gli strumenti di allineamento restituiscono radianti. Offset trave: componenti lungo gli assi principali 1–2, Z deve essere zero. L'inserimento della sezione si aggiunge all'offset esplicito. Offset piastra: somma di offset dell'elemento e della proprietà, lungo la normale positiva.
- I lettori diretti dei file usano i **numeri nativi Straus7**, da 1. I lettori `Model …` usano invece gli **ID del Model** e conservano anche il numero nativo nei record. `Read Straus Geometry` espone numeri e ID separatamente. Le colonne dei risultati sono descritte nei socket e conservano l'ordine API.
- Le trasformazioni FEM supportano spostamenti e rotazioni rigide. I carichi e i vincoli globali mantengono le direzioni globali. Per scale, specchiature e morph occorre ricreare gli elementi e assegnare le proprietà.

## Esportazione e analisi

`Save` oppure il fronte `Run=false → true` avvia l'export; la sola modifica degli input non scrive file. `Overwrite` è disattivato per default. L'export valida il modello, salva un file temporaneo e pubblica il `.st7` soltanto a operazione completa. L'output `Model path` collega l'export ai componenti solver.

Sono presenti statica lineare, frequenze naturali, buckling lineare e statica non lineare incrementale. Il buckling richiede risultati statici iniziali. La non lineare sostituisce la tabella degli incrementi non staged con una rampa del caso selezionato; le leggi materiali devono essere già definite nel file per abilitare la non linearità materiale. I solver sono sincroni e possono occupare Grasshopper durante il calcolo.

Le funzioni native richiedono una licenza API funzionante. Il codice compila e i test gestiti/geometrici sono separati dal collaudo Straus7: consultare [lo stato delle verifiche](docs/VALIDATION.md) prima dell'uso produttivo.

## Sviluppo

```powershell
dotnet build Rhino2Straus.sln -c Release
dotnet run --project Rhino2Straus.Tests -c Release
dotnet run --project tools/GeometrySmoke -c Release
dotnet run --project tools/GeometrySmoke -c Release -- --preview-gallery
dotnet run --project Rhino2Straus.Tests -c Release -- --native --solve
python tools/import_straus_sdk.py
python tools/catalogue.py
dotnet run --project tools/IconGallery -c Release -- .
```

Il test geometrico richiede Rhino installato e usa il resolver ufficiale Rhino.Inside; il test nativo richiede la licenza API Straus7. La CI esegue build e test gestiti. Il binding API proviene dalla parte Straus del progetto iniziale; i diritti dei rispettivi SDK rimangono ai produttori.
