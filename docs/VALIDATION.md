# Stato delle verifiche

Data: 25 settembre 2026. Ambiente: Windows x64, .NET SDK 9, Rhino 8, Straus7 R31.

- Build Release dell'intera soluzione: completata senza errori e senza avvisi.
- **51 test gestiti**: unità, proprietà, copie indipendenti, merge di nodi e Model, ID, carichi, vincoli, spostamenti imposti, solidi, tabelle immutabili dei risultati, identità del Model, invalidazione, manifest SHA-256, dipendenze delle combinazioni, resistenze materiali e copie dei casi.
- **39 controlli geometrici nel motore Rhino**: sezioni, inerzie principali, estrusione con volume positivo, offset trave, inserimento, trasformazione rigida, spessore e offset piastra, mesh chiusa, allineamento assi di travi verticali/inclinate e piastre, direzioni parallele rifiutate e assi geometrici/principali. Il test ha identificato e consentito di correggere l'orientamento iniziale delle estrusioni e la sezione T mancante.
- **140 componenti istanziati nel runtime Grasshopper**: GUID unici, icone 24×24 e registrazione delle uscite.
- **8 controlli di calcolo dati Grasshopper** con risultati sintetici: rimappatura N/M2, estremi firmati e posizioni governanti, decomposizione Model, alberi di spostamenti, collegamento dati del verificatore e deformata. Sono prove dei componenti, non soluzioni FEM native.
- Atlante PNG delle icone controllato visivamente.
- **Test nativo Straus7 non completato**: `St7Init` restituisce errore **-8**, licenza STANDALONE senza hardware lock disponibile. Non sono stati prodotti risultati di solver da usare come validazione. Il relativo test è presente e può essere rilanciato con `--native --solve` quando la licenza è accessibile.

Questi test verificano la libreria e parte dell'integrazione Rhino. Non attestano ancora un collaudo interattivo completo di ogni componente in Grasshopper né la correttezza ingegneristica di un modello dell'utente.
