# Sezioni, link e default — Straus7 R31

## Riferimenti SDK locali

`python tools/import_straus_sdk.py` importa dalla propria installazione il manuale R3, la guida R24→R3, `St7API.cs`, il Readme API e `St7API_DotNet.dll` in `.work/straus-sdk`, esclusa da Git. Il percorso installato della DLL è `API Includes/Grasshopper/St7API_DotNet.dll`.

L'audit confronta costanti e firme P/Invoke con il binding del progetto: **5.741 dichiarazioni, nessuna differenza**. Gli hash e i nomi dei riferimenti sono in [straus-sdk-reference.json](straus-sdk-reference.json). La DLL Grasshopper del produttore è un riferimento locale; il plugin usa il proprio binding verificato e il caricatore x64 condiviso.

## Tipologie di sezione

| Famiglia R31 | Componente |
|---|---|
| `bsCircularSolid`, `bsCircularHollow` | Frame Section solid circle, pipe |
| `bsSquareSolid`, `bsSquareHollow` | Frame Section solid rectangular, box |
| `bsISection`, `bsTSection`, `bsLSection` | Frame Section I, tee, Angle |
| `bsLipChannel` | Frame Section channel senza labbri; Beam Section Lipped Channel con labbri |
| `bsTopHatChannel`, `bsZSection` | Beam Section Top Hat Channel, Z |
| `bsTrapezoidSolid`, `bsTrapezoidHollow` | Beam Section Solid/Hollow Trapezoid |
| `bsTriangleSolid`, `bsTriangleHollow` | Beam Section Solid/Hollow Triangle |
| `bsCruciform` | Beam Section Cruciform |
| `bsTSection` con labbri | Beam Section Lipped Tee |
| `bsBXSSection` | Beam Section BXS, con dati incorporati nel Model |
| `bsBGLSection`: sei geometrie | Beam Section BGL I, Tee, Channel, Angle, Rectangular Hollow, Bulb Flat |
| Rigidezze numeriche libere | Frame Section Generic |

Gli otto profili originali e la sezione generica funzionano senza API. I **15 nuovi profili parametrici nativi** richiedono la licenza API: definiscono la geometria attraverso `St7SetBeamSectionGeometry`/`St7SetBeamSectionGeometryBGL`, calcolano le proprietà e producono il profilo di preview dalla mesh nativa. Le chiamate seguono il manuale R3, pagine stampate 456–462, 1003–1009; il loro collaudo nativo è ancora da completare, come indicato in [VALIDATION.md](VALIDATION.md).

Le dimensioni standard seguono `[D1,D2,D3,T1,T2,T3]`; le BGL seguono i 16 indici del manuale, con socket nominati `D`, `B`, `tw`, `tf`, `Rr`, `Rt` e `alpha` dove applicabili. Le lunghezze usano `Units`, gli angoli **BGL sono in gradi**. Il profilo restituito è una discretizzazione, centrata sul baricentro nativo; area, inerzie, torsione e taglio provengono dal calcolo nativo. La definizione parametrica resta nel Model ed è riesportata come tipologia nativa, non sostituita da sole rigidezze numeriche.

`Native Section Options` controlla divisioni nominali (default 24), divisioni circolari (default automatico) e calcolo delle aree di taglio (default attivo). `Units` omesso significa kN/m/°C. Il modello rifiuta una sezione nativa o BXS calcolata con unità di lunghezza diverse: rigenerarla con le unità del Model.

`Frame Section Generic` espone A, I11, I22, J, aree/lunghezze di taglio, baricentro e angolo principale in **radianti**. Rifiuta dati non finiti, area nulla/negativa e rigidezze negative. Un Brep planare XY facoltativo fornisce la preview, senza cambiare le rigidezze inserite. I default numerici illustrano una sezione quadrata 0,1×0,1 in metri e vanno adattati al caso reale.

`Deconstruct Beam Section` espone anche profilo, baricentro, angolo, famiglia, array dimensionale, opzioni e unità. `Beam Section Insertion` e gli offset degli elementi continuano a funzionare; il cambiamento del profilo che modifica l'offset d'inserimento invalida i risultati precedenti. Dopo l'aggiornamento dalla 0.2 rigenerare i risultati per il nuovo identificatore del Model.

## Link

`Link element` richiede due punti distinti; la proprietà omessa crea un link rigido XYZ. Sono disponibili ID automatico (`-1`) o esplicito e gruppi facoltativi. Tutti entrano nella raccolta Link del singolo Model e mantengono la preview del collegamento.

| Componente proprietà | Esportazione |
|---|---|
| Rigid Link Property | `St7SetRigidLink`; piano XYZ/XY/YZ/ZX |
| Pinned Link Property | `St7SetPinnedLink` |
| MasterSlave Link Property | `St7SetMasterSlaveLink`; UX/UY/UZ/RX/RY/RZ liberi, uguali o opposti |
| SectorSymmetry Link Property | `St7SetSectorSymmetryLink`; asse globale X/Y/Z |
| Elastic Link Property | Beam `btConnection`, sei rigidezze globali |

`Link Constraint Options` configura piano rigido, asse di simmetria e sei relazioni master-slave. Default: XYZ, asse Z, sei relazioni uguali. Ogni proprietà usa solo le opzioni applicabili al suo tipo. `Link Type` conserva i socket precedenti e aggiunge Options; il suo vecchio default errato `5` è corretto in `0=Rigid`. Le rigidezze elastiche usano forza/lunghezza e momento/radiante; i valori di default sono esempi, non un dimensionamento del collegamento.

I lettori `Model Link Node Displacement` e `Model Link Node Reaction` distinguono link nativi e connection beam; la verifica del file controlla tipo, connettività e opzioni native. I risultati restano in `Model.Results`. Sono supportati questi collegamenti a **due nodi distinti dopo la saldatura**; coupling a tre nodi, link multipunto e UCS arbitrari non sono esposti.

## Input predefiniti e icone

Dimensioni e spessori dei profili, materiale campione, selettori delle librerie, nomi di casi/proprietà/gruppi, fattori, carichi nulli, direzioni di riferimento, numeri di entità native, unità e opzioni hanno default dove il significato è determinabile. Geometrie, oggetti FEM da modificare, file e nomi di casi da selezionare in un modello esistente restano input espliciti. `Run` e `Overwrite` restano disattivati di default.

Tutti i **162 componenti** hanno un'icona 24×24 generata da codice; altre due icone coprono libreria e parametro Group. I 164 PNG sono in `Rhino2Straus.Grasshopper/Assets/Icons`; la [tavola completa](icons.png) è rigenerabile con `tools/IconGallery`. Generazione e uso delle icone non richiedono Straus7 né la licenza API.
