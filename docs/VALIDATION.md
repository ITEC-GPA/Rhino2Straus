# Stato delle verifiche

Data: 26 settembre 2026. Ambiente: Windows x64, .NET SDK 9, Rhino 8, Straus7 R31.

- Build Release dell'intera soluzione: completata senza errori e senza avvisi.
- **60 test gestiti**: unità, proprietà, copie indipendenti, merge di nodi e Model, ID, carichi, vincoli, spostamenti imposti, solidi, tabelle immutabili dei risultati, identità del Model, invalidazione, manifest SHA-256, dipendenze delle combinazioni, resistenze materiali e copie dei casi.
- **42 controlli geometrici nel motore Rhino**: sezioni, inerzie principali, estrusione con volume positivo, offset trave, inserimento, trasformazione rigida, spessore e offset piastra, mesh chiusa, allineamento assi di travi verticali/inclinate e piastre, direzioni parallele rifiutate e assi geometrici/principali. Il test ha identificato e consentito di correggere l'orientamento iniziale delle estrusioni e la sezione T mancante.
- **162 componenti istanziati nel runtime Grasshopper**: GUID unici, icone 24×24 e registrazione delle uscite.
- **35 controlli di calcolo dati Grasshopper** con risultati sintetici: rimappatura N/M2, estremi firmati e posizioni governanti, decomposizione Model, alberi di spostamenti, collegamento dati del verificatore e deformata. Includono anche il calcolo con i default di 22 componenti e il passaggio reale delle opzioni attraverso connessioni Grasshopper. Sono prove dei componenti, non soluzioni FEM native.
- **164 PNG** (162 componenti, libreria e parametro Group) e atlante controllati visivamente; generazione indipendente da Straus7.
- SDK R31 importato localmente: 5.741 dichiarazioni confrontate, nessuna mancante o differente.
- I nuovi profili nativi e le chiamate native dei nuovi link sono implementati sul manuale R31; la loro esecuzione e il confronto con Straus7 rimangono da collaudare con licenza API operativa.
- **Test nativo Straus7 non completato**: `St7Init` restituisce errore **-8**, licenza STANDALONE senza hardware lock disponibile. Non sono stati prodotti risultati di solver da usare come validazione. Il relativo test è presente e può essere rilanciato con `--native --solve` quando la licenza è accessibile.

Questi test verificano la libreria e parte dell'integrazione Rhino. Non attestano ancora un collaudo interattivo completo di ogni componente in Grasshopper né la correttezza ingegneristica di un modello dell'utente.
