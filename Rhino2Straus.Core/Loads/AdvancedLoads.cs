using Rhino.Geometry;
using Rhino2Straus.Core.Attributes;
using Rhino2Straus.Core.Cases;

namespace Rhino2Straus.Core.Loads;

public class FramePointLoadModel : FrameLoadBaseModel
{
    public Vector3d Value { get; set; }
    public double Position { get; set; }
    public bool Moment { get; set; }
}

public class FrameTemperatureGradientModel : FrameLoadBaseModel
{
    public double Gradient1 { get; set; }
    public double Gradient2 { get; set; }
}

public class FramePreloadModel : FrameLoadBaseModel
{
    public double Value { get; set; }
    public bool Strain { get; set; }
}

public class AreaTemperatureGradientModel : AreaLoadBaseModel
{
    public double Gradient { get; set; }
}

public class AreaPreloadModel : AreaLoadBaseModel
{
    public Vector3d Value { get; set; }
    public bool Strain { get; set; }
}

public class AreaEdgePressureModel : AreaLoadBaseModel
{
    public int Edge { get; set; }
    public double Value { get; set; }
}

public sealed class NodeTemperatureModel
{
    public LoadCaseModel LoadCase { get; set; }
    public double Temperature { get; set; }
    /// <summary>Native type: 0 reference, 1 fixed, 2 initial. Null retains native default; table temperatures are unsupported.</summary>
    public int? Type { get; set; }
}
