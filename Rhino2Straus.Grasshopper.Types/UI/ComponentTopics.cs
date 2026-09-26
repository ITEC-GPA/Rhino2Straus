using System.Linq;

namespace Rhino2Straus.Grasshopper;

public sealed record ComponentTopic(string Key,string Label,string Description,bool IsResults=false);

/// <summary>Shared ribbon topics, catalogue order and result-preview classification.</summary>
public static class ComponentTopics
{
    public const string Tab="Rhino2Straus";
    public const string Materials="01-Materiali";
    public const string Sections="02-Sezioni";
    public const string Properties="03-Proprietà";
    public const string Elements="04-Elementi";
    public const string ConstraintsLinks="05-Vincoli e link";
    public const string AxesOffsets="06-Assi e offset";
    public const string Cases="07-Casi e combo";
    public const string Loads="08-Carichi";
    public const string Model="09-Modello";
    public const string Files="10-File Straus";
    public const string Analysis="11-Analisi";
    public const string ResultsModel="12-Ris. modello";
    public const string ResultsNodes="13-Ris. nodi";
    public const string ResultsBeams="14-Ris. travi";
    public const string ResultsPlates="15-Ris. piastre";
    public const string ResultsSolids="16-Ris. solidi";
    public const string Preview="17-Preview e bake";

    public static readonly ComponentTopic[] All=
    {
        new(nameof(Materials),Materials,"Materiali, librerie, resistenze e lettura delle proprietà del materiale."),
        new(nameof(Sections),Sections,"Sezioni standard, generiche, BXS e BGL; opzioni native e decomposizione del profilo."),
        new(nameof(Properties),Properties,"Proprietà di travi e piastre, spessori, comportamento e assegnazione agli elementi."),
        new(nameof(Elements),Elements,"Creazione di nodi, travi, piastre e solidi; conversione delle geometrie e decomposizione degli elementi."),
        new(nameof(ConstraintsLinks),ConstraintsLinks,"Vincoli, svincoli, molle, masse, spostamenti imposti e creazione/proprietà dei link."),
        new(nameof(AxesOffsets),AxesOffsets,"Assi locali, allineamenti, orientamento, offset e inserimento delle sezioni."),
        new(nameof(Cases),Cases,"Casi di carico, fattori e combinazioni."),
        new(nameof(Loads),Loads,"Peso proprio e carichi nodali, su travi, piastre e solidi, inclusi termici e preload."),
        new(nameof(Model),Model,"Unità, Build, Merge, validazione, riepilogo, decomposizione, filtri e gruppi del Model unico."),
        new(nameof(Files),Files,"Esportazione .st7, lettura e bake della geometria da file Straus7."),
        new(nameof(Analysis),Analysis,"Esecuzione di casi e combinazioni, statica lineare/non lineare, modi e buckling."),
        new(nameof(ResultsModel),ResultsModel,"Lettura nel Model, casi risultato, estremi, metadati, deformata e dati per il verificatore.",true),
        new(nameof(ResultsNodes),ResultsNodes,"Spostamenti, reazioni e deformata dei nodi; risultati nodali dei link.",true),
        new(nameof(ResultsBeams),ResultsBeams,"Azioni, tensioni, deformazioni, spostamenti e diagrammi delle travi.",true),
        new(nameof(ResultsPlates),ResultsPlates,"Forze, momenti, tensioni, deformazioni, curvature e risultati nodali delle piastre.",true),
        new(nameof(ResultsSolids),ResultsSolids,"Tensioni, deformazioni e risultati nodali dei solidi FEM.",true),
        new(nameof(Preview),Preview,"Display Manager, impostazioni di visualizzazione, geometrie fisiche, Brep e bake solido.")
    };

    // Retain the legacy name for components deserialized from older Grasshopper documents.
    public static bool IsResult(string category)=>category=="12-Results"||All.Any(t=>t.IsResults&&t.Label==category);
}
