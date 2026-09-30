# Rhino2Straus — tutorial Grasshopper

I modelli costruiti usano kN, m, C; gli importatori riportano le unita native o la conversione dichiarata. RUN, READ, BAKE, REPLACE e OVERWRITE sono inizialmente spenti.

| File | Argomento |
|---|---|
| [01_Frame_polilinea.gh](01_Frame_polilinea.gh) | Frame, materiali e sezioni |
| [02_Mesh_quad.gh](02_Mesh_quad.gh) | Mesh quadrangolare e pressione |
| [03_Mesh_tri.gh](03_Mesh_tri.gh) | Mesh triangolare |
| [04_Solidi.gh](04_Solidi.gh) | Solidi Hexa8 |
| [05_Link_elastico.gh](05_Link_elastico.gh) | Link e rigidezze |
| [06_Molle_masse.gh](06_Molle_masse.gh) | Vincoli, molle e masse |
| [07_Assi_offset_svincoli.gh](07_Assi_offset_svincoli.gh) | Assi locali, offset e svincoli |
| [08_Carichi_combinazioni.gh](08_Carichi_combinazioni.gh) | Casi e combinazioni |
| [09_Unione_filtri.gh](09_Unione_filtri.gh) | Unione, decomposizione e filtri |
| [10_Export.gh](10_Export.gh) | Esportare un modello |
| [11_Import_model.gh](11_Import_model.gh) | Modello esistente verso Grasshopper |
| [12_Analisi_frame.gh](12_Analisi_frame.gh) | Analisi e risultati delle travi |
| [13_Analisi_mesh.gh](13_Analisi_mesh.gh) | Analisi e risultati delle piastre |
| [14_Rilettura_risultati.gh](14_Rilettura_risultati.gh) | Risultati gia calcolati |
| [15_Preview_bake.gh](15_Preview_bake.gh) | Preview, impostazioni e bake |
| [16_Analisi_avanzate.gh](16_Analisi_avanzate.gh) | Analisi avanzate |

I file .checks.json registrano collegamenti e controlli. Ogni definizione e calcolata, salvata e riaperta in .gh e .ghx. Sono verificati i rami offline, i modelli costruiti e le geometrie. Dove applicabile, una variazione del 10% dello slider deve cambiare il volume dei solidi; il valore iniziale viene ripristinato. Le azioni native restano spente e richiedono applicazione/licenza/API e file reali. Gli avvisi di input vuoto sui rami spenti sono attesi. Nessun risultato fittizio e incorporato.

Per avviare un'azione usare false -> true. Scegliere Path; per MIDAS configurare Connection. Leggere Issues dopo import e analisi. Validate controlla i dati, non dimostra stabilita o adeguatezza strutturale.
