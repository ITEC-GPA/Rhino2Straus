# Esempi Grasshopper / Rhino2Straus

Tre definizioni **con componenti reali già collegati**, geometria interna, slider, gruppi per argomento e note sul canvas. Aprire il file `.gh` in Grasshopper; il `.ghx` contiene la stessa definizione in formato XML.

Richiedono **Rhino 8 in .NET 8** e **Rhino2Straus 0.4.3**. Non servono altri plugin Grasshopper né un file Rhino di partenza. Impostare preferibilmente il documento Rhino in **metri**: tutti gli esempi usano metri, kN e °C; Build Model non riscalerebbe automaticamente coordinate e sezioni di un documento in millimetri.

| Esempio | Componenti Rhino2Straus | Modello iniziale | File |
|---|---:|---|---|
| Mensola, offset e bake | 19 | 2 nodi, 1 trave, 1 caso | [Apri .gh](01_mensola_offset_bake.gh) · [XML .ghx](01_mensola_offset_bake.ghx) |
| Portale, casi, combo e risultati | 35 | 4 nodi, 3 travi, 2 casi, 2 combo | [Apri .gh](02_portale_casi_combo_risultati.gh) · [XML .ghx](02_portale_casi_combo_risultati.ghx) |
| Piastra, mesh e pressione | 17 | 9 nodi, 4 Quad4, 1 caso | [Apri .gh](03_piastra_mesh_pressione.gh) · [XML .ghx](03_piastra_mesh_pressione.ghx) |

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

## Preview, parametri e geometria interna

In tutti gli esempi **Preview ON/OFF** controlla Display Settings e Display Manager. Le categorie includono sezioni, carichi, vincoli, offset e assi; `Values` è inizialmente spento per evitare testi sovrapposti. Abilitalo per leggere i valori. I parametri geometrici di ingresso e il parametro Brep finale hanno la preview propria nascosta: il gestore controlla la scena. Il bake resta manuale e disponibile anche con preview spenta.

I valori non collegati sono preimpostati nei socket: tasto destro sull'input per modificarli oppure collega uno slider, un toggle o un pannello. Curve, punti e mesh sono già internalizzati; puoi sostituirli con geometrie Rhino. Mantieni coerenti i punti dei vincoli con gli estremi degli elementi. La mesh della piastra ha i punti di bordo internalizzati separatamente: se cambi topologia, aggiorna anche i vincoli.

## Verifica e rigenerazione

[validation.json](validation.json) riporta versioni runtime, conteggi, collegamenti, volumi, avvisi e hash SHA-256. Il generatore salva entrambi i formati, li rilegge e ricalcola i grafi collegati. Controlla preflight, Brep chiusi, preview ON/OFF senza alterare il Model, geometria disponibile a preview spenta e toggle di analisi inizialmente falsi.

Dalla radice del repository, con Rhino 8 installato:

```powershell
./build.ps1
dotnet run --project tools/Examples -c Release
```

Il parametro `-- "--rhino=C:\Program Files\Rhino 8\System"` permette di indicare la cartella del runtime. La generazione usa un registro Grasshopper isolato e non carica altri plugin installati. Non avvia Straus, non fa bake e non modifica il documento Rhino aperto dell'utente.
