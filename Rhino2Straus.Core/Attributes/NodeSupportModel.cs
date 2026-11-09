namespace Rhino2Straus.Core.Attributes
{
    public class NodeSupportModel
    {
        public bool Dx { get; set; }

        public bool Dy { get; set; }

        public bool Dz { get; set; }

        public bool Mx { get; set; }

        public bool My { get; set; }

        public bool Mz { get; set; }

        public bool IsActive => Dx || Dy || Dz || Mx || My || Mz;

        public BoundaryGroupModel BoundaryGroup { get; set; }

        public bool HasBoundaryGroup => BoundaryGroup != null;

        public NodeSupportModel(bool dx, bool dy, bool dz, bool mx, bool my, bool mz)
        {
            Dx = dx;
            Dy = dy;
            Dz = dz;
            Mx = mx;
            My = my;
            Mz = mz;
        }

        public NodeSupportModel()
        {
        }


        public NodeSupportModel(NodeSupportModel nodeSupportModel)
        {
            Dx = nodeSupportModel.Dx;
            Dy = nodeSupportModel.Dy;
            Dz = nodeSupportModel.Dz;
            Mx = nodeSupportModel.Mx;
            My = nodeSupportModel.My;
            Mz = nodeSupportModel.Mz;
            BoundaryGroup = nodeSupportModel.BoundaryGroup == null ? null : new BoundaryGroupModel(nodeSupportModel.BoundaryGroup);
        }
    }
}
