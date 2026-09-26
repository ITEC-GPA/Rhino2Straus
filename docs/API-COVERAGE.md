# Copertura API Straus7 R3

La copertura riguarda il flusso FEM strutturale fondamentale. Il binding contiene molte altre funzioni del produttore; la presenza nel binding non implica l'esistenza di un componente o il collaudo della funzione.

| Area | Implementazione |
|---|---|
| Sessioni e file | Caricamento DLL x64, sessioni serializzate e chiuse, errori API, export atomico, import geometrico |
| Modello | Unità, saldatura nodi, ID, raccolta casi/proprietà, merge di Model, validazione |
| Nodi | Coordinate, vincoli 6 DOF, spostamenti imposti, forze/momenti, molle, masse, temperatura |
| Travi | Beam/Truss/Cable, sezioni tutte le tipologie standard R31, sei famiglie BGL, generiche numeriche e BXS, angolo, offset, inserimento, svincoli agli estremi |
| Piastre | Tri3/Quad4, spessore membranale/flessionale, offset, angolo; PlateShell/Membrane/ShearPanel/PlaneStress/PlaneStrain/Axisymmetric/LoadPatch |
| Solidi | Tet4/Wedge6/Hexa8, materiale isotropo, pressione normale per faccia; preview chiusa |
| Collegamenti | Rigidi XYZ/XY/YZ/ZX, pinned, master-slave, simmetria settoriale; connection beam con 6 rigidezze globali |
| Materiali | Isotropi elastici, librerie acciaio/calcestruzzo, proprietà termiche e smorzamento |
| Carichi | Nodi, distribuiti e concentrati trave, pressioni locali/globali/bordo piastra, gradienti termici, pretensioni/predeformazioni, peso proprio, combinazioni LSA |
| Organizzazione | Gruppi gerarchici, filtri, decompose e informazioni su oggetti |
| Analisi | Statica lineare con selezione casi/combinazioni e dipendenze, frequenze naturali, buckling lineare, non lineare statica non staged; flusso file oppure Model → Model con risultati |
| Risultati | Model.Results immutabile: casi, nodi, travi, piastre, solidi, link, tensioni/deformazioni e azioni, proprietà native di sezione, frequenze/partecipazione, fattori di buckling, estremi con caso/posizione, provenienza e rilevamento risultati obsoleti |
| Preview | Display Manager sul Model assemblato: geometria, sezioni/spessori fisici, carichi, offset, assi, etichette, proprietà, vincoli, svincoli, molle, masse, spostamenti imposti; interruttori per categoria e risultati, filtro casi, trasparenza, About e guida. Deformata e diagrammi trave nei componenti risultati |
| Brep e bake | Cast dei singoli elementi e albero dei Brep fisici chiusi dal Model; travi anche cave/composte, piastre con spessore, Tet4/Wedge6/Hexa8. Bake dal menu Grasshopper con attributi, ID e Undo; indipendente dalla preview e dalle API Straus7 |

[Gestione della preview](PREVIEW.md). [Sezioni R31, link, riferimenti SDK e default](SECTIONS-LINKS.md). [Flusso operativo e contratto per il verificatore](MODEL-RESULTS.md). Gli strumenti di allineamento coprono direzione, punto, piano di riferimento e Model assemblato.

## Limiti espliciti

- Import `.st7` di **geometria e identificativi**: non è un roundtrip completo di proprietà, vincoli e carichi. Gli elementi quadratici non vengono convertiti in elementi lineari in modo implicito.
- La preview offline delle sezioni standard calcola area e inerzie geometriche. J e aree di taglio vengono calcolati da Straus7 durante l'export; per leggerli prima si può usare una sezione BXS. La sezione generica può ricevere un profilo XY destinato alla preview, con rigidezze esplicitamente fornite.
- I tipi standard C e RHS dell'API richiedono flange simmetriche. Gli input asimmetrici non vengono esportati come una sezione differente: usare BXS oppure proprietà generiche calibrate. Per i raccordi sono disponibili i componenti BGL oppure BXS.
- Un solo freedom case, numero 1. Le molle, gli spostamenti imposti e i link fanno riferimento a questo caso. Coordinate dei vincoli e delle connection globali; UCS arbitrari non esposti.
- La saldatura per tolleranza unisce i nodi coincidenti: collegamenti a lunghezza zero con nodi volutamente duplicati non sono supportati in Build Model.
- Una sola definizione di peso proprio per Model. I Model con definizioni incompatibili non si uniscono automaticamente.
- Un gruppo nativo per elemento: con più gruppi, l'export assegna il primo. I gruppi rimangono tutti nel Model per filtri e organizzazione.
- Le pressioni sulle facce dei solidi e i carichi nodali dello stesso caso vengono sommati. Attributi piastra duplicati dello stesso tipo/caso/faccia, temperature o preload duplicati vengono segnalati: combinare esplicitamente il contributo prima dell'export.
- Quad4 non complanari oltre tolleranza, geometrie degeneri, unità incompatibili e offset assiali non supportati vengono rifiutati. La validazione geometrica dei solidi controlla connettività e volume orientato; non sostituisce un controllo completo dei Jacobiani di integrazione.
- Solver non lineare: rampa di un caso e geometria non lineare; configurazione di leggi plastiche, tabelle costitutive e costruzione staged non esposta. La casella di non linearità materiale usa le leggi già presenti nel file.
- Non esposti: transienti, risposta spettrale/armonica, contatto avanzato, fluidi/termica come solver, laminati, materiali ortotropi/anisotropi, automeshing e l'intera API di visualizzazione Straus7.
- I risultati non disponibili sono elencati in `Issues`. La lettura di proprietà/carichi da un file `.st7` esterno rimane parziale; `Read Results Into Model` associa risultati a un Model Rhino2Straus già definito, non ricostruisce un Model completo da un file arbitrario.
- Il confronto finale tra preview e orientamento delle sezioni native, nonché il collaudo completo di tutti i solver, richiede una licenza API operativa. Lo stato di verifica è separato dalla copertura implementata.

## Numerazione facce dei solidi

Ordine nodi secondo il manuale API R3; la pressione positiva entra nel solido.

| Tipo | Facce native, in ordine |
|---|---|
| Tet4 | 1-2-3; 4-1-3; 4-2-1; 4-3-2 |
| Wedge6 | 1-2-3; 5-2-1-4; 4-6-5; 4-1-3-6; 6-3-2-5 |
| Hexa8 | 1-2-3-4; 7-3-2-6; 6-5-8-7; 5-1-4-8; 8-4-3-7; 6-2-1-5 |

Fonte implementativa: *Straus7 R3 API Manual*, installato con Straus7, sezioni Model, Attributes, Properties, Solvers, Results e Element Connections. [Guida ufficiale alla migrazione API R3](https://www.strand7.com/r3/Strand7%20R24%20to%20R3%20API%20Conversion%20Guide.pdf). Il manuale proprietario non è incluso nel repository.
