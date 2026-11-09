# Note di versione

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
