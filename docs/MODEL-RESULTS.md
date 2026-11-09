# Model, solver e risultati

## Flusso Grasshopper

1. Creare nodi, travi, piastre, solidi e collegamenti; assegnare materiali, proprietà, carichi, vincoli, offset e gruppi.
2. Usare `Align Beam Local Axis`, `Align Beam Axis To Point`, `Align Plate Local Axis` o `Align Plate Axis To Plane` quando serve controllare l'orientamento. La direzione viene proiettata nel piano trasversale della trave o nel piano della piastra. `Align Model Local Axes` opera sul modello assemblato. Una direzione parallela all'asse trave o alla normale piastra viene rifiutata.
3. Unire tutto con `Build Model` / `Merge Straus Models`. Rimane **un solo Model**. Controllare `Validate Straus Model` e `Preview Straus Model`.
4. Collegare quel Model a `Run Model Cases and Combinations`, indicare un percorso `.st7` e attivare `Run` con un pulsante o un fronte false → true. `Overwrite` consente di sostituire file esistenti.
5. Collegare il **Model in uscita dal solver** ai componenti `Model …` dei risultati, alla deformata, ai diagrammi e al futuro verificatore.

Le liste vuote selezionano tutti i casi/combinazioni della famiglia abilitata. `Run cases=false` e `Run combinations=true` consente di richiedere solo le combinazioni, ma il solver calcola anche i casi da cui dipendono. I nomi errati producono un errore. Le combinazioni vengono rigenerate dai fattori del Model; non viene riutilizzato un vecchio `.LSC`.

`Run Model Natural Frequencies`, `Run Model Nonlinear Static` e `Run Model Linear Buckling` seguono lo stesso percorso e restituiscono un Model con risultati. La non lineare applica una rampa di un caso con materiale elastico e, opzionalmente, non linearità geometrica. Il buckling richiede risultati LSA verificati dello stesso modello e conserva la selezione delle combinazioni iniziali. Usare percorsi distinti per analisi diverse se si vuole conservare la coppia originale `.st7` / risultati senza invalidarne il manifest.

Gli otto componenti solver sono asincroni durante il calcolo Straus7: il processo solver esterno lavora mentre Grasshopper rimane utilizzabile. Export, preparazione e lettura dei risultati restano sul thread UI e possono richiedere tempo su modelli grandi. I lavori sono serializzati nell'intero processo Rhino; altre operazioni native Rhino2Straus segnalano `API busy` senza attendere. Ripeterle al termine del calcolo. Le operazioni offline non richiedono l'API. Se l'API non riesce a determinare lo stato del processo, un avviso riporta l'errore e l'API rimane riservata finché Straus7 non conferma la terminazione; non si presume concluso un solver non interrogabile.

`Run` accetta un fronte false → true. Rilasciare il pulsante non annulla l'analisi; tenerlo True non genera nuovi lavori. Gli stati sono `Queued`, `Running`, `Completed` (eventualmente con quantità non disponibili), `Failed` e `Run required`. Un nuovo fronte durante un lavoro ancora valido non lo duplica. Input e Model vengono acquisiti prima dell'avvio; il Model viene copiato e i risultati preesistenti eliminati dall'output in attesa.

Una modifica del Model o dei parametri invalida l'operazione: serve un nuovo fronte Run. Il risultato tardivo viene scartato anche nella sequenza A → B → A; la rimozione del componente, gli input mancanti e la chiusura del documento ritirano il relativo lavoro. Dopo il ripristino degli input o lo sblocco/riapertura del documento occorre osservare Run=False e poi Run=True: un toggle rimasto True non rilancia il solver. I lavori in coda vengono annullati; un solver già avviato termina normalmente prima di liberare l'API. **I suoi file possono restare sul disco**, anche se il risultato non viene pubblicato in Grasshopper. Non è prevista l'interruzione forzata del processo solver.

I solver su file verificano esistenza, dimensione e ultima modifica degli input prima di eseguire un lavoro rimasto in coda. Gli output in cache vengono invalidati se cambiano i file associati (controllo periodico di circa un secondo). È un controllo tramite metadati, non un hash continuo: modifiche esterne che conservano sia dimensione sia data non vengono rilevate dalla cache. La rilettura dei risultati Model conserva la verifica SHA-256 del manifest. Usare percorsi distinti per analisi da conservare.

I dati calcolati rimangono nella sessione Grasshopper; per rileggerli dopo la riapertura usare `Read Results Into Model`.

## Assi e preview

Le direzioni dei carichi locali, gli offset principali e gli svincoli locali seguono gli assi riallineati. L'operazione conserva geometria e normale delle piastre; non ribalta la connettività. Gli angoli restituiti dagli strumenti di allineamento sono in radianti. L'allineamento di una sezione può riferirsi agli assi principali oppure agli assi geometrici del profilo.

`Model Deformed Geometry` mostra la geometria di analisi spostata, con fattore di amplificazione visuale. Le travi usano gli spostamenti alle stazioni, quando disponibili; altrimenti gli estremi nodali. Spessori e offset fisici non vengono ricostruiti sulla deformata. `Model Beam Force Diagram` disegna N, V1, V2, T, M1 o M2 lungo l'asse principale scelto.

## Quantità dentro Model.Results

| Elementi | Quantità lette |
|---|---|
| Nodi | Spostamenti e rotazioni; reazioni di forza e momento |
| Travi | Forze interne; tensioni complete; deformazioni elastiche e totali; spostamenti; reazioni nodali |
| Piastre | Forze membranali/tagli, momenti, curvature, tensioni, deformazioni elastiche/totali, spostamenti e reazioni nodali |
| Solidi | Tensioni, deformazioni elastiche/totali, spostamenti e reazioni nodali |
| Link rigidi e connection | Spostamenti e reazioni alle estremità, in assi globali |
| Modale | Frequenze, massa/rigidezza/smorzamento modale, partecipazioni di massa e modali |
| Buckling | Fattori critici per modo |
| Sezioni trave | Proprietà native effettive: A, I11, I22, J, aree/centri di taglio, baricentro e angolo principale |

Le tensioni e deformazioni piastra vengono lette al piano medio e sulle due superfici. Il campionamento predefinito usa punti di Gauss; sono disponibili baricentro o nodi con/senza media. Le stazioni trave sono distanze dall'estremo 1; Straus7 può aggiungere stazioni per cogliere estremi. Non confondere l'indice dei campioni di una piastra/solido con una coordinata geometrica.

La disponibilità effettiva dipende da solver, tipo di elemento e sezione. Gli errori di una quantità sono memorizzati in `Issues`, con caso, elemento, superficie e messaggio API. **Non sono sostituiti da zeri.** `IsComplete` significa che tutte le quantità richieste sono state lette; non certifica la validità ingegneristica del modello. Le forme modali e di buckling sono normalizzate secondo Straus7, non sono spostamenti sotto un carico fisico.

## Identificativi e alberi dati

I componenti che leggono direttamente file usano i numeri nativi Straus7. I nuovi componenti `Model …` usano gli **ID del Model**. Ogni record conserva entrambi. La lettura ricostruisce la corrispondenza mediante ID, coordinate e connettività, senza presumere che ID e posizione nel file coincidano.

- `Values`: percorsi `{caso; ID elemento; superficie; campione}`, valori nell'ordine di `Columns`.
- `Positions`, `Columns`, `Axes and sampling`: percorsi `{caso; ID elemento; superficie}`.
- `Model Beam Actions N V T M`: un'uscita per N, V1, V2, T, M1, M2 e posizioni, percorsi `{caso; ID trave}`.
- Superfici: 0 piano medio, 1 −Z, 2 +Z. Nei filtri −1 seleziona tutte.
- Caso 0 nei lettori significa tutti. Gli altri numeri sono **numeri di caso risultato**, da recuperare con `Model Result Cases`; non sono gli ID dei load case.

`Model Result Extrema` restituisce minimo e massimo firmati con caso e posizione governanti. Gli estremi di quantità differenti non rappresentano necessariamente uno stato di carico simultaneo.

## Unità per il verificatore

Forze nell'unità del Model; momenti in forza × lunghezza; spostamenti nell'unità di lunghezza; rotazioni in radianti. Tensioni e moduli elastici sono in **kPa**. Le proprietà nominali di resistenza `TensileYeld`, `TensileStrength` e `CompressiveStrength` conservano la convenzione della libreria in **MPa**: i getter `YieldStressKPa`, `UltimateStressKPa` e `CompressiveStressKPa` danno il valore convertito. Zero indica un dato di resistenza non definito.

Le forze piastra sono per unità di lunghezza, i momenti piastra per unità di larghezza; la convenzione tensoriale e i segni rimangono quelli dell'API. Le colonne vengono etichettate, senza riordinare implicitamente i tensori. Le proprietà di sezione native sono nelle potenze appropriate dell'unità di lunghezza. Le resistenze nominali sono metadati per verifiche successive: non impostano leggi plastiche nel solver.

## Collegamento al futuro verificatore

`Model Verification Data` restituisce elementi, proprietà, materiali e record raggruppati per `{tipo elemento; ID}`. Tipi: Node=0, Beam=1, Plate=2, Solid=3, Link=4. Include unità, quantità mancanti e sezioni native. `Decompose Model` conserva le prime cinque uscite e aggiunge materiali, sezioni, proprietà, casi, combinazioni, gruppi, unità e risultati.

Il contratto C# centrale è `ModelModel.Results`, di tipo `AnalysisResultsModel`. I record `ElementResult` sono immutabili e contengono identità, caso, quantità, assi, superficie, campionamento, colonne, posizioni e righe. Il verificatore deve richiedere le quantità che gli servono, considerare gli `Issues` pertinenti e chiamare `ValidateFor(model, requireProvenance: true)` prima di usarle. Nessun componente implementa ancora verifiche normative.

L'impronta del modello include geometria, proprietà, materiali, carichi, casi, combinazioni e unità. I risultati restano associati alla loro impronta: una modifica rende le query non valide; Build/Merge/riallineamento del Model eliminano i risultati. La copia di un Model invariato li conserva.

Ogni esecuzione integrata produce un manifest `<risultato>.rhino2straus.json` con gli hash SHA-256 del `.st7` e del file dei risultati. `Read Results Into Model` richiede che entrambi corrispondano. `Allow unverified=true` permette la lettura di un file risolto esternamente, purché il `.st7` conservi l'identità del Model e la geometria corrisponda; marca esplicitamente la provenienza come non verificata. `Model Verification Data` la rifiuta per default. I manifest verificano l'associazione dei file, non sostituiscono un controllo di convergenza o una validazione strutturale.
