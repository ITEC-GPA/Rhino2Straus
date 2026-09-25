using Rhino.Geometry;
using Rhino2Straus.Core.Base;
using System;
using System.Collections.Generic;

namespace Rhino2Straus.Core.ElementProperties
{
    public class FramePropertyModel : ModelObjectId, IEquatable<FramePropertyModel>
    {
        public enum Behaviours { Beam, Truss, Cable }
        public Behaviours Behaviour { get; set; }
        public MaterialModel Material { get; set; }
        public FrameSectionModel Section { get; set; }

        public FrameSectionModel.SectionGeometryTypes SectionGeometryType => Section.SectionGeometryType;
        public FrameSectionModel.Types Type => Section.Type;
        public FrameSectionModel.OffsetTypes Offset => Section.Offset;

        public double Dimension1 => Section.Dimension1;
        public double Dimension2 => Section.Dimension2;
        public double Dimension3 => Section.Dimension3;
        public double Dimension4 => Section.Dimension4;
        public double Dimension5 => Section.Dimension5;
        public double Dimension6 => Section.Dimension6;
        public double Dimension7 => Section.Dimension7;
        public double Dimension8 => Section.Dimension8;
        public double Dimension9 => Section.Dimension9;
        public double Dimension10 => Section.Dimension10;

        public double SectionArea => Section.SectionArea;
        public double Ixx => Section.Ixx;
        public double Iyy => Section.Iyy;
        public double Ixy => Section.Ixy;
        public double I11 => Section.I11;
        public double I22 => Section.I22;
        public double J => Section.J;
        public double ShearL1 => Section.ShearL1;
        public double ShearL2 => Section.ShearL2;
        public double ShearA1 => Section.ShearA1;
        public double ShearA2 => Section.ShearA2;
        public double Angle => Section.Angle;
        public Point2d Centroid => Section.Centroid;

        public List<Brep> Breps => Section.Breps;

        public FramePropertyModel(string name, MaterialModel material, FrameSectionModel section)
            : base(name)
        {
            Material = material;
            Section = section;
        }

        public FramePropertyModel(FramePropertyModel beamPropertyModel)
            : this(beamPropertyModel.Name, beamPropertyModel.Material, beamPropertyModel.Section)
        {
            Id = beamPropertyModel.Id;
            Behaviour = beamPropertyModel.Behaviour;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FramePropertyModel);
        }

        public bool Equals(FramePropertyModel other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   Name == other.Name && Behaviour == other.Behaviour &&
                   EqualityComparer<MaterialModel>.Default.Equals(Material, other.Material) &&
                   EqualityComparer<FrameSectionModel>.Default.Equals(Section, other.Section);
        }

        public override int GetHashCode()
        {
            int hashCode = -1663097784;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<MaterialModel>.Default.GetHashCode(Material);
            hashCode = hashCode * -1521134295 + EqualityComparer<FrameSectionModel>.Default.GetHashCode(Section);
            return hashCode;
        }

        public static bool operator ==(FramePropertyModel left, FramePropertyModel right)
        {
            return EqualityComparer<FramePropertyModel>.Default.Equals(left, right);
        }

        public static bool operator !=(FramePropertyModel left, FramePropertyModel right)
        {
            return !(left == right);
        }
    }
}
