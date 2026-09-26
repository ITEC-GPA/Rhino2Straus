using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Base;
using Rhino2Straus.Core.Cases;
using Rhino2Straus.Core.ElementProperties;
using Rhino2Straus.Core.Elements;
using Rhino2Straus.Core.Loads;
using Rhino2Straus.Core.Models;
using Rhino2Straus.Core.Settings;
using St7API;
using Rhino2Straus.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rhino2Straus.Core.Helper
{
    public class HelperStrausExport
    {

        public ModelModel Model { get; set; }

        public HelperStrausExport(ModelModel model)
        {
            Model = model;
        }

        public bool CreateModelR3(string outputPath, out int modelId, out List<string> warnings, out List<string> errors, bool overwrite = false)
        {
            warnings = new List<string>();
            errors = new List<string>();
            modelId = 0;
            string stagingPath = null;
            StrausSession session = null;
            try
            {
                var validation = ModelValidator.Validate(Model);
                errors.AddRange(validation.Errors);
                warnings.AddRange(validation.Warnings);
                if (errors.Count > 0) return false;
                outputPath = Path.GetFullPath(outputPath);
                if (!string.Equals(Path.GetExtension(outputPath), ".st7", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Output must have .st7 extension.");
                if (!Directory.Exists(Path.GetDirectoryName(outputPath))) throw new DirectoryNotFoundException("Output directory does not exist.");
                if (File.Exists(outputPath) && !overwrite) throw new IOException("Output exists. Enable Overwrite to replace it.");
                stagingPath = Path.Combine(Path.GetDirectoryName(outputPath), ".rhino2straus-" + Guid.NewGuid().ToString("N") + ".st7");
                session = new StrausSession(stagingPath, create: true);
                modelId = session.Id;
                HandleError(St7.St7SetTitle(modelId, St7.tbReference, "Rhino2Straus:" + ModelFingerprint.Compute(Model)));
                HandleError(St7.St7SetUnits(modelId, StrausUnits.ToApi(Model.ModelUnits)));

                var groupsIds=new Dictionary<ElementGroupModel,int>();var groupPaths=new Dictionary<string,int>();
                int EnsureGroup(ElementGroupModel group)
                {
                    if(groupPaths.TryGetValue(group.GroupFullName,out int existing))return existing;
                    int parent=group.HasGroupParent?EnsureGroup(group.GroupParent):1;int created=0;
                    HandleError(St7.St7NewChildGroup(session.Id,parent,group.Name,ref created));
                    groupPaths.Add(group.GroupFullName,created);return created;
                }
                foreach(var group in Model.Groups.Values)groupsIds[group]=EnsureGroup(group);

                int id = 1;
                Dictionary<string, int> beamProperties = new Dictionary<string, int>();
                Dictionary<int, string> bxsFiles = new Dictionary<int, string>();

                for (int k = 0; k < Model.FrameProperties.Count; k++)
                {
                    FramePropertyModel bp = Model.FrameProperties.ElementAt(k).Value;
                    if (beamProperties.ContainsKey(bp.Name))
                    {
                        warnings.Add(string.Format("The beam property {0} already exists", bp.Name));
                        continue;
                    }

                    int beamType = bp.Behaviour switch { FramePropertyModel.Behaviours.Truss => St7.btTruss, FramePropertyModel.Behaviours.Cable => St7.btCable, _ => St7.btBeam };

                    if (HandleError(St7.St7NewBeamProperty(modelId, id, beamType, bp.Name)))
                    {
                        errors.Add(string.Format("Failed to create beam property {0}\r\n", bp.Name));
                        continue;
                    }

                    if (bp.SectionGeometryType != FrameSectionModel.SectionGeometryTypes.Generic)
                    {

                        {
                            int sectionType = St7.bsNullSection;
                            double[] st_doubles = new double[6];
                            switch (bp.SectionGeometryType)
                            {
                                case FrameSectionModel.SectionGeometryTypes.SR:
                                    HandleError(St7.St7SetBeamSectionName(modelId, id, "SolidRound"));
                                    sectionType = St7.bsCircularSolid;
                                    st_doubles[0] = bp.Dimension1;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.P:
                                    HandleError(St7.St7SetBeamSectionName(modelId, id, "HollowRound"));
                                    sectionType = St7.bsCircularHollow;
                                    st_doubles[0] = bp.Dimension1;
                                    st_doubles[3] = bp.Dimension2;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.SB:
                                    HandleError(St7.St7SetBeamSectionName(modelId, id, "SolidRect"));
                                    sectionType = St7.bsSquareSolid;
                                    st_doubles[0] = bp.Dimension2;
                                    st_doubles[1] = bp.Dimension1;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.B:
                                    HandleError(St7.St7SetBeamSectionName(modelId, id, "HollowRect"));
                                    sectionType = St7.bsSquareHollow;
                                    st_doubles[0] = bp.Dimension2;
                                    st_doubles[1] = bp.Dimension1;
                                    st_doubles[3] = bp.Dimension3;
                                    st_doubles[4] = bp.Dimension4;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.H:
                                    HandleError(St7.St7SetBeamSectionName(modelId, id, "IBeam"));
                                    sectionType = St7.bsISection;
                                    st_doubles[0] = bp.Dimension5;
                                    st_doubles[1] = bp.Dimension2;
                                    st_doubles[2] = bp.Dimension1;
                                    st_doubles[3] = bp.Dimension6;
                                    st_doubles[4] = bp.Dimension4;
                                    st_doubles[5] = bp.Dimension3;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.T:
                                    HandleError(St7.St7SetBeamSectionName(modelId, id, "TBeam"));
                                    sectionType = St7.bsTSection;
                                    st_doubles[0] = bp.Dimension2;
                                    st_doubles[1] = bp.Dimension1;
                                    st_doubles[2] = bp.Dimension5;
                                    st_doubles[3] = bp.Dimension4;
                                    st_doubles[4] = bp.Dimension3;
                                    st_doubles[5] = bp.Dimension6;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.C:
                                    HandleError(St7.St7SetBeamSectionName(modelId, id, "Channel"));
                                    sectionType = St7.bsLipChannel;
                                    st_doubles[0] = bp.Dimension2;
                                    st_doubles[1] = bp.Dimension1;
                                    st_doubles[2] = 0;
                                    st_doubles[3] = bp.Dimension4;
                                    st_doubles[4] = bp.Dimension3;
                                    st_doubles[5] = 0;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.L:
                                    HandleError(St7.St7SetBeamSectionName(modelId, id, "Angle"));
                                    sectionType = St7.bsLSection;
                                    st_doubles[0] = bp.Dimension2;
                                    st_doubles[1] = bp.Dimension1;
                                    st_doubles[3] = bp.Dimension4;
                                    st_doubles[4] = bp.Dimension3;
                                    break;
                                default:
                                    warnings.Add(string.Format("Unable to det the section geometry {0} of the beam property {1}",
                                        bp.SectionGeometryType, bp.Id));
                                    break;
                            }
                            HandleError(St7.St7SetBeamSectionGeometry(modelId, id, sectionType, st_doubles));
                        }
                    }

                    if(bp.Section.NativeDefinition!=null) NativeSections.Assign(modelId,id,bp.Section.NativeDefinition);
                    else if(bp.Section.BxsData!=null) BxsSections.Assign(modelId,id,bp.Section.BxsData);
                    else if (bp.SectionGeometryType == FrameSectionModel.SectionGeometryTypes.Generic)
                    {
                        int[] ints = new int[1];
                        ints[0] = 0;
                        double[] doubles = new double[11];
                        doubles[St7.ipAREA] = bp.SectionArea;
                        doubles[St7.ipI11] = bp.I11;
                        doubles[St7.ipI22] = bp.I22;
                        doubles[St7.ipJ] = bp.J;
                        doubles[St7.ipSL1] = bp.ShearL1;
                        doubles[St7.ipSL2] = bp.ShearL2;
                        doubles[St7.ipSA1] = bp.ShearA1;
                        doubles[St7.ipSA2] = bp.ShearA2;
                        doubles[St7.ipXBAR] = bp.Centroid.X;
                        doubles[St7.ipYBAR] = bp.Centroid.Y;
                        doubles[St7.ipANGLE] = bp.Angle;
                        HandleError(St7.St7SetBeamSectionPropertyData(modelId, id, ints, doubles));
                    }

                    HandleError(St7.St7SetMaterialName(modelId, St7.ptBEAMPROP, id, bp.Material.Name));

                    double[] doubles1 = new double[9];
                    doubles1[St7.ipBeamModulus] = bp.Material.ModulusElasticity;
                    doubles1[St7.ipBeamDensity] = bp.Material.Density / Math.Pow(StrausUnits.LengthPerMetre(Model.ModelUnits.LengthUnit), 3);
                    doubles1[St7.ipBeamAlpha] = StrausUnits.ThermalExpansion(bp.Material.ThermalCoefficient,Model.ModelUnits);
                    doubles1[St7.ipBeamViscosity] = bp.Material.ViscousDamping;
                    doubles1[St7.ipBeamDampingRatio] = bp.Material.DampingRatio;
                    doubles1[St7.ipBeamConductivity] = bp.Material.Conductivity;
                    doubles1[St7.ipBeamSpecificHeat] = bp.Material.SpecificHeat;
                    doubles1[St7.ipBeamShear] = bp.Material.ShearModulus;
                    doubles1[St7.ipBeamPoisson] = bp.Material.PoissonRatio;

                    HandleError(St7.St7SetBeamMaterialData(modelId, id, doubles1));

                    HandleError(St7.St7SetBeamShearModulusMode(modelId, id, St7.smUsePoissonsRatio));

                    HandleError(St7.St7UpdateElementPropertyData(modelId, St7.ptBEAMPROP, id));

                    if (bp.Section.NativeDefinition is { } nativeDefinition)
                        HandleError(St7.St7CalculateBeamSectionProperties(modelId,id,(byte)(nativeDefinition.Options.IncludeShear ? St7.btTrue : St7.btFalse)));
                    else if (bp.SectionGeometryType != FrameSectionModel.SectionGeometryTypes.Generic)
                        HandleError(St7.St7CalculateBeamSectionProperties(modelId, id, St7.btTrue));

                    beamProperties.Add(bp.Name, id++);

                }

                id = 1;
                Dictionary<string, int> plateProperties = new Dictionary<string, int>();
                for (int j = 0; j < Model.AreaProperties.Count; j++)
                {
                    AreaPropertyModel pp = Model.AreaProperties.ElementAt(j).Value;
                    if (plateProperties.ContainsKey(pp.Name))
                    {
                        warnings.Add(string.Format("The plate property {0} already exists", pp.Name));
                        continue;
                    }

                    int plateType=pp.PropertyType switch {
                        AreaPropertyModel.PlatePropertyType.PlateShell=>St7.ptPlateShell,
                        AreaPropertyModel.PlatePropertyType.Membrane=>St7.ptMembrane,
                        AreaPropertyModel.PlatePropertyType.LoadPatch=>St7.ptLoadPatch,
                        AreaPropertyModel.PlatePropertyType.ShearPanel=>St7.ptShearPanel,
                        AreaPropertyModel.PlatePropertyType.PlaneStress=>St7.ptPlaneStress,
                        AreaPropertyModel.PlatePropertyType.PlaneStrain=>St7.ptPlaneStrain,
                        AreaPropertyModel.PlatePropertyType.Axisymmetric=>St7.ptAxisymmetric,
                        _=>throw new ArgumentException("Unsupported Straus7 plate property type.") };
                    HandleError(St7.St7NewPlateProperty(modelId,id,plateType,St7.mtIsotropic,pp.Name));
                    if(pp.PropertyType!=AreaPropertyModel.PlatePropertyType.LoadPatch)
                    {
                        HandleError(St7.St7SetMaterialName(modelId,St7.ptPLATEPROP,id,pp.Material.Name));
                        HandleError(St7.St7SetPlateThickness(modelId,id,new[]{pp.ThicknessMembrane,pp.PropertyType==AreaPropertyModel.PlatePropertyType.PlateShell?pp.ThicknessBending:0}));
                        var m=pp.Material;
                        HandleError(St7.St7SetPlateIsotropicMaterial(modelId,id,new[]{m.ModulusElasticity,m.PoissonRatio,m.Density/Math.Pow(StrausUnits.LengthPerMetre(Model.ModelUnits.LengthUnit),3),StrausUnits.ThermalExpansion(m.ThermalCoefficient,Model.ModelUnits),m.ViscousDamping,m.DampingRatio,m.Conductivity,m.SpecificHeat}));
                    }

                    plateProperties.Add(pp.Name, id++);
                }

                Dictionary<string, int> loadCaseNameIdMap = new Dictionary<string, int>();

                id = 1;
                if (Model.SelfWeight != null)
                {
                    var sw = Model.SelfWeight;

                    if (HandleError(St7.St7SetLoadCaseName(modelId, id, sw.LoadCase.Name)))
                        errors.Add(string.Format("Failed to rename the default load case in {0}", sw.LoadCase.Name));

                    loadCaseNameIdMap[sw.LoadCase.Name] = id;

                    int caseType = St7.lcNoInertia;

                    if (sw.GlobalInertiaLoad == SelfWeightModel.GlobalInertiaLoads.Gravity)
                        caseType = St7.lcGravity;

                    if (HandleError(St7.St7SetLoadCaseType(modelId, id, caseType)))
                        errors.Add(string.Format("Failed to set load case type of the load case {0}", sw.LoadCase.Name));

                    if (HandleError(St7.St7SetLoadCaseGravityDir(modelId, id, (int)sw.GravityDirection + 1)))
                        errors.Add(string.Format("Failed to set gravity direction of the load case {0}", sw.LoadCase.Name));

                    if (HandleError(St7.St7SetLoadCaseMassOption(modelId, id, Convert.ToByte(sw.StructuralMassAcceleration), Convert.ToByte(sw.NonStructuralMassAcceleration))))
                        errors.Add(string.Format("Failed to set the mass options of the {0} load case", sw.LoadCase.Name));

                    if (HandleError(St7.St7SetLoadCaseGravity(modelId, id, sw.GravityValue)))
                        errors.Add(string.Format("Failed to set the gravity value of the {0} load case", sw.LoadCase.Name));
                    id++;
                }

                for (int i = 0; i < Model.LoadCases.Count; i++)
                {
                    LoadCaseModel loadCase = new LoadCaseModel(Model.LoadCases.ElementAt(i).Value);

                    if (loadCaseNameIdMap.ContainsKey(loadCase.Name)) continue;
                    if (HandleError(id == 1 ? St7.St7SetLoadCaseName(modelId, id, loadCase.Name) : St7.St7NewLoadCase(modelId, loadCase.Name)))
                        errors.Add(string.Format("Failed to define the load case {0}", loadCase.Name));

                    loadCaseNameIdMap[loadCase.Name] = id;

                    int caseType = St7.lcNoInertia;

                    if (HandleError(St7.St7SetLoadCaseType(modelId, id, caseType)))
                        errors.Add(string.Format("Failed to set load case type of the load case {0}", loadCase.Name));

                    if (HandleError(St7.St7SetLoadCaseMassOption(modelId, id, Convert.ToByte(false), Convert.ToByte(loadCase.GravityNoStructuralMass))))
                        errors.Add(string.Format("Failed to set the mass options of the {0} load case", loadCase.Name));
                    id++;
                }

                int comboNumber = 1;
                for (int i = 0; i < Model.LoadCombinations.Count; i++)
                {
                    LoadCombinationModel combo = Model.LoadCombinations.ElementAt(i).Value;

                    if (HandleError(St7.St7AddLSACombination(modelId, combo.Name)))
                    {
                        errors.Add($"Failed to add the LSA combination: {combo.Name}");
                    }
                    else
                    {
                        foreach (var item in combo.LoadFactorList.GroupBy(x => x.LoadCase.Name))
                        {
                            int lc = loadCaseNameIdMap[item.Key];
                            if (HandleError(St7.St7SetLSACombinationFactor(modelId, St7.ltLoadCase, comboNumber, lc, 1, item.Sum(x => x.Factor))))
                                errors.Add($"Failed to set the LSA combination factor: {combo.Name}");
                        }
                    }

                    comboNumber++;
                }

                var nodePositionMap=new Dictionary<NodeElementModel,int>(ReferenceEqualityComparer.Instance);
                int nodeNumbering=0;foreach(var node in Model.NodeElements.Values)nodePositionMap.Add(node,++nodeNumbering);
                var beamsNodeNumberMap=Model.FrameElements.Values.Select(beam=>new[]{2,nodePositionMap[beam.NodeStart],nodePositionMap[beam.NodeEnd]}).ToArray();
                var plateNodeNumberMap=Model.AreaElements.Values.Select(plate=>new[]{plate.NodeList.Count}.Concat(plate.NodeList.Select(node=>nodePositionMap[node])).ToArray()).ToArray();

                #region Export nodi ed elementi

                HashSet<int> addedNodesNumber = new HashSet<int>();
                for (int i = 0; i < Model.NodeElements.Count; i++)
                {
                    NodeElementModel node = Model.NodeElements.ElementAt(i).Value;
                    try
                    {
                        int nodeNumber = i + 1;

                        if (!addedNodesNumber.Contains(nodeNumber))
                        {
                            HandleError(St7.St7SetNodeXYZ(modelId, nodeNumber, new double[] { node.Position.X, node.Position.Y, node.Position.Z }));
                            addedNodesNumber.Add(nodeNumber);
                        }
                        if (node.Id != ModelObjectId.UNASSIGNED)
                            if (St7.St7SetNodeID(modelId, nodeNumber, node.Id) != 0)
                                errors.Add(string.Format($"Failed to set Node ID {node.Id} on node {nodeNumber}."));

                        if (node.Support.IsActive)
                        {
                            try
                            {
                                int[] sts = new int[6];
                                sts[0] |= node.Support.Dx ? St7.btTrue : St7.btFalse;
                                sts[1] |= node.Support.Dy ? St7.btTrue : St7.btFalse;
                                sts[2] |= node.Support.Dz ? St7.btTrue : St7.btFalse;
                                sts[3] |= node.Support.Mx ? St7.btTrue : St7.btFalse;
                                sts[4] |= node.Support.My ? St7.btTrue : St7.btFalse;
                                sts[5] |= node.Support.Mz ? St7.btTrue : St7.btFalse;

                                double[] doubles = node.PrescribedDisplacement;

                                HandleError(St7.St7SetNodeRestraint6(modelId, nodeNumber, 1, 1, sts, doubles));
                            }
                            catch (Exception ex)
                            {
                                errors.Add(string.Format($"Failed to set Node Restraint on {node.Id}. {ex.Message}"));
                                continue;
                            }
                        }

                        foreach (var caseLoads in node.NodalLoadList.GroupBy(load => load.LoadCase.Name))
                        {
                            int lc = loadCaseNameIdMap[caseLoads.Key];
                            HandleError(St7.St7SetNodeForce3(modelId, nodeNumber, lc, new[] { caseLoads.Sum(x => x.FX), caseLoads.Sum(x => x.FY), caseLoads.Sum(x => x.FZ) }));
                            HandleError(St7.St7SetNodeMoment3(modelId, nodeNumber, lc, new[] { caseLoads.Sum(x => x.MX), caseLoads.Sum(x => x.MY), caseLoads.Sum(x => x.MZ) }));
                        }
                        AdvancedExport.Node(modelId, nodeNumber, node, loadCaseNameIdMap);
                        if(node.Groups.Count>0)HandleError(St7.St7SetEntityGroup(modelId,St7.tyNODE,nodeNumber,groupsIds[node.Groups[0]]));

                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format($"Failed to create node {node.Id}. {ex.Message}"));
                        continue;
                    }
                }

                HashSet<int> addedBeamsId = new HashSet<int>();

                for (int k = 0; k < Model.FrameElements.Count; k++)
                {
                    FrameElementModel beam = Model.FrameElements.ElementAt(k).Value;
                    try
                    {
                        int beamNumber = k + 1;
                        int propNumber = beamProperties[beam.FrameProperty.Name];

                        if (!addedBeamsId.Contains(beamNumber))
                        {
                            try
                            {
                                if (St7.St7SetElementConnection(modelId, St7.tyBEAM, beamNumber, propNumber, beamsNodeNumberMap[k]) != 0)
                                {
                                    errors.Add(string.Format($"Failed to set the connection of the beam {beam.Id} with nodes {beam.NodeStart.Id} and {beam.NodeEnd.Id}"));
                                    continue;
                                }

                                if (beam.Id != ModelObjectId.UNASSIGNED)
                                    if (St7.St7SetBeamID(modelId, beamNumber, beam.Id) != 0)
                                        errors.Add(string.Format($"Failed to set the ID {beam.Id} of the beam {beamNumber}."));

                                if (beam.Groups.Count > 0)
                                {
                                    if (HandleError(St7.St7SetEntityGroup(modelId, St7.tyBEAM, beamNumber, groupsIds[beam.Groups[0]])))
                                        errors.Add(string.Format("Failed to set the group {0} to the beam {1}\r\n", beam.Groups[0], id));
                                    if (beam.Groups.Count > 1)
                                        warnings.Add(string.Format("Straus7 allows assigning only one single group to the beam {0}\r\n", id));
                                }

                                AdvancedExport.AlignBeam(modelId,beamNumber,beam);

                                var effectiveOffset = beam.GetEffectiveOffset();
                                HandleError(St7.St7SetBeamOffset2(modelId, beamNumber, new[] { effectiveOffset.X, effectiveOffset.Y }));

                                addedBeamsId.Add(beamNumber);
                            }
                            catch (Exception ex)
                            {
                                errors.Add(string.Format($"Failed to create geometry of beam {beam.Id}. {ex.Message}"));
                                continue;
                            }
                        }

                        AdvancedExport.FrameAttributes(modelId, beamNumber, beam);
                        List<int> preLoadLC = new List<int>();
                        Dictionary<int, int> pointFPId = new Dictionary<int, int>();
                        Dictionary<int, int> pointFGId = new Dictionary<int, int>();
                        Dictionary<int, int> pointMPId = new Dictionary<int, int>();
                        Dictionary<int, int> pointMGId = new Dictionary<int, int>();
                        Dictionary<int, int> distrFPId = new Dictionary<int, int>();
                        Dictionary<int, int> distrFGId = new Dictionary<int, int>();

                        for (int j = 0; j < beam.FrameLoadList.Count; j++)
                        {
                            FrameLoadBaseModel load = beam.FrameLoadList[j];
                            try
                            {
                                int lc = loadCaseNameIdMap[load.LoadCase.Name];

                                if (AdvancedExport.FrameLoad(modelId, beamNumber, lc, j + 1, load)) continue;
                                if (load.GetType() == typeof(FrameLoadStrausModel))
                                {
                                    FrameLoadStrausModel dl = (FrameLoadStrausModel)load;
                                    int type = 0;
                                    int loadId;
                                    switch (dl.LoadSchema)
                                    {
                                        case FrameLoadStrausModel.LoadSchemas.Uniform:
                                            type = St7.dlConstant;
                                            break;
                                        case FrameLoadStrausModel.LoadSchemas.Keystone:
                                            type = St7.dlLinear;
                                            break;
                                        case FrameLoadStrausModel.LoadSchemas.Triangular:
                                            type = St7.dlTriangular;
                                            break;
                                        case FrameLoadStrausModel.LoadSchemas.TriangularEnd1_Keystone:
                                            type = St7.dlThreePoint0;
                                            break;
                                        case FrameLoadStrausModel.LoadSchemas.Keystone_TriangularEnd2:
                                            type = St7.dlThreePoint1;
                                            break;
                                        case FrameLoadStrausModel.LoadSchemas.TriangularEnds_Keystone:
                                            type = St7.dlTrapezoidal;
                                            break;
                                    }
                                    if (dl.CoordinateSystem == CoordinateSystemModel.Local)
                                    {
                                        int dir = (int)dl.Direction + 1;
                                        if (distrFPId.TryGetValue(lc, out loadId))
                                        {
                                            loadId++;
                                            distrFPId[lc] = loadId;
                                        }
                                        else
                                        {
                                            loadId = 1;
                                            distrFPId.Add(lc, loadId);
                                        }
                                        if (dl.LoadType == FrameLoadStrausModel.FrameLoadTypes.Force)
                                            HandleError(St7.St7SetBeamDistributedForcePrincipal6ID(modelId, beamNumber, dir, lc, type, loadId, BeamLoadDistribution.NativeValues(dl)));
                                        else
                                            HandleError(St7.St7SetBeamDistributedMomentPrincipal6ID(modelId, beamNumber, dir, lc, type, loadId, BeamLoadDistribution.NativeValues(dl)));
                                    }
                                    else
                                    {
                                        int dir = 0;
                                        switch (dl.Direction)
                                        {
                                            case FrameLoadStrausModel.LoadDirections.X:
                                            case FrameLoadStrausModel.LoadDirections.X_Projected:
                                                dir = 1;
                                                break;
                                            case FrameLoadStrausModel.LoadDirections.Y:
                                            case FrameLoadStrausModel.LoadDirections.Y_Projected:
                                                dir = 2;
                                                break;
                                            case FrameLoadStrausModel.LoadDirections.Z:
                                            case FrameLoadStrausModel.LoadDirections.Z_Projected:
                                            case FrameLoadStrausModel.LoadDirections.Gravity:
                                            case FrameLoadStrausModel.LoadDirections.Gravity_Projected:
                                                dir = 3;
                                                break;
                                        }
                                        int projectFlag = dl.IsProjected ? St7.btTrue : St7.btFalse;
                                        switch (dl.Direction)
                                        {
                                            case FrameLoadStrausModel.LoadDirections.X:
                                            case FrameLoadStrausModel.LoadDirections.Y:
                                            case FrameLoadStrausModel.LoadDirections.Z:
                                            case FrameLoadStrausModel.LoadDirections.Gravity:
                                                projectFlag = dl.IsProjected ? St7.btTrue : St7.btFalse;
                                                break;
                                            case FrameLoadStrausModel.LoadDirections.X_Projected:
                                            case FrameLoadStrausModel.LoadDirections.Y_Projected:
                                            case FrameLoadStrausModel.LoadDirections.Z_Projected:
                                            case FrameLoadStrausModel.LoadDirections.Gravity_Projected:
                                                projectFlag = St7.btTrue;
                                                break;
                                        }
                                        double[] values = BeamLoadDistribution.NativeValues(dl);
                                        if (dl.Direction == FrameLoadStrausModel.LoadDirections.Gravity ||
                                            dl.Direction == FrameLoadStrausModel.LoadDirections.Gravity_Projected)
                                        {
                                            values[0] = -dl.PA != 0 ? -dl.PA : 0;
                                            values[1] = -dl.PB != 0 ? -dl.PB : 0;
                                            values[2] = -dl.P1 != 0 ? -dl.P1 : 0;
                                            values[3] = -dl.P2 != 0 ? -dl.P2 : 0;
                                        }

                                        if (distrFGId.TryGetValue(lc, out loadId))
                                        {
                                            loadId++;
                                            distrFGId[lc] = loadId;
                                        }
                                        else
                                        {
                                            loadId = 1;
                                            distrFGId.Add(lc, loadId);
                                        }

                                        HandleError(St7.St7SetBeamDistributedForceGlobal6ID(modelId, beamNumber, dir, projectFlag, lc, type, loadId, values));
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                errors.Add(string.Format($"Failed to set load case {load.LoadCase.Id} to beam {beam.Id}. {ex.Message}"));
                                continue;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format($"Failed to create beam {beam.Id}. {ex.Message}"));
                        continue;
                    }
                }

                HashSet<int> addedPlatesNumbers = new HashSet<int>();
                for (int k = 0; k < Model.AreaElements.Count; k++)
                {
                    var plate = Model.AreaElements.ElementAt(k).Value;
                    try
                    {
                        int plateNumber = k + 1;

                        if (!addedPlatesNumbers.Contains(plateNumber))
                        {
                            try
                            {
                                int propId = plateProperties[plate.AreaProperty.Name];
                                if (HandleError(St7.St7SetElementConnection(modelId, St7.tyPLATE, plateNumber, propId, plateNodeNumberMap[k])))
                                {
                                    errors.Add($"Failed to define the plate {id}");
                                    continue;
                                }
                                if (St7.St7SetPlateID(modelId, plateNumber, plate.Id) != 0)
                                    errors.Add($"Failed to set the ID to the plate {plate.Id}");

                                if (plate.Groups.Count > 0)
                                {
                                    if (HandleError(St7.St7SetEntityGroup(modelId, St7.tyPLATE, plateNumber, groupsIds[plate.Groups[0]])))
                                        errors.Add($"Failed to set the group {plate.Groups[0]} to the plate {id}");
                                    if (plate.Groups.Count > 1)
                                        warnings.Add($"Straus7 allows assigning only one single group to the beam {id}");
                                }
                                if (plate.Angle != 0)
                                {
                                    HandleError(St7.St7SetPlateXAngle1(modelId, plateNumber, new[] { Rhino.RhinoMath.ToDegrees(plate.Angle) }));
                                }
                                if (plate.Offset + plate.AreaThickness.Offset != 0)
                                {
                                    if (HandleError(St7.St7SetPlateOffset1(modelId, plateNumber, new double[] { plate.Offset + plate.AreaThickness.Offset })))
                                        errors.Add($"Failed to set the offset to the plate {id}");
                                }

                                addedPlatesNumbers.Add(plateNumber);
                            }
                            catch (Exception ex)
                            {
                                errors.Add(string.Format($"Failed to create geometry of plate {plate.Id}. {ex.Message}"));
                                continue;
                            }
                        }

                        for (int i = 0; i < plate.AreaLoadList.Count; i++)
                        {
                            AreaLoadBaseModel load = plate.AreaLoadList[i];

                            {
                                int lc = loadCaseNameIdMap[load.LoadCase.Name];

                                if (AdvancedExport.AreaLoad(modelId, plateNumber, lc, load)) continue;
                                if (load is AreaPressureLoadModel pp)
                                {
                                    if (St7.St7SetPlateNormalPressure2(modelId, plateNumber, lc, new double[] { pp.Value, 0.0 }) != 0)
                                        errors.Add(string.Format($"Failed to set load case {lc} on plate {plate.Id}."));
                                }
                                if (load is AreaGlobalPressureStraus gp)
                                {
                                    int projectFlag = gp.Projected ? St7.btTrue : St7.btFalse;
                                    int face = gp.Face == AreaGlobalPressureStraus.LoadFace.Top ? St7.psPlatePlusZ : St7.psPlateMinusZ;
                                    HandleError(St7.St7SetPlateGlobalPressure3S(modelId, plateNumber, face, projectFlag, lc, new double[] { gp.Value.X, gp.Value.Y, gp.Value.Z }));

                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format($"Failed to create plate {plate.Id}. {ex.Message}"));
                        continue;
                    }
                }

                #endregion

                AdvancedExport.Solids(modelId, Model, nodePositionMap, loadCaseNameIdMap, groupsIds);
                AdvancedExport.Links(modelId, Model, nodePositionMap, groupsIds);
                if (errors.Count > 0) return false;
                session.Save();
                session.Close();
                File.Move(stagingPath, outputPath, overwrite);
                stagingPath = null;
                return true;
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                return false;
            }
            finally
            {
                try { session?.Dispose(); }
                catch (Exception e) { errors.Add("Close model: " + e.Message); }
                if (stagingPath != null && File.Exists(stagingPath)) File.Delete(stagingPath);
            }
        }

        private static bool HandleError(int code)
        {
            StrausRuntime.Check(code, "Export Straus7 model");
            return false;
        }



    }
}
