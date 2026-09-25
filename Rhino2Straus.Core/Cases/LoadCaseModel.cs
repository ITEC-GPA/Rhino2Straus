using System.ComponentModel;
using Rhino2Straus.Core.Base;

namespace Rhino2Straus.Core.Cases
{
    public class LoadCaseModel : LoadCaseBase
    {
        public enum LoadCaseTypes
        {
            [Description("User Defined Load(USER)")] USER,
            [Description("Dead Load(D)")] D,
            [Description("Live Load(L)")] L,
            [Description("Roof Live Load(LR)")] LR,
            [Description("Wind Load on Structure(W)")] W,
            [Description("Across Wind Load(WA)")] WA,
            [Description("Torsional Wind Load(WT)")] WT,
            [Description("Earthquake(E)")] E,
            [Description("Vertical Earthquake(EVT)")] EVT,
            [Description("Snow Load(S)")] S,
            [Description("Rain Load(R)")] R,
            [Description("Ice Pressure(IP)")] IP,
            [Description("Earth Pressure(EP)")] EP,
            [Description("Horizontal Earth Pressure(EH)")] EH,
            [Description("Vertical Earth Pressure(EV)")] EV,
            [Description("Ground Water Pressure(WP)")] WP,
            [Description("Fluid Pressure(FP)")] FP,
            [Description("Stream Flow Pressure(SF)")] SF,
            [Description("Buoyancy(B)")] B,
            [Description("Creep(CR)")] CR,
            [Description("Shrinkage(SH)")] SH,
            [Description("Temperature(T)")] T,
            [Description("Prestress(PS)")] PS,
            [Description("Construction Stage Load(CS)")] CS,
            [Description("Erection Load(ER)")] ER,
            [Description("Live Load Impact(IL)")] IL,
            [Description("Longitudinal Force from Live Load (BK)")] BK,
            [Description("Wind Load on Live Load(WL)")] WL,
            [Description("Centrifugal Force(CF)")] CF,
            [Description("Collision Load(CO)")] CO,
            [Description("Rib Shortening(RS)")] RS,
            [Description("Explosion Load(EX)")] EX,
            [Description("Imperfection Load(I)")] I,
            [Description("Earthquake for Elastic(EE)")] EE,
        }

        public LoadCaseTypes Type { get; set; }
        public bool GravityNoStructuralMass { get; set; }

        public LoadCaseModel(string name, LoadCaseTypes type = LoadCaseTypes.USER, string description = "", bool gravityNSM = false)
            : base(name, description)
        {
            Type = type;
            Description = description;
            GravityNoStructuralMass = gravityNSM;
        }

        public LoadCaseModel()
            : base()
        {
        }

        public LoadCaseModel(LoadCaseModel loadCaseModel)
            : base(loadCaseModel.Name, loadCaseModel.Description)
        {
            Type = loadCaseModel.Type;
            Id = loadCaseModel.Id;
            GravityNoStructuralMass = loadCaseModel.GravityNoStructuralMass;
        }
    }
}
