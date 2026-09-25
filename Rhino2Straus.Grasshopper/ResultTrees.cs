using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino2Straus.Core.Models;

namespace Rhino2Straus.Grasshopper;

internal static class ResultTrees
{
    public static void Write(IGH_DataAccess da, IEnumerable<ElementResult> records)
    {
        var values = new GH_Structure<GH_Number>(); var positions = new GH_Structure<GH_Number>();
        var columns = new GH_Structure<GH_String>(); var metadata = new GH_Structure<GH_String>();
        foreach (var r in records)
        {
            var path = new GH_Path(r.CaseNumber, r.EntityId, r.Surface);
            for (int row = 0; row < r.Values.Count; row++)
                values.AppendRange(r.Values[row].Select(v => new GH_Number(v)), path.AppendElement(row));
            positions.AppendRange(r.Positions.Select(v => new GH_Number(v)), path);
            columns.AppendRange(r.Columns.Select(v => new GH_String(v)), path);
            metadata.AppendRange(new[] { new GH_String(r.Axes), new GH_String(r.Sampling) }, path);
        }
        da.SetDataTree(0, values); da.SetDataTree(1, positions); da.SetDataTree(2, columns); da.SetDataTree(3, metadata);
        da.SetDataList(4, records.Select(r => new GH_ObjectWrapper(r)));
    }
}
