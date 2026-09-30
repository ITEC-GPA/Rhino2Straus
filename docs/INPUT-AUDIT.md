# Controllo ingressi — 2026-09-30

164 componenti e 766 ingressi ispezionati in Rhino/Grasshopper reali, con tutti e quattro i plugin caricati insieme e nessuna collisione GUID.

- Ingressi con dati predefiniti: 491.
- Ingressi marcati opzionali: 95; possono avere anche un default.
- Ingressi che richiedono dati del modello o parametri espliciti: 182.
- Componenti eseguiti con soli default/opzioni scollegate: 26.
- Sezioni native Straus escluse dal calcolo per dipendenza dalla licenza API: 15; tutti i loro ingressi sono stati ispezionati.

[Inventario completo per componente e ingresso](INPUT-AUDIT.json). I dati indispensabili — geometria, Model, proprietà, nomi/riferimenti e argomenti obbligatori delle API — richiedono ancora un collegamento. I default geometrici sono esempi modificabili nelle unità del Model.

Ripetere dalla cartella Rhino2MidasGen: `dotnet run --project tools/PluginAudit -c Release -- ..`, dopo build.ps1 dei quattro progetti (assembly in bin). Il controllo automatico dei default non avvia i solver strutturali.
