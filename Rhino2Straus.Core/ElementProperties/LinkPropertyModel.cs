using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Base;

namespace Rhino2Straus.Core.ElementProperties
{
    public class LinkPropertyModel : ModelObjectId
    {
        public enum LinkPropertyTypes
        {
            RIGID,
            GEN,
            PINNED,
            MASTER_SLAVE,
            SECTOR_SYMMETRY,
        }

        public LinkPropertyTypes Type { get; set; }
        public LinkConstraintOptions Options { get; set; } = new();
        public bool IsNativeLink => Type != LinkPropertyTypes.GEN;

        public double Kx { get; set; }

        public double Ky { get; set; }

        public double Kz { get; set; }

        public double Rx { get; set; }

        public double Ry { get; set; }

        public double Rz { get; set; }

        public BoundaryGroupModel BoundaryGroup { get; set; }

        public LinkPropertyModel(LinkPropertyTypes type, double kx = 0.0, double ky = 0.0,
            double kz = 0.0, double rx = 0.0, double ry = 0.0, double rz = 0.0)
            : base()
        {
            Type = type;
            Kx = kx;
            Ky = ky;
            Kz = kz;
            Rx = rx;
            Ry = ry;
            Rz = rz;
        }

        public LinkPropertyModel()
            : base()
        {
        }

        public LinkPropertyModel(LinkPropertyModel framePropertyModel)
            : base(framePropertyModel.Id, framePropertyModel.Name)
        {
            Type = framePropertyModel.Type;
            Options = framePropertyModel.Options.Copy();
            Kx = framePropertyModel.Kx;
            Ky = framePropertyModel.Ky;
            Kz = framePropertyModel.Kz;
            Rx = framePropertyModel.Rx;
            Ry = framePropertyModel.Ry;
            Rz = framePropertyModel.Rz;
            BoundaryGroup = framePropertyModel.BoundaryGroup == null ? null : new BoundaryGroupModel(framePropertyModel.BoundaryGroup);
        }
    }
}
