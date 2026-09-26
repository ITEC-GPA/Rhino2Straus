# Catalogo componenti

163 componenti Grasshopper in **17 pannelli per argomento**, tutti nella tab **Rhino2Straus** e tutti con icona. Il parametro persistente Group e la libreria hanno icone aggiuntive.

[Organizzazione della tab e mappa degli argomenti](TOPICS.md).

Il percorso resta elementi/attributi → Build Model o Merge Straus Models → un solo Model → Preview/Run → Model con risultati → estrazione/deformata/dati per il verificatore.

## 01-Materiali

Materiali, librerie, resistenze e lettura delle proprietà del materiale. **5 componenti.**

| Componente |
|---|
| [Database Concrete](../Rhino2Straus.Grasshopper/Components/Materials/DatabaseConcreteMaterialComponent.cs) |
| [Database Steel](../Rhino2Straus.Grasshopper/Components/Materials/DatabaseSteelMaterialComponent.cs) |
| [Deconstruct Material](../Rhino2Straus.Grasshopper/Components/Materials/MaterialInfoComponent.cs) |
| [Material](../Rhino2Straus.Grasshopper/Components/Materials/MaterialComponent.cs) |
| [Material Design Strengths](../Rhino2Straus.Grasshopper/Components/Materials/MaterialStrengthsComponent.cs) |

## 02-Sezioni

Sezioni standard, generiche, BXS e BGL; opzioni native e decomposizione del profilo. **27 componenti.**

| Componente |
|---|
| [Beam Section BGL Angle](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionBglAngleComponent.cs) |
| [Beam Section BGL Bulb Flat](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionBglBulbComponent.cs) |
| [Beam Section BGL Channel](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionBglChannelComponent.cs) |
| [Beam Section BGL I](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionBglIComponent.cs) |
| [Beam Section BGL Rectangular Hollow](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionBglRhsComponent.cs) |
| [Beam Section BGL Tee](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionBglTeeComponent.cs) |
| [Beam Section BXS](../Rhino2Straus.Grasshopper/Components/Sections/BxsSectionComponent.cs) |
| [Beam Section Cruciform](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionCruciformComponent.cs) |
| [Beam Section Hollow Trapezoid](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionTrapezoidHollowComponent.cs) |
| [Beam Section Hollow Triangle](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionTriangleHollowComponent.cs) |
| [Beam Section Lipped Channel](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionLipChannelComponent.cs) |
| [Beam Section Lipped Tee](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionTeeLippedComponent.cs) |
| [Beam Section Solid Trapezoid](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionTrapezoidSolidComponent.cs) |
| [Beam Section Solid Triangle](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionTriangleSolidComponent.cs) |
| [Beam Section Top Hat Channel](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionTopHatComponent.cs) |
| [Beam Section Z](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionZComponent.cs) |
| [Deconstruct Beam Section](../Rhino2Straus.Grasshopper/Components/Sections/SectionInfoComponent.cs) |
| [Frame Section Angle](../Rhino2Straus.Grasshopper/Components/Sections/FrameSectionAngleComponent.cs) |
| [Frame Section Generic](../Rhino2Straus.Grasshopper/Components/Sections/FrameSectionGenericComponent.cs) |
| [Frame Section I](../Rhino2Straus.Grasshopper/Components/Sections/FrameSectionIComponent.cs) |
| [Frame Section box](../Rhino2Straus.Grasshopper/Components/Sections/FrameSectionBoxComponent.cs) |
| [Frame Section channel](../Rhino2Straus.Grasshopper/Components/Sections/FrameSectionChannelComponent.cs) |
| [Frame Section pipe](../Rhino2Straus.Grasshopper/Components/Sections/FrameSectionPipeComponent.cs) |
| [Frame Section solid circle](../Rhino2Straus.Grasshopper/Components/Sections/FrameSectionCircleComponent.cs) |
| [Frame Section solid rectangular](../Rhino2Straus.Grasshopper/Components/Sections/FrameSectionRectangularComponent.cs) |
| [Frame Section tee](../Rhino2Straus.Grasshopper/Components/Sections/FrameSectionTeeComponent.cs) |
| [Native Section Options](../Rhino2Straus.Grasshopper/Components/Sections/NativeSectionOptionsComponent.cs) |

## 03-Proprietà

Proprietà di travi e piastre, spessori, comportamento e assegnazione agli elementi. **6 componenti.**

| Componente |
|---|
| [Area Property](../Rhino2Straus.Grasshopper/Components/Properties/AreaPropertyComponent.cs) |
| [Area thickness](../Rhino2Straus.Grasshopper/Components/Properties/AreaThicknessComponent.cs) |
| [Assign Beam Property](../Rhino2Straus.Grasshopper/Components/Properties/BeamAssignPropertyComponent.cs) |
| [Assign Plate Property](../Rhino2Straus.Grasshopper/Components/Properties/PlateAssignPropertyComponent.cs) |
| [Beam Behaviour](../Rhino2Straus.Grasshopper/Components/Properties/BeamBehaviourComponent.cs) |
| [Frame Property](../Rhino2Straus.Grasshopper/Components/Properties/FramePropertyComponent.cs) |

## 04-Elementi

Creazione di nodi, travi, piastre e solidi; conversione delle geometrie e decomposizione degli elementi. **10 componenti.**

| Componente |
|---|
| [Area element](../Rhino2Straus.Grasshopper/Components/Elements/AreaElementComponent.cs) |
| [Deconstruct Beam](../Rhino2Straus.Grasshopper/Components/Elements/BeamInfoComponent.cs) |
| [Deconstruct Node](../Rhino2Straus.Grasshopper/Components/Elements/NodeInfoComponent.cs) |
| [Deconstruct Plate](../Rhino2Straus.Grasshopper/Components/Elements/PlateInfoComponent.cs) |
| [Frame element](../Rhino2Straus.Grasshopper/Components/Elements/FrameElementComponent.cs) |
| [Mesh to Plates](../Rhino2Straus.Grasshopper/Components/Elements/MeshPlatesComponent.cs) |
| [Node Element](../Rhino2Straus.Grasshopper/Components/Elements/NodeElementComponent.cs) |
| [Polyline to Beams](../Rhino2Straus.Grasshopper/Components/Elements/PolylineBeamsComponent.cs) |
| [Solid Element](../Rhino2Straus.Grasshopper/Components/Elements/SolidElementComponent.cs) |
| [Solid Info](../Rhino2Straus.Grasshopper/Components/Elements/SolidInfoComponent.cs) |

## 05-Vincoli e link

Vincoli, svincoli, molle, masse, spostamenti imposti e creazione/proprietà dei link. **13 componenti.**

| Componente |
|---|
| [Beam End Releases](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/BeamReleasesComponent.cs) |
| [Elastic Link Property](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/ElasticLinkPropertyComponent.cs) |
| [Link Constraint Options](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/LinkConstraintOptionsComponent.cs) |
| [Link Type](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/LinkPropertyComponent.cs) |
| [Link element](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/LinkElementComponent.cs) |
| [MasterSlave Link Property](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/MasterSlaveLinkPropertyComponent.cs) |
| [Node Mass](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/NodeMassComponent.cs) |
| [Node Prescribed Displacement](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/PrescribedDisplacementComponent.cs) |
| [Node Springs](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/NodeSpringComponent.cs) |
| [Pinned Link Property](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/PinnedLinkPropertyComponent.cs) |
| [Rigid Link Property](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/RigidLinkPropertyComponent.cs) |
| [SectorSymmetry Link Property](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/SectorSymmetryLinkPropertyComponent.cs) |
| [Support](../Rhino2Straus.Grasshopper/Components/ConstraintsLinks/NodeSupportComponent.cs) |

## 06-Assi e offset

Assi locali, allineamenti, orientamento, offset e inserimento delle sezioni. **10 componenti.**

| Componente |
|---|
| [Align Beam Axis To Point](../Rhino2Straus.Grasshopper/Components/AxesOffsets/AlignBeamToPointComponent.cs) |
| [Align Beam Local Axis](../Rhino2Straus.Grasshopper/Components/AxesOffsets/AlignBeamAxisComponent.cs) |
| [Align Model Local Axes](../Rhino2Straus.Grasshopper/Components/AxesOffsets/AlignModelAxesComponent.cs) |
| [Align Plate Axis To Plane](../Rhino2Straus.Grasshopper/Components/AxesOffsets/AlignPlateToPlaneComponent.cs) |
| [Align Plate Local Axis](../Rhino2Straus.Grasshopper/Components/AxesOffsets/AlignPlateAxisComponent.cs) |
| [Beam Local Axes](../Rhino2Straus.Grasshopper/Components/AxesOffsets/BeamAxesComponent.cs) |
| [Beam Offset and Orientation](../Rhino2Straus.Grasshopper/Components/AxesOffsets/BeamOffsetComponent.cs) |
| [Beam Section Insertion](../Rhino2Straus.Grasshopper/Components/AxesOffsets/SectionInsertionComponent.cs) |
| [Plate Local Axes](../Rhino2Straus.Grasshopper/Components/AxesOffsets/PlateAxesComponent.cs) |
| [Plate Offset and Orientation](../Rhino2Straus.Grasshopper/Components/AxesOffsets/PlateOffsetComponent.cs) |

## 07-Casi e combo

Casi di carico, fattori e combinazioni. **3 componenti.**

| Componente |
|---|
| [Load case](../Rhino2Straus.Grasshopper/Components/Cases/LoadCaseComponent.cs) |
| [Load combination](../Rhino2Straus.Grasshopper/Components/Cases/LoadCombinationComponent.cs) |
| [Load factor](../Rhino2Straus.Grasshopper/Components/Cases/LoadFactorComponent.cs) |

## 08-Carichi

Peso proprio e carichi nodali, su travi, piastre e solidi, inclusi termici e preload. **15 componenti.**

| Componente |
|---|
| [Area Load Uniform](../Rhino2Straus.Grasshopper/Components/Loads/LoadAreaUniformComponent.cs) |
| [Beam Point Force](../Rhino2Straus.Grasshopper/Components/Loads/BeamPointForceComponent.cs) |
| [Beam Point Moment](../Rhino2Straus.Grasshopper/Components/Loads/BeamPointMomentComponent.cs) |
| [Beam Preload](../Rhino2Straus.Grasshopper/Components/Loads/BeamPreloadComponent.cs) |
| [Beam Temperature Gradient](../Rhino2Straus.Grasshopper/Components/Loads/BeamGradientComponent.cs) |
| [Load frame non-uniform](../Rhino2Straus.Grasshopper/Components/Loads/LoadFrameNonUniComponent.cs) |
| [Load frame uniform](../Rhino2Straus.Grasshopper/Components/Loads/LoadFrameUniformComponent.cs) |
| [Load nodal](../Rhino2Straus.Grasshopper/Components/Loads/LoadNodalComponent.cs) |
| [Node Temperature](../Rhino2Straus.Grasshopper/Components/Loads/NodeTemperatureComponent.cs) |
| [Plate Edge Pressure](../Rhino2Straus.Grasshopper/Components/Loads/PlateEdgePressureComponent.cs) |
| [Plate Global Pressure](../Rhino2Straus.Grasshopper/Components/Loads/GlobalPressureComponent.cs) |
| [Plate Preload](../Rhino2Straus.Grasshopper/Components/Loads/PlatePreloadComponent.cs) |
| [Plate Temperature Gradient](../Rhino2Straus.Grasshopper/Components/Loads/PlateGradientComponent.cs) |
| [Self Weight](../Rhino2Straus.Grasshopper/Components/Loads/SelfWeightComponent.cs) |
| [Solid Face Pressure](../Rhino2Straus.Grasshopper/Components/Loads/SolidPressureComponent.cs) |

## 09-Modello

Unità, Build, Merge, validazione, riepilogo, decomposizione, filtri e gruppi del Model unico. **10 componenti.**

| Componente |
|---|
| [Area Filter](../Rhino2Straus.Grasshopper/Components/Model/AreaFilterComponent.cs) |
| [Build Model](../Rhino2Straus.Grasshopper/Components/Model/BuildModelComponent.cs) |
| [Decompose Model](../Rhino2Straus.Grasshopper/Components/Model/DecomposeModelComponent.cs) |
| [Element Group](../Rhino2Straus.Grasshopper/Components/Model/ElementGroupComponent.cs) |
| [Frame Filter](../Rhino2Straus.Grasshopper/Components/Model/FrameFilterComponent.cs) |
| [Merge Straus Models](../Rhino2Straus.Grasshopper/Components/Model/MergeModelsComponent.cs) |
| [Model Summary](../Rhino2Straus.Grasshopper/Components/Model/ModelSummaryComponent.cs) |
| [Model Units](../Rhino2Straus.Grasshopper/Components/Model/ModelUnitsComponent.cs) |
| [Node Filter](../Rhino2Straus.Grasshopper/Components/Model/NodeFilterComponent.cs) |
| [Validate Straus Model](../Rhino2Straus.Grasshopper/Components/Model/ValidateModelComponent.cs) |

È disponibile anche il parametro persistente [Group](../Rhino2Straus.Grasshopper/Parameter/GroupParam.cs), nello stesso pannello.

## 10-File Straus

Esportazione .st7, lettura e bake della geometria da file Straus7. **3 componenti.**

| Componente |
|---|
| [Bake Straus Geometry](../Rhino2Straus.Grasshopper/Components/Files/BakeStrausGeometryComponent.cs) |
| [Read Straus Geometry](../Rhino2Straus.Grasshopper/Components/Files/ReadStrausGeometryComponent.cs) |
| [Straus7 R3 Export](../Rhino2Straus.Grasshopper/Components/Files/StrausR3ExportComponent.cs) |

## 11-Analisi

Esecuzione di casi e combinazioni, statica lineare/non lineare, modi e buckling. **8 componenti.**

| Componente |
|---|
| [Run Linear Buckling](../Rhino2Straus.Grasshopper/Components/Analysis/RunLinearBucklingComponent.cs) |
| [Run Linear Static](../Rhino2Straus.Grasshopper/Components/Analysis/RunLinearStaticComponent.cs) |
| [Run Model Cases and Combinations](../Rhino2Straus.Grasshopper/Components/Analysis/RunModelCasesComponent.cs) |
| [Run Model Linear Buckling](../Rhino2Straus.Grasshopper/Components/Analysis/RunModelBucklingComponent.cs) |
| [Run Model Natural Frequencies](../Rhino2Straus.Grasshopper/Components/Analysis/RunModelModalComponent.cs) |
| [Run Model Nonlinear Static](../Rhino2Straus.Grasshopper/Components/Analysis/RunModelNonlinearComponent.cs) |
| [Run Natural Frequency](../Rhino2Straus.Grasshopper/Components/Analysis/RunNaturalFrequencyComponent.cs) |
| [Run Nonlinear Static](../Rhino2Straus.Grasshopper/Components/Analysis/RunNonlinearStaticComponent.cs) |

## 12-Ris. modello

Lettura nel Model, casi risultato, estremi, metadati, deformata e dati per il verificatore. **14 componenti.**

| Componente |
|---|
| [Buckling Factors](../Rhino2Straus.Grasshopper/Components/ResultsModel/BucklingFactorsComponent.cs) |
| [Clear Model Results](../Rhino2Straus.Grasshopper/Components/ResultsModel/ClearModelResultsComponent.cs) |
| [Decompose Native Beam Section](../Rhino2Straus.Grasshopper/Components/ResultsModel/DecomposeNativeSectionComponent.cs) |
| [Decompose Result Record](../Rhino2Straus.Grasshopper/Components/ResultsModel/DecomposeResultRecordComponent.cs) |
| [Modal Frequencies](../Rhino2Straus.Grasshopper/Components/ResultsModel/ModalResultsComponent.cs) |
| [Model Deformed Geometry](../Rhino2Straus.Grasshopper/Components/ResultsModel/ModelDeformedGeometryComponent.cs) |
| [Model Modal and Buckling Data](../Rhino2Straus.Grasshopper/Components/ResultsModel/ModelModalMetadataComponent.cs) |
| [Model Native Beam Sections](../Rhino2Straus.Grasshopper/Components/ResultsModel/ModelNativeSectionsComponent.cs) |
| [Model Result Cases](../Rhino2Straus.Grasshopper/Components/ResultsModel/ModelResultCasesComponent.cs) |
| [Model Result Extrema](../Rhino2Straus.Grasshopper/Components/ResultsModel/ModelResultEnvelopeComponent.cs) |
| [Model Verification Data](../Rhino2Straus.Grasshopper/Components/ResultsModel/ModelVerificationDataComponent.cs) |
| [Read Results Into Model](../Rhino2Straus.Grasshopper/Components/ResultsModel/ReadResultsIntoModelComponent.cs) |
| [Result Envelope](../Rhino2Straus.Grasshopper/Components/ResultsModel/ResultEnvelopeComponent.cs) |
| [Straus Result Cases](../Rhino2Straus.Grasshopper/Components/ResultsModel/StrausResultCasesComponent.cs) |

## 13-Ris. nodi

Spostamenti, reazioni e deformata dei nodi; risultati nodali dei link. **7 componenti.**

| Componente |
|---|
| [Deformed Nodes](../Rhino2Straus.Grasshopper/Components/ResultsNodes/DeformedNodesComponent.cs) |
| [Model Link Node Displacement](../Rhino2Straus.Grasshopper/Components/ResultsNodes/ModelLinkNodeDisplacementComponent.cs) |
| [Model Link Node Reaction](../Rhino2Straus.Grasshopper/Components/ResultsNodes/ModelLinkNodeReactionComponent.cs) |
| [Model Node Displacement](../Rhino2Straus.Grasshopper/Components/ResultsNodes/ModelNodeDisplacementComponent.cs) |
| [Model Node Reaction](../Rhino2Straus.Grasshopper/Components/ResultsNodes/ModelNodeReactionComponent.cs) |
| [Node Displacements](../Rhino2Straus.Grasshopper/Components/ResultsNodes/NodeDisplacementsComponent.cs) |
| [Node Reactions](../Rhino2Straus.Grasshopper/Components/ResultsNodes/NodeReactionsComponent.cs) |

## 14-Ris. travi

Azioni, tensioni, deformazioni, spostamenti e diagrammi delle travi. **11 componenti.**

| Componente |
|---|
| [Beam Displacements](../Rhino2Straus.Grasshopper/Components/ResultsBeams/BeamDisplacementResultsComponent.cs) |
| [Beam Forces](../Rhino2Straus.Grasshopper/Components/ResultsBeams/BeamForcesComponent.cs) |
| [Beam Stress](../Rhino2Straus.Grasshopper/Components/ResultsBeams/BeamStressComponent.cs) |
| [Model Beam Actions N V T M](../Rhino2Straus.Grasshopper/Components/ResultsBeams/ModelBeamActionsComponent.cs) |
| [Model Beam Displacement](../Rhino2Straus.Grasshopper/Components/ResultsBeams/ModelBeamDisplacementComponent.cs) |
| [Model Beam Force](../Rhino2Straus.Grasshopper/Components/ResultsBeams/ModelBeamForceComponent.cs) |
| [Model Beam Force Diagram](../Rhino2Straus.Grasshopper/Components/ResultsBeams/ModelBeamDiagramComponent.cs) |
| [Model Beam Node Reaction](../Rhino2Straus.Grasshopper/Components/ResultsBeams/ModelBeamNodeReactionComponent.cs) |
| [Model Beam Strain](../Rhino2Straus.Grasshopper/Components/ResultsBeams/ModelBeamStrainComponent.cs) |
| [Model Beam Stress](../Rhino2Straus.Grasshopper/Components/ResultsBeams/ModelBeamStressComponent.cs) |
| [Model Beam Total Strain](../Rhino2Straus.Grasshopper/Components/ResultsBeams/ModelBeamTotalStrainComponent.cs) |

## 15-Ris. piastre

Forze, momenti, tensioni, deformazioni, curvature e risultati nodali delle piastre. **11 componenti.**

| Componente |
|---|
| [Model Plate Curvature](../Rhino2Straus.Grasshopper/Components/ResultsPlates/ModelPlateCurvatureComponent.cs) |
| [Model Plate Force](../Rhino2Straus.Grasshopper/Components/ResultsPlates/ModelPlateForceComponent.cs) |
| [Model Plate Moment](../Rhino2Straus.Grasshopper/Components/ResultsPlates/ModelPlateMomentComponent.cs) |
| [Model Plate Node Displacement](../Rhino2Straus.Grasshopper/Components/ResultsPlates/ModelPlateNodeDisplacementComponent.cs) |
| [Model Plate Node Reaction](../Rhino2Straus.Grasshopper/Components/ResultsPlates/ModelPlateNodeReactionComponent.cs) |
| [Model Plate Strain](../Rhino2Straus.Grasshopper/Components/ResultsPlates/ModelPlateStrainComponent.cs) |
| [Model Plate Stress](../Rhino2Straus.Grasshopper/Components/ResultsPlates/ModelPlateStressComponent.cs) |
| [Model Plate Total Strain](../Rhino2Straus.Grasshopper/Components/ResultsPlates/ModelPlateTotalStrainComponent.cs) |
| [Plate Forces](../Rhino2Straus.Grasshopper/Components/ResultsPlates/PlateForceResultsComponent.cs) |
| [Plate Moments](../Rhino2Straus.Grasshopper/Components/ResultsPlates/PlateMomentResultsComponent.cs) |
| [Plate Stress](../Rhino2Straus.Grasshopper/Components/ResultsPlates/PlateStressResultsComponent.cs) |

## 16-Ris. solidi

Tensioni, deformazioni e risultati nodali dei solidi FEM. **7 componenti.**

| Componente |
|---|
| [Model Solid Node Displacement](../Rhino2Straus.Grasshopper/Components/ResultsSolids/ModelSolidNodeDisplacementComponent.cs) |
| [Model Solid Node Reaction](../Rhino2Straus.Grasshopper/Components/ResultsSolids/ModelSolidNodeReactionComponent.cs) |
| [Model Solid Strain](../Rhino2Straus.Grasshopper/Components/ResultsSolids/ModelSolidStrainComponent.cs) |
| [Model Solid Stress](../Rhino2Straus.Grasshopper/Components/ResultsSolids/ModelSolidStressComponent.cs) |
| [Model Solid Total Strain](../Rhino2Straus.Grasshopper/Components/ResultsSolids/ModelSolidTotalStrainComponent.cs) |
| [Solid Strain](../Rhino2Straus.Grasshopper/Components/ResultsSolids/SolidStrainResultsComponent.cs) |
| [Solid Stress](../Rhino2Straus.Grasshopper/Components/ResultsSolids/SolidStressResultsComponent.cs) |

## 17-Preview e bake

Display Manager, impostazioni di visualizzazione, geometrie fisiche, Brep e bake solido. **3 componenti.**

| Componente |
|---|
| [Model Physical Geometry](../Rhino2Straus.Grasshopper/Components/Preview/ModelGeometryComponent.cs) |
| [Rhino2Straus Display Manager](../Rhino2Straus.Grasshopper/Components/Preview/PreviewModelComponent.cs) |
| [Rhino2Straus Display Settings](../Rhino2Straus.Grasshopper/Components/Preview/DisplaySettingsComponent.cs) |

![Icone per argomento](icons.png)
