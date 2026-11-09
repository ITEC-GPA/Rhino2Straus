# Importare un file Straus nel Model Grasshopper

La versione **0.5.0** aggiunge **Read Straus Model** nel pannello **10-File Straus**. Il componente restituisce lo stesso `Model` usato da Build, Merge, Display Manager, export e solver. [Esempio collegato 16](../examples/16_export_import_model_esistente.gh) · [Tutti gli esempi](../examples/README.md).

## Uso

1. Collega il percorso di un file `.st7` a **Model path**.
2. Seleziona il **Freedom case** dei vincoli (default 1); **Tolerance** è in metri (default 0.001).
3. Porta **Read** da False a True. Una successiva modifica degli input invalida la copia in cache; per rileggere usa nuovamente False → True.
4. Controlla **Complete**, **Report** e **Identity map** prima di usare il Model. Con **Allow partial=False**, un dato strutturale non rappresentabile impedisce l'uscita del Model; il rapporto resta disponibile. True consente esplicitamente una copia con le omissioni indicate.
5. Collega **Model** al Display Manager o a Decompose Model. Per ricostruire dopo una modifica, collega anche **Units**, **Self weight**, **Load cases**, **Combinations** e **Preserve nodes** a Build Model.

Il sorgente viene copiato in un file temporaneo, convertito con `St7ConvertUnits`, letto e chiuso senza salvarlo. L'originale non viene sovrascritto. Il Model restituito usa **m, kN, kPa, tonne, °C e kJ**; non basta cambiare l'etichetta delle unità. Anche i valori termici e le temperature di riferimento vengono conservati nelle unità convertite. La licenza Straus7 R3 API è necessaria.

## Dati trasferiti

| Famiglia | Supporto |
|---|---|
| Nodi | Coordinate, connettività, gruppi; ID del Model uguale al numero nativo, label originale nell'Identity map |
| Vincoli | Un freedom case selezionato, sei restrizioni e spostamenti/rotazioni imposti in UCS globale |
| Attributi nodali | Molle e masse diagonali globali, forze/momenti, temperature reference/fixed/initial |
| Travi | Beam2 di tipo Beam, Truss o Cable; orientamento principale, offset, svincoli booleani |
| Proprietà trave | Materiale elastico isotropo, area, I11/I22/J, aree e offset di taglio, baricentro e angolo della sezione; profilo per preview quando estraibile |
| Piastre | Tri3/Quad4, formulazioni isotrope disponibili nel plugin, spessori membranale/flessionale anche assegnati all'elemento, assi e offset |
| Solidi | Tet4/Wedge6/Hexa8 isotropi e pressioni normali per faccia |
| Link | Rigid, pinned, master-slave e sector symmetry; connection beam con sei rigidezze e UCS globale |
| Carichi trave | Forze/momenti puntuali, distribuiti nelle sei forme R31 supportate, proiezione, gradienti e preload |
| Carichi piastra | Pressione normale sulla faccia -Z, pressione globale sulle due facce, pressione normale sul bordo, gradiente e preload |
| Casi | Nomi, temperatura di riferimento, casi inutilizzati, un peso proprio, combinazioni LSA attive dei casi supportati |

Un numero nativo e una label Straus possono essere diversi. L'Identity map mantiene entrambi; i link nativi e le connection beam confluiscono nella stessa raccolta Link del Model e ricevono ID distinti. Le label originali sono una mappa di riferimento: un successivo export assegna le label corrispondenti agli ID del nuovo Model.

I materiali importati sono indicati come **Isotropic**, senza dedurre acciaio/calcestruzzo o inventare fy/fu/fck. Queste resistenze possono essere assegnate con Material Strengths prima di un verificatore.

## Nodi coincidenti, merge e modifica

Due nodi nello stesso punto possono rappresentare una discontinuità o un collegamento elastico. Il lettore imposta `PreserveCoincidentNodes=True`: la copia del Model e il merge conservano nodi con ID positivi distinti. Se una delle parti di Merge proviene da questo import, la conservazione si applica all'intero merge. Con Renumber=True, ogni parte riceve numeri nuovi senza saldare nodi distinti.

Build Model continua a saldare per tolleranza quando **Preserve nodes=False**. Dopo Decompose, collegare l'uscita Preserve nodes a Build. Per saldare intenzionalmente un file importato, ricostruire con False e controllare la nuova topologia. Gli elementi rimandano ai nodi canonici del Model; le copie e il merge non aggiungono una seconda volta gli attributi dello stesso nodo originale.

## Preview, sezioni e bake

Le sezioni importate conservano le **rigidezze numeriche del file**, come sezioni generiche. Il contorno è estratto separatamente dalla mesh di sezione quando Straus lo rende disponibile, e serve alla preview/Brep. Non viene inventato un rettangolo a partire da area e inerzie.

Per sezioni numeriche senza contorno il rapporto lo segnala: l'asse della trave resta disponibile, il bake solido della sezione richiede un profilo. Il contorno non ricostruisce la definizione originale di libreria/BGL né la discretizzazione per il recupero delle tensioni nella sezione del file sorgente. Per quelle esigenze riassegnare una sezione nativa/BXS appropriata.

Display Manager e Display Settings mantengono le categorie disattivabili. Il bake Brep rimane manuale e funziona a preview spenta per gli elementi convertibili.

## Ambito e omissioni

**Complete** significa assenza di omissioni rilevate nell'ambito strutturale elastico descritto sopra. Non certifica equivalenza dell'intero file Straus o adeguatezza del modello.

Il rapporto identifica gli attributi presenti ma non rappresentabili: ad esempio elementi quadratici, materiali non isotropi, UCS non globali per vincoli/molle/link, svincoli elastici o non lineari, taper, fattori di sezione, masse non strutturali, pressioni normali sulla faccia +Z delle piastre, attributi termici con tabelle, più casi di gravità, combinazioni che mescolano freedom case e riferimenti a risultati spettrali. Le combinazioni disabilitate sono escluse dall'insieme attivo e segnalate.

Impostazioni dei solver, leggi costitutive non lineari, costruzione per fasi, geometrie CAD, metadati di progetto/verifica e file risultati non appartengono al Model importato. Il rapporto riporta questo ambito anche quando Complete=True. Una lettura API fallita interrompe l'operazione e mostra l'errore; non viene sostituita con valori zero.

## Risultati già esistenti

Read Straus Model ricostruisce l'input strutturale; non associa automaticamente risultati di un file esterno. Per leggerli direttamente utilizzare i lettori su file dell'[esempio 22](../examples/22_file_esistenti_solver_risultati.gh), con numeri nativi e file corrispondenti.

Per avere risultati incorporati e verificabili nel Model, riesportare/calcolare con i componenti Run Model. Read Results Into Model richiede un `.st7` con l'identità del Model Rhino2Straus; Allow unverified non disabilita quel controllo. L'[esempio 23](../examples/23_rilettura_risultati_nel_model.gh) mostra la rilettura con manifest.

## Collaudo

Sono verificati assemblaggio, topologia coincidente, copie, merge, casi inutilizzati, temperature, controllo delle importazioni parziali e avvio a fronte. L'esempio 16 è salvato, riletto e ricalcolato in Grasshopper senza chiamare Straus. Il test nativo con confronto dei byte originali viene eseguito con `dotnet run --project tools/NativeTests -c Release -- --solve`.

Il 30 settembre 2026 `St7Init` restituisce **-8: hardware lock non trovato**. Questa era la limitazione della prova storica; il 9 novembre 2026 il roundtrip di base e i benchmark dei quattro solver sono stati eseguiti con esito positivo. [Stato completo delle verifiche](VALIDATION.md).

## Gruppi nodali

R31 non ammette nodi in `St7GetEntityGroup`. Il lettore lascia vuoti i gruppi nodali; quelli presenti in Grasshopper non vengono esportati e producono un avviso. Il roundtrip nativo di base è verificato il 9 novembre 2026: vedere [VALIDATION](VALIDATION.md) per ambito e limiti.
