# Esempi Grasshopper / Rhino2Straus

23 definizioni **con componenti reali già collegati**, geometria interna, slider, gruppi per argomento e note sul canvas. Aprire il file `.gh` in Grasshopper; il `.ghx` contiene la stessa definizione in formato XML.

Richiedono **Rhino 8 in .NET 8** e **Rhino2Straus 0.5.0**. Non servono altri plugin Grasshopper né un file Rhino di partenza. Impostare preferibilmente il documento Rhino in **metri**: tutti gli esempi usano metri, kN e °C; Build Model non riscalerebbe automaticamente coordinate e sezioni di un documento in millimetri.

| N. | Argomento | Componenti plugin | File |
|---:|---|---:|---|
| 01 | mensola offset bake | 19 | [.gh](01_mensola_offset_bake.gh) · [.ghx](01_mensola_offset_bake.ghx) |
| 02 | portale casi combo risultati | 35 | [.gh](02_portale_casi_combo_risultati.gh) · [.ghx](02_portale_casi_combo_risultati.ghx) |
| 03 | piastra mesh pressione | 17 | [.gh](03_piastra_mesh_pressione.gh) · [.ghx](03_piastra_mesh_pressione.ghx) |
| 04 | frame polilinea assi svincoli | 19 | [.gh](04_frame_polilinea_assi_svincoli.gh) · [.ghx](04_frame_polilinea_assi_svincoli.ghx) |
| 05 | nodi vincoli molle masse temperatura | 17 | [.gh](05_nodi_vincoli_molle_masse_temperatura.gh) · [.ghx](05_nodi_vincoli_molle_masse_temperatura.ghx) |
| 06 | link rigidi elastici master slave | 21 | [.gh](06_link_rigidi_elastici_master_slave.gh) · [.ghx](06_link_rigidi_elastici_master_slave.ghx) |
| 07 | mesh tri quad proprieta assi | 15 | [.gh](07_mesh_tri_quad_proprieta_assi.gh) · [.ghx](07_mesh_tri_quad_proprieta_assi.ghx) |
| 08 | solidi tet wedge hexa pressione | 22 | [.gh](08_solidi_tet_wedge_hexa_pressione.gh) · [.ghx](08_solidi_tet_wedge_hexa_pressione.ghx) |
| 09 | materiali sezioni base generica | 45 | [.gh](09_materiali_sezioni_base_generica.gh) · [.ghx](09_materiali_sezioni_base_generica.ghx) |
| 10 | sezioni native standard | 45 | [.gh](10_sezioni_native_standard.gh) · [.ghx](10_sezioni_native_standard.ghx) |
| 11 | sezioni native bgl | 33 | [.gh](11_sezioni_native_bgl.gh) · [.ghx](11_sezioni_native_bgl.ghx) |
| 12 | sezione bxs profilo bake | 12 | [.gh](12_sezione_bxs_profilo_bake.gh) · [.ghx](12_sezione_bxs_profilo_bake.ghx) |
| 13 | modello gruppi filtri ricostruzione | 23 | [.gh](13_modello_gruppi_filtri_ricostruzione.gh) · [.ghx](13_modello_gruppi_filtri_ricostruzione.ghx) |
| 14 | carichi travi peso proprio | 22 | [.gh](14_carichi_travi_peso_proprio.gh) · [.ghx](14_carichi_travi_peso_proprio.ghx) |
| 15 | carichi piastre termici precarico | 19 | [.gh](15_carichi_piastre_termici_precarico.gh) · [.ghx](15_carichi_piastre_termici_precarico.ghx) |
| 16 | export import model esistente | 22 | [.gh](16_export_import_model_esistente.gh) · [.ghx](16_export_import_model_esistente.ghx) |
| 17 | analisi modale | 18 | [.gh](17_analisi_modale.gh) · [.ghx](17_analisi_modale.ghx) |
| 18 | analisi buckling | 19 | [.gh](18_analisi_buckling.gh) · [.ghx](18_analisi_buckling.ghx) |
| 19 | analisi non lineare | 18 | [.gh](19_analisi_non_lineare.gh) · [.ghx](19_analisi_non_lineare.ghx) |
| 20 | risultati nodi travi link | 31 | [.gh](20_risultati_nodi_travi_link.gh) · [.ghx](20_risultati_nodi_travi_link.ghx) |
| 21 | risultati piastre solidi | 33 | [.gh](21_risultati_piastre_solidi.gh) · [.ghx](21_risultati_piastre_solidi.ghx) |
| 22 | file esistenti solver risultati | 20 | [.gh](22_file_esistenti_solver_risultati.gh) · [.ghx](22_file_esistenti_solver_risultati.ghx) |
| 23 | rilettura risultati nel model | 19 | [.gh](23_rilettura_risultati_nel_model.gh) · [.ghx](23_rilettura_risultati_nel_model.ghx) |

Tutti i **164 componenti** compaiono in almeno un esempio: [indice per componente](COMPONENTS.md) e [copertura JSON](component-coverage.json). La presenza nella definizione non indica che la relativa API sia stata eseguita: i rami nativi aspettano una licenza e l'attivazione dei toggle.

Gli ingressi lista di **Build Model sono impostati su Flatten** per ottenere un solo Model anche quando piastre, nodi e altri elementi arrivano da rami diversi. Non collegare contemporaneamente un elemento originale e una sua copia caricata; concatena i componenti di attributo e assembla soltanto l'ultima uscita.


## 01 — Mensola con offset e bake solido

Il flusso è `Steel + Rectangular Section → Frame Property → Frame Element → Beam Offset → Align Beam Axis → Build Model`. Al Model arrivano anche il nodo incastrato e il nodo libero caricato.

- Mensola da `(0,0,0)` a `(4,0,0)`, acciaio S355, rettangolo iniziale 0.30 × 0.20 m.
- Incastro: tutte le sei componenti bloccate al nodo iniziale. Caso `Q`: forza iniziale `Fz=-10 kN` all'estremo libero.
- Offset `(0.05,0,0)` negli assi principali della sezione; asse 1 orientato verso Z globale. L'allineamento viene applicato **dopo** Beam Offset, che imposta anche l'angolo.
- Slider per altezza, larghezza e forza. Per cambiare l'offset: tasto destro sull'input Offset del componente e modifica del vettore persistente.

Collegamenti finali: Validate, Summary, Decompose, Display Settings, Display Manager e Model Physical Geometry. L'uscita **Breps** arriva a un parametro Brep: tasto destro → **Bake**. Si ottiene un solido chiuso; ai valori iniziali il volume è **0.24 m³**. Non viene effettuata un'unione booleana con altri elementi.

## 02 — Portale, merge, casi/combinazioni e lettura risultati

Il portale è largo 6 m e alto 3 m, con due incastri di base. Usa S355 e una sezione I parametrica: altezza 0.30 m, flange larghe 0.20 m e spesse 0.01 m, anima spessa 0.01 m. Lo slider varia l'altezza.

`Build Model A` assembla pilastri e incastri; `Build Model B` assembla trave, carichi e combinazioni. **Merge Straus Models**, con `Renumber=true`, salda i nodi comuni e restituisce **un solo Model**.

Sulla trave sono applicati in sequenza `G=-5 kN/m` e `Q=-3 kN/m`, entrambi lungo Z globale. Build riceve soltanto l'ultima copia dell'elemento, contenente entrambi i carichi. `G` è un carico assegnato, **senza peso proprio automatico**.

Le combinazioni dimostrative sono `ULS_demo = 1.35 G + 1.50 Q` e `SLS_demo = G + Q`. I coefficienti mostrano l'uso dei componenti; vanno adattati al proprio progetto.

Per eseguire il calcolo:

1. Verifica che il preflight sia `True` e controlla gli input.
2. Nel pannello **Percorso .st7 modificabile**, scegli una cartella locale scrivibile e un nome `.st7` disponibile. Il percorso iniziale è `C:\Temp\Rhino2StrausExamples\02_portale.st7`.
3. Con Straus7 R3 e licenze API/solver disponibili, cambia **Run analisi** da `False` a `True`. `Overwrite` è inizialmente `False`; abilitalo solo se vuoi sostituire i file di un calcolo precedente.
4. Leggi **Stato analisi**, **Quantità non disponibili** e `Model Result Cases`. I filtri casi/combinazioni vuoti includono tutti.
5. Il Model in uscita contiene i risultati e alimenta azioni N/V/T/M, inviluppo M2, deformata, diagramma M2 e dati per il verificatore. I numeri dei casi si leggono da `Numbers` nel lettore casi. Actions usa inizialmente `Case=0` (tutti); deformata e diagramma usano `Case=1`.

Lo **Stream Gate** riceve lo stesso toggle Run: con `False` tiene vuoto l'ingresso dei lettori. I sei lettori e i due pannelli associati possono quindi apparire arancioni all'apertura con avviso di dati mancanti: è lo stato di attesa previsto. Mantieni `Run=True` per leggerli dopo il calcolo; dopo modifiche al modello usa nuovamente `False → True`. Prima di salvare una tua copia da riaprire, riporta **Run=False** per evitare un nuovo avvio automatico alla riapertura.

Il diagramma mostra M2 lungo l'asse di visualizzazione 1, nel piano del portale; la deformata ha scala 50. La preview dei risultati è abilitata in Display Settings, ma può essere spenta indipendentemente o tramite l'interruttore generale.

Ai valori iniziali: **3 Brep chiusi**, volume complessivo **0.0816 m³**. Il file distribuito non contiene risultati precalcolati o sintetici. La catena nativa Straus è predisposta ma non è stata eseguita per validare questo esempio.

## 03 — Piastra da mesh con pressione e assi locali

Una mesh interna con quattro facce quadrangolari descrive una piastra di 4 × 3 m. Il flusso è `Concrete + Area Thickness → Area Property → Mesh to Plates → Plate Offset → Align Plate Axis → Global Pressure → Build Model`.

- Calcestruzzo C30/37, spessore iniziale 0.20 m.
- Normali dei Quad4 verso +Z, offset +0.10 m: i volumi fisici occupano Z da 0 a 0.20 m.
- Assi 1 delle piastre allineati a `(1,1,0)`. La pressione è il vettore **globale** `(0,0,-5)` kN/m², quindi resta verticale cambiando gli assi locali.
- Otto nodi di bordo incastrati in tutti i gradi di libertà; il nodo centrale è libero.
- Slider per spessore e offset. Per modificare la pressione o l'orientamento, cambia i vettori interni nei relativi parametri.

Build salda i nodi condivisi: **9 nodi e 4 piastre**. La preview produce **4 Brep chiusi**, volume totale **2.4 m³**. La mesh grossolana serve a mostrare il flusso: per un'analisi occorre infittirla e verificare convergenza e vincoli.

## Esempi 04–23: percorso operativo

### 04 frame polilinea assi svincoli

Una polilinea genera tre travi. Sezione con punto d'inserimento, asse locale verso un punto, svincolo torsionale finale e cambio proprieta'.
Beam Behaviour permette Beam / Truss / Cable. Qui rimane Beam: Truss e Cable non sostengono flessione.

### 05 nodi vincoli molle masse temperatura

Attributi in sequenza sullo stesso nodo: cedimento imposto, molle, masse, temperatura e forza.
Build riceve solo l'ultima copia: collegare copie intermedie sommerebbe nuovamente i contributi. Le rotazioni sono in radianti.

### 06 link rigidi elastici master slave

Confronto dei cinque tipi: rigid, pinned, master-slave, sector symmetry e connessione elastica.
Le opzioni controllano piano rigido, asse di simmetria e sei relazioni DoF (libero / uguale / opposto).
Le rigidezze elastiche sono negli assi globali. Ogni coppia ha il primo nodo bloccato. Esempio di assegnazione: non ogni configurazione e' stabile per ogni solver.

### 07 mesh tri quad proprieta assi

Una mesh mista produce due Tri3 e un Quad4; un'altra faccia usa Area Element.
Riassegnazione della proprieta', allineamento a un piano e lettura degli assi. Invertire l'ordine dei vertici cambia normale e lato dei carichi.

### 08 solidi tet wedge hexa pressione

Tet4, Wedge6 e Hexa8 costruiti con nodi ordinati secondo Straus.
Pressione su una faccia, incastro dei nodi a Z=0, controllo dei volumi e Brep chiusi per bake.
Volumi attesi: 1/6, 1/2 e 1 m3. Numerazione delle facce nativa: controllare la preview prima del calcolo.

### 09 materiali sezioni base generica

Otto tipologie geometriche e una sezione generica con contorno esplicito per la preview.
Material custom con resistenze nominali assegnate separatamente (MPa). Le resistenze non modificano il modulo elastico.
La sezione generica e' un quadrato 0.1 x 0.1 m: i valori numerici e il contorno devono essere coerenti; il profilo non ricalcola la rigidezza.

### 10 sezioni native standard

Sezioni native Straus: cruciforme, canale labbrato, T labbrato, top-hat, trapezio e triangolo pieni/cavi, Z.
Native Section Options controlla discretizzazione e taglio. API necessaria: Genera profili e' False all'apertura.

### 11 sezioni native bgl

Sezioni BGL: angolare, bulb, canale, I, RHS e T. Le dimensioni sono negli input del componente.
Richiede API Straus: abilita Genera profili per calcolare rigidezze e contorni. Ogni sezione alimenta una trave e un solo Model.

### 12 sezione bxs profilo bake

Importazione di una sezione .bxs esistente. Imposta il percorso, poi Leggi BXS=True.
Il componente conserva i dati BXS nel Model per l'export. Section Info espone contorno e proprieta'.
Il profilo viene estruso lungo una trave; Display Manager espone Breps per bake manuale.

### 13 modello gruppi filtri ricostruzione

Gruppi gerarchici, filtri per posizione/proprieta'/gruppo, decomposizione e ricostruzione.
I filtri combinano i criteri con OR. Un solo Model finale. Per un import collegare anche Preserve nodes e Self weight.
Merge salda le parti create; se una parte viene da import, conserva i nodi distinti.

### 14 carichi travi peso proprio

Carichi concatenati: forza e momento concentrati, distribuzione lineare parziale, gradiente termico e pretensione.
Posizioni relative da nodo 1: 0..1. Carichi locali lungo assi principali; carichi globali indipendenti dalla rotazione della sezione.
Peso proprio G separato dai carichi Q. Tutti gli attributi rimangono nel Model unico.

### 15 carichi piastre termici precarico

Carico normale alla piastra, pressione globale, pressione su bordo, gradiente termico e predeformazione nel piano.
I carichi vengono applicati in sequenza sulla stessa piastra. Il filtro finale mostra la selezione per proprieta'.
Normal pressure e Global pressure sono attributi diversi: qui vengono sommati intenzionalmente per illustrare il flusso.

### 16 export import model esistente

1. Il modello dimostrativo puo' essere esportato con Run. 2. Read Straus Model legge quel file o un altro .st7.
3. Controlla Complete, Report e Identity map: il Model contiene geometrie, proprieta', carichi e attributi supportati.
Il file sorgente non viene salvato. L'import normalizza in metri e kN; Allow partial=False blocca il Model in presenza di omissioni.
La lettura geometrica e il bake diretto dal file sono alternative. Ogni azione ha un interruttore indipendente.

### 17 analisi modale

Mensola con massa strutturale: Run Model Natural Frequencies esporta, esegue e incorpora i risultati.
Imposta percorso e numero modi. Le ampiezze modali dipendono dalla normalizzazione Straus: non sono spostamenti sotto Q.
Il Model contiene frequenze, masse modali, casi e forme deformate; tutti i comandi sono inizialmente False.

### 18 analisi buckling

Mensola compressa. Prima esegui statica; poi abilita il solver buckling che riceve Initial LSA dalla statica.
Variable result case=1, Fixed result case=0. Il fattore critico moltiplica il carico di riferimento.
Usa lo stesso modello di input nei due solver. Il cambio dei dati richiede una nuova statica. Le forme sono autovettori normalizzati.

### 19 analisi non lineare

Analisi statica con non linearita' geometrica della mensola; Q viene applicato in dieci incrementi.
Nonlinear geometry=True, materiale elastico. Non e' una legge plastica del materiale.
Model Result Cases espone gli incrementi disponibili; leggere valori e diagrammi per il caso desiderato.

### 20 risultati nodi travi link

Tutti i lettori Model per nodi, travi e link. Prima avvia la statica.
Case=0 legge tutti i casi; IDs vuoto legge tutti gli elementi. Records conserva caso, ID, numero nativo, assi, colonne e campionamento.
Values usa rami per caso/elemento/superficie. Controlla Columns e Unavailable: una quantita' non disponibile non equivale a zero.

### 21 risultati piastre solidi

Model misto: una piastra Quad4 e un Hexa8 con appoggi e pressione.
Lettura di forze, momenti, curvature, tensioni, deformazioni, reazioni e spostamenti nodali per elemento.
Le famiglie di risultati seguono i campionamenti e gli assi dichiarati nei Records. Controlla il lato/superficie delle piastre.

### 22 file esistenti solver risultati

Lettori diretti di file Straus: .st7 e risultati gia' calcolati (.LSA / .NFA / .LBA / .NLA).
Scegli file corrispondenti e numeri nativi degli elementi, non le label. Leggi=True apre i lettori; non avvia un solver.
La zona inferiore mostra i solver su file esistente, con pulsanti separati. Non sovrascrivere un file di produzione per provare l'esempio.
La geometria deformata usa punti e spostamenti con la stessa sequenza di numeri nodo.

### 23 rilettura risultati nel model

Read Results Into Model ricollega un file risultati al suo Model di input.
La statica di questo esempio crea .st7, .LSA e manifest; Read li rilegge verificando identita' e provenienza.
Per file esterni creati senza Rhino2Straus usare l'esempio 22; il nuovo import strutturale richiede riesportazione e ricalcolo per ottenere questa provenienza.
Il risultato rimane nel Model per il futuro verificatore; una modifica strutturale invalida i vecchi risultati.

Per **10–12**, impostare file/opzioni e attivare il toggle API: i profili nativi vengono calcolati da Straus. Per **16**, leggere il [contratto di import](../docs/MODEL-IMPORT.md), poi controllare Report e Complete. Il bake diretto dal file e il bake Brep dal Model sono operazioni separate.

Per **17–21**, impostare un percorso scrivibile e attivare Run; in **18** eseguire prima la statica, poi il buckling. I lettori ricevono dati attraverso uno Stream Gate: con Run=False gli avvisi arancioni di input mancante sono previsti. **22** legge file già calcolati e richiede numeri nativi degli elementi presenti nel file. **23** verifica la rilettura nel Model con identità e manifest.

Le mesh degli esempi sono piccole e dimostrative; verificare discretizzazione, vincoli, stabilità e convergenza per un progetto reale. Gli esempi di sezioni e link mostrano assegnazioni e preview, senza promettere stabilità di ogni alternativa cambiando i parametri.

## Preview, parametri e geometria interna

In tutti gli esempi **Preview ON/OFF** controlla Display Settings e Display Manager. Le categorie includono sezioni, carichi, vincoli, offset e assi; `Values` è inizialmente spento per evitare testi sovrapposti. Abilitalo per leggere i valori. I parametri geometrici di ingresso e il parametro Brep finale hanno la preview propria nascosta: il gestore controlla la scena. Il bake resta manuale e disponibile anche con preview spenta.

I valori non collegati sono preimpostati nei socket: tasto destro sull'input per modificarli oppure collega uno slider, un toggle o un pannello. Curve, punti e mesh sono già internalizzati; puoi sostituirli con geometrie Rhino. Mantieni coerenti i punti dei vincoli con gli estremi degli elementi. La mesh della piastra ha i punti di bordo internalizzati separatamente: se cambi topologia, aggiorna anche i vincoli.

## Verifica e rigenerazione

[validation.json](validation.json) riporta versioni runtime, conteggi, collegamenti, volumi, avvisi e hash SHA-256. Il generatore verifica inoltre che nessuna libreria API Straus sia stata caricata durante la generazione. Salva entrambi i formati, li rilegge e ricalcola i grafi collegati. Per i 18 esempi con Model offline controlla preflight, Brep chiusi, preview ON/OFF senza alterare il Model, geometria disponibile a preview spenta e toggle di analisi inizialmente falsi.

Dalla radice del repository, con Rhino 8 installato:

```powershell
./build.ps1
dotnet run --project tools/Examples -c Release
```

Il parametro `-- "--rhino=C:\Program Files\Rhino 8\System"` permette di indicare la cartella del runtime. La generazione usa un registro Grasshopper isolato e non carica altri plugin installati. Non avvia Straus, non fa bake e non modifica il documento Rhino aperto dell'utente.

La validazione non esegue import/export/solver nativi: la licenza locale restituisce errore -8. Le definizioni non contengono risultati FEM sintetici o precalcolati. I file aggiuntivi storici o nella cartella tutorials non fanno parte della suite di 23 definizioni descritta in validation.json.
