# Controllo ingressi — 27 settembre 2026

163 componenti e 757 ingressi ispezionati in Rhino/Grasshopper reali, con tutti e quattro i plugin caricati insieme e nessuna collisione GUID.

- Ingressi con dati predefiniti: 485.
- Ingressi marcati opzionali: 93; possono avere anche un default.
- Ingressi che richiedono dati del modello o parametri espliciti: 181.
- Componenti eseguiti con soli default/opzioni scollegate: 26.
- Sezioni native Straus escluse dal calcolo per dipendenza dalla licenza API: 15; tutti i loro ingressi sono stati ispezionati.

[Inventario completo per componente e ingresso](INPUT-AUDIT.json). I dati indispensabili — geometria, Model, proprietà, nomi/riferimenti e argomenti obbligatori delle API — richiedono ancora un collegamento. I default geometrici sono esempi modificabili nelle unità del Model.

Ripetere dalla cartella Rhino2MidasGen: `dotnet run --project tools/PluginAudit -c Release -- ..`, dopo la build Release dei quattro progetti. Il controllo automatico dei default non avvia i solver strutturali.
